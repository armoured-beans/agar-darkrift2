using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AgarObject : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The speed that the player will move.")]
    float speed = 1f;

    [SerializeField]
    [Tooltip("Multiplier for the scaling of the player.")]
    float scale = 1f;

    Vector3 movePosition;

    private void Awake()
    {
        this.movePosition = transform.position;
    }

    private void Update()
    {
        if (speed != 0f)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, this.movePosition, this.speed * Time.deltaTime);
        }
    }

    public void SetColor(Color32 color)
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = color;
    }

    public void SetRadius(float radius)
    {
        this.transform.localScale = new Vector3(radius * this.scale, radius * this.scale, 1);
    }

    public void SetMovePosition(Vector3 newPosition)
    {
        this.movePosition = newPosition;
    }
}