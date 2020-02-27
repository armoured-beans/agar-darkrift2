using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

public class FoodManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    public UnityClient client;

    [SerializeField]
    [Tooltip("The food prefab.")]
    public GameObject foodPrefab;

    private Dictionary<ushort, AgarObject> networkFoodItems = new Dictionary<ushort, AgarObject>();

    private void Awake()
    {
        this.client.MessageReceived += MessageReceived;
    }

    public void Add(ushort id, AgarObject foodItem)
    {
        this.networkFoodItems.Add(id, foodItem);
    }

    private void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == Tags.FoodSpawnTag)
            {
                this.SpawnFood(message);
            }
            else if (message.Tag == Tags.MoveFoodTag)
            {
                this.MoveFood(message);
            }
        }
    }

    private void SpawnFood(Message message)
    {
        using (DarkRiftReader reader = message.GetReader())
        {
            if (reader.Length % 17 != 0)
            {
                Debug.LogWarning("Received malformed food spawn packet.");
                return;
            }

            while (reader.Position < reader.Length)
            {
                ushort id = reader.ReadUInt16();
                Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                float radius = reader.ReadSingle();
                Color32 color = new Color32(
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    255
                );

                GameObject obj;
                obj = Instantiate(this.foodPrefab, position, Quaternion.identity) as GameObject;

                AgarObject agarObj = obj.GetComponent<AgarObject>();

                agarObj.SetRadius(radius);
                agarObj.SetColor(color);
                agarObj.SetMovePosition(position);

                this.Add(id, agarObj);
            }
        }
    }

    private void MoveFood(Message message)
    {
        using (DarkRiftReader reader = message.GetReader())
        {
            if (reader.Length % 10 != 0)
            {
                Debug.LogWarning("Received malformed move packet.");
                return;
            }

            ushort id = reader.ReadUInt16();
            Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());

            if (networkFoodItems.ContainsKey(id))
            {
                networkFoodItems[id].transform.position = position;
            }  
        }
    }
}