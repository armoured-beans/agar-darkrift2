using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    [SerializeField]
    [Tooltip("The controllable player prefab.")]
    GameObject controllablePrefab;

    [SerializeField]
    [Tooltip("The network controllable player prefab.")]
    GameObject networkPrefab;

    [SerializeField]
    [Tooltip("The network player manager.")]
    NetworkPlayerManager networkPlayerManager;

    private void Awake()
    {
        if (this.client == null)
        {
            Debug.LogError("Client unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (this.controllablePrefab == null)
        {
            Debug.LogError("Controllable Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (this.networkPrefab == null)
        {
            Debug.LogError("Network Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        client.MessageReceived += MessageReceived;
    }
    private void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == Tags.SpawnPlayerTag)
            {
                SpawnPlayer(sender, e);
            }
            else if (message.Tag == Tags.DespawnPlayerTag)
            {
                DespawnPlayer(sender, e);
            }
        }
    }

    private void DespawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                this.networkPlayerManager.DestroyPlayer(reader.ReadUInt16());
            }
        }
    }

    private void SpawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                if (message.Tag == Tags.SpawnPlayerTag)
                {
                    if (reader.Length % 17 != 0)
                    {
                        Debug.LogWarning("Received malformed spawn packet.");
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
                        if (id == client.ID)
                        {
                            obj = Instantiate(this.controllablePrefab, position, Quaternion.identity) as GameObject;

                            Player player = obj.GetComponent<Player>();
                            player.Client = client;
                        }
                        else
                        {
                            obj = Instantiate(this.networkPrefab, position, Quaternion.identity) as GameObject;
                        }

                        AgarObject agarObj = obj.GetComponent<AgarObject>();

                        agarObj.SetRadius(radius);
                        agarObj.SetColor(color);
                        this.networkPlayerManager.Add(id, agarObj);
                    }
                }
            }
        }
    }
}