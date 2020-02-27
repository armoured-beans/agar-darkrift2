using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;

namespace AgarPlugin
{
    class AgarFoodManager : Plugin
    {
        Dictionary<int, FoodItem> foodItems = new Dictionary<int, FoodItem>();

        const int NUM_FOOD = 20;
        private ushort nextFoodId = 0;
        public override bool ThreadSafe => false;

        public override Version Version => new Version(0, 1, 0);

        public AgarFoodManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            // Spawn initial food
            for (int i = 0; i < NUM_FOOD; i++)
            {
                this.SpawnFood();
            }

            ClientManager.ClientConnected += ClientConnected;
        }

        private void SpawnFood()
        {
            Random r = new Random();
            FoodItem foodItem = new FoodItem(
                this.nextFoodId++,
                (float)r.NextDouble() * AgarPlayerManager.MAP_WIDTH - AgarPlayerManager.MAP_WIDTH / 2,
                (float)r.NextDouble() * AgarPlayerManager.MAP_WIDTH - AgarPlayerManager.MAP_WIDTH / 2,
                (byte)r.Next(0, 200),
                (byte)r.Next(0, 200),
                (byte)r.Next(0, 200)
            );

            this.foodItems.Add(foodItem.ID, foodItem);
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            // Send all food items to player
            using (DarkRiftWriter foodItemWriter = DarkRiftWriter.Create())
            {
                foreach (FoodItem foodItem in foodItems.Values)
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
    }
}