using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    private void Start()
    {
        offset = transform.position;
    }

    void Update()
    {
        transform.position = target.position + offset;
    }
}
