using UnityEngine;

[RequireComponent(typeof(AgarObject))]
public class MouseController : MonoBehaviour
{
    AgarObject agarObject;

    private void Awake()
    {
        this.agarObject = GetComponent<AgarObject>();
    }

    private void Update()
    {
        Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePoint.z = 0;

        this.agarObject.SetMovePosition(mousePoint);
    }
}