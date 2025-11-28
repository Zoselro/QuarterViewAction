using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type
    {
        Ammo,
        Coin,
        Grenade,
        Heart,
        Weapon
    };

    [SerializeField] private Type type;
    [SerializeField] private int value;
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private SphereCollider sphereCollider;

    private void Awake()
    {
        speed = 20;
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            rb.isKinematic = true;
            sphereCollider.enabled = false;
        }
    }

    public int GetValue()
    {
        return value;
    }

    public Type GetItemType()
    {
        return type;
    }
}
