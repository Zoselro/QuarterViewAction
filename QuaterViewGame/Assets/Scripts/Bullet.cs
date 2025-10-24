using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private bool isMelee; // 근접으로 공격하는 몬스터인가?

    protected bool isRock;

    private void OnCollisionEnter(Collision collision)
    {
        if (!isRock && collision.gameObject.tag == "Floor")
        {
            Destroy(gameObject, 3);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        /*if (!isMelee && (other.gameObject.tag == "Wall" || other.gameObject.tag == "Floor"))
        {
            Destroy(gameObject);
        }*/
        if (!isMelee)
        {
            if(other.gameObject.tag == "Wall")
            {
                Destroy(gameObject);
                return;
            }
            else if(other.gameObject.tag == "Floor")
            {
                if (isRock)
                {
                    return;
                }
                Destroy(gameObject);
            }
        }
    }

    public int GetDamage()
    {
        return damage;
    }
}
