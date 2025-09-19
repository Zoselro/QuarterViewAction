using UnityEngine;

public class Orbit : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float orbitSpeed;
    private Vector3 offset;
    void Start()
    {
        offset = transform.position - target.position;
    }
    void Update()
    {
        transform.position = target.position + offset;
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);

        offset = transform.position - target.position;
    }
}
