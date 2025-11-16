using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private bool isMelee; // 근접으로 공격하는 몬스터인가?
    [SerializeField] private bool isCase;

    protected bool isRock;
    private bool _released;

    private void OnEnable()
    {
        _released = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_released)
            return;

        if (!isRock && collision.gameObject.tag == "Floor")
        {
            //Destroy(gameObject, 3);
            Invoke("ReleaseToPool", 3f);
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
                //Destroy(gameObject);
                ReleaseToPool();
            }
            else if(other.gameObject.tag == "Floor")
            {
                if (isRock)
                {
                    return;
                }
                //Destroy(gameObject);
                ReleaseToPool();
            }
        }
    }

    public int GetDamage()
    {
        return damage;
    }
    private void ReleaseToPool()
    {
        if (_released) return;
        _released = true;

        if (isCase)
            BulletObjectPool.ReturnBulletCase(this);
        else
            BulletObjectPool.ReturnBullet(this);
    }
}
