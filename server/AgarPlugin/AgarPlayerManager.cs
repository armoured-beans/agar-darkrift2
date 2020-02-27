using System;
using System.Linq;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;

namespace AgarPlugin
{
    public class AgarPlayerManager : Plugin
    {
        public const float MAP_WIDTH = 50;
        private Random random;
        public override bool ThreadSafe => false;
        public override Version Version => new Version(0, 1, 0);

        Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();

        public AgarPlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            this.random = new Random();

            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Player newPlayer = new Player(
                e.Client.ID,
                (float)this.random.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,
                (float)this.random.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,
                0.2f,
                (byte)this.random.Next(0, 200),
                (byte)this.random.Next(0, 200),
                (byte)this.random.Next(0, 200)
            );

            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                newPlayerWriter.Write(newPlayer.ID);
                newPlayerWriter.Write(newPlayer.X);
                newPlayerWriter.Write(newPlayer.Y);
                newPlayerWriter.Write(newPlayer.Radius);
                newPlayerWriter.Write(newPlayer.ColorR);
                newPlayerWriter.Write(newPlayer.ColorG);
                newPlayerWriter.Write(newPlayer.ColorB);

                using (Message newPlayerMessage = Message.Create(Tags.SpawnPlayerTag, newPlayerWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                    {
                        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                    }
                }
            }

            this.players.Add(e.Client, newPlayer);

            using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
            {
                foreach (Player player in players.Values)
                {
                    playerWriter.Write(player.ID);
                    playerWriter.Write(player.X);
                    playerWriter.Write(player.Y);
                    playerWriter.Write(player.Radius);
                    playerWriter.Write(player.ColorR);
                    playerWriter.Write(player.ColorG);
                    playerWriter.Write(player.ColorB);
                }

                using (Message playerMessage = Message.Create(Tags.SpawnPlayerTag, playerWriter))
                {
                    e.Client.SendMessage(playerMessage, SendMode.Reliable);
                }
            }

            e.Client.MessageReceived += MovementMessageReceived;
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            this.players.Remove(e.Client);

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(e.Client.ID);

                using (Message message = Message.Create(Tags.DespawnPlayerTag, writer))
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                    {
                        client.SendMessage(message, SendMode.Reliable);
                    }
                }
            }
        }

        private void MovementMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.MovePlayerTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        float newX = reader.ReadSingle();
                        float newY = reader.ReadSingle();

                        Player player = players[e.Client];

                        player.X = newX;
                        player.Y = newY;

                        AgarFoodManager foodManager = PluginManager.GetPluginByType<AgarFoodManager>();

                        foreach (FoodItem food in foodManager.foodItems)
                        {
                            if (Math.Pow(player.X - food.X, 2) + Math.Pow(player.Y - food.Y, 2) < Math.Pow(player.Radius, 2))
                            {
                                player.Radius += food.Radius;
                                this.SendRadiusUpdate(player);
                                foodManager.Eat(food);
                            }
                        }

                        foreach (Player otherPlayer in players.Values.Where(x => x.Alive))
                        {
                            if (otherPlayer != player && Math.Pow(player.X - otherPlayer.X, 2) + Math.Pow(player.Y - otherPlayer.Y, 2) < Math.Pow(player.Radius, 2))
                            {
                                player.Radius += otherPlayer.Radius;
                                this.SendRadiusUpdate(player);
                                this.Kill(otherPlayer);
                            }
                        }

                        using (DarkRiftWriter writer = DarkRiftWriter.Create())
                        {
                            writer.Write(player.ID);
                            writer.Write(player.X);
                            writer.Write(player.Y);
                            message.Serialize(writer);
                        }

                        foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                        {
                            c.SendMessage(message, e.SendMode);
                        }
                    }
                }
            }
        }

        private void SendRadiusUpdate(Player player)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.ID);
                writer.Write(player.Radius);

                using (Message message = Message.Create(Tags.SetRadiusTag, writer))
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                    {
                        client.SendMessage(message, SendMode.Reliable);
                    }
                }
            }
        }

        private void Kill(Player player)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(player.ID);

                using (Message message = Message.Create(Tags.KillPlayerTag, writer))
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                    {
                        client.SendMessage(message, SendMode.Reliable);

                        if(this.players[client].ID == player.ID)
                        {
                            this.players[client].Alive = false;
                        }
                    }
                }            
            }
        }
    }
}
