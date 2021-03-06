using UnityEngine;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

public class NetworkPlayerManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    public UnityClient client;

    private Dictionary<ushort, AgarObject> networkPlayers = new Dictionary<ushort, AgarObject>();

    private void Awake()
    {
        this.client.MessageReceived += MessageReceived;
    }

    public void Add(ushort id, AgarObject player)
    {
        this.networkPlayers.Add(id, player);
    }

    private void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == Tags.MovePlayerTag)
            {
                this.MovePlayer(message);
            }
            else if (message.Tag == Tags.SetRadiusTag)
            {
                this.SetRadius(message);
            }
        }
    }

    private void MovePlayer(Message message)
    {
        using (DarkRiftReader reader = message.GetReader())
        {
            ushort id = reader.ReadUInt16();
            Vector3 newPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);

            if (this.networkPlayers.ContainsKey(id))
            {
                this.networkPlayers[id].SetMovePosition(newPosition);
            }
        }
    }

    private void SetRadius(Message message)
    {
        using (DarkRiftReader reader = message.GetReader())
        {
            ushort id = reader.ReadUInt16();

            if (this.networkPlayers.ContainsKey(id))
            {
                this.networkPlayers[id].SetRadius(reader.ReadSingle());
            }
        }
    }

    public void DestroyPlayer(ushort id)
    {
        AgarObject o = this.networkPlayers[id];

        Destroy(o.gameObject);

        this.networkPlayers.Remove(id);
    }
}