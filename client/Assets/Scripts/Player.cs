using UnityEngine;
using DarkRift;
using DarkRift.Client.Unity;

public class Player : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The distance we can move before we send a position update.")]
    float moveDistance = 0.05f;

    public UnityClient Client { get; set; }

    Vector3 lastPosition;

    private void Awake()
    {
        this.lastPosition = transform.position;
    }

    private void Update()
    {
        if (Vector3.Distance(this.lastPosition, this.transform.position) > this.moveDistance)
        {
            this.lastPosition = this.transform.position;
        }

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(this.transform.position.x);
            writer.Write(this.transform.position.y);

            using (Message message = Message.Create(Tags.MovePlayerTag, writer))
            {
                Client.SendMessage(message, SendMode.Unreliable);
            }
        }
    }
}