using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    public float speed = 5f;

    public Transform Target { get; set; }

    private void Update()
    {
        if (Target != null)
        {
            Vector3 targetPos = Target.GetComponent<Renderer>().bounds.center;
            this.transform.position = Vector3.Lerp(
                this.transform.position,
                new Vector3(targetPos.x, targetPos.y, this.transform.position.z),
                this.speed * Time.deltaTime
            );
        }
    }
}