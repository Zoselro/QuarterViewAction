using UnityEngine;

public class FollowCamera : MonoBehaviour
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
