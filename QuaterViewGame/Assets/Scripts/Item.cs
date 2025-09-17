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

    private void Awake()
    {
        speed = 20;
    }

    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime);
    }

    public int GetValue()
    {
        return value;
    }
}
