using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;

namespace AgarPlugin
{
    class AgarFoodManager : Plugin
    {
        const int NUM_FOOD = 20;
        private ushort nextFoodId = 0;
        private Random random;
        public override bool ThreadSafe => false;
        public override Version Version => new Version(0, 1, 0);

        public List<FoodItem> foodItems = new List<FoodItem>();

        public AgarFoodManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            this.random = new Random();

            // Spawn initial food
            for (int i = 0; i < NUM_FOOD; i++)
            {
                this.SpawnFood();
            }

            ClientManager.ClientConnected += ClientConnected;
        }

        private void SpawnFood()
        {
            FoodItem foodItem = new FoodItem(
                this.nextFoodId++,
                (float)this.random.NextDouble() * AgarPlayerManager.MAP_WIDTH - AgarPlayerManager.MAP_WIDTH / 2,
                (float)this.random.NextDouble() * AgarPlayerManager.MAP_WIDTH - AgarPlayerManager.MAP_WIDTH / 2,
                (byte)this.random.Next(0, 200),
                (byte)this.random.Next(0, 200),
                (byte)this.random.Next(0, 200)
            );

            this.foodItems.Add(foodItem);
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            // Send all food items to player
            using (DarkRiftWriter foodItemWriter = DarkRiftWriter.Create())
            {
                foreach (FoodItem foodItem in this.foodItems)
                {
                    foodItemWriter.Write(foodItem.ID);
                    foodItemWriter.Write(foodItem.X);
                    foodItemWriter.Write(foodItem.Y);
                    foodItemWriter.Write(foodItem.Radius);
                    foodItemWriter.Write(foodItem.ColorR);
                    foodItemWriter.Write(foodItem.ColorG);
                    foodItemWriter.Write(foodItem.ColorB);
                }

                using (Message foodItemMessage = Message.Create(Tags.FoodSpawnTag, foodItemWriter))
                {
                    e.Client.SendMessage(foodItemMessage, SendMode.Reliable);
                }
            }
        }

        public void Eat(FoodItem foodItem)
        {
            foodItem.X = (float)this.random.NextDouble() * AgarPlayerManager.MAP_WIDTH - AgarPlayerManager.MAP_WIDTH / 2;
            foodItem.Y = (float)this.random.NextDouble() * AgarPlayerManager.MAP_WIDTH - AgarPlayerManager.MAP_WIDTH / 2;

            using (DarkRiftWriter foodWriter = DarkRiftWriter.Create())
            {
                foodWriter.Write(foodItem.ID);
                foodWriter.Write(foodItem.X);
                foodWriter.Write(foodItem.Y);

                using (Message playerMessage = Message.Create(Tags.MoveFoodTag, foodWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                    {
                        client.SendMessage(playerMessage, SendMode.Reliable);
                    }
                }
            }
        }
    }
}