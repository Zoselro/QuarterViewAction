using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage;

    [SerializeField] private bool isMelee; // 근접으로 공격하는 몬스터인가?

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            Destroy(gameObject, 3);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isMelee && (other.gameObject.tag == "Wall" || other.gameObject.tag == "Floor"))
        {
            Destroy(gameObject);
        }
    }

    public int GetDamage()
    {
        return damage;
    }
}
