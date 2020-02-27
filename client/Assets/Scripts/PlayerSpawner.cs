using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    public UnityClient client;

    [SerializeField]
    [Tooltip("The controllable player prefab.")]
    public GameObject controllablePrefab;

    [SerializeField]
    [Tooltip("The network controllable player prefab.")]
    public GameObject networkPrefab;

    [SerializeField]
    [Tooltip("The network player manager.")]
    public NetworkPlayerManager networkPlayerManager;

    [SerializeField]
    [Tooltip("The CameraFollow script on the Main Camera.")]
    public CameraFollow cameraFollow;

    [SerializeField]
    [Tooltip("The Canvas.")]
    public Transform canvas;

    [SerializeField]
    [Tooltip("The prefab containing game over text.")]
    public GameObject gameOverPrefab;

    private void Awake()
    {
        this.client.MessageReceived += MessageReceived;
    }
    private void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == Tags.SpawnPlayerTag)
            {
                this.SpawnPlayer(message);
            }
            else if (message.Tag == Tags.DespawnPlayerTag)
            {
                this.DespawnPlayer(message);
            }
            else if (message.Tag == Tags.KillPlayerTag)
            {
                this.KillPlayer(message);
            }
        }
    }

    private void KillPlayer(Message message)
    {
        using (DarkRiftReader reader = message.GetReader())
        {
            ushort id = reader.ReadUInt16();
            this.networkPlayerManager.DestroyPlayer(id);

            if (id == this.client.ID)
            {
                Instantiate(this.gameOverPrefab, this.canvas);
            }
        }
    }

    private void DespawnPlayer(Message message)
    {
        using (DarkRiftReader reader = message.GetReader())
        {
            this.networkPlayerManager.DestroyPlayer(reader.ReadUInt16());
        }
    }

    private void SpawnPlayer(Message message)
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
                    if (id == this.client.ID)
                    {
                        obj = Instantiate(this.controllablePrefab, position, Quaternion.identity) as GameObject;

                        Player player = obj.GetComponent<Player>();
                        player.Client = this.client;

                        this.cameraFollow.Target = obj.transform;
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