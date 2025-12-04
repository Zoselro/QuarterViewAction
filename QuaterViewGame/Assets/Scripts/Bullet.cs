using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private bool isMelee; // 근접으로 공격하는 몬스터인가?
    [SerializeField] private bool isCase;
    [SerializeField] private bool isEnemyBullet;

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
            Invoke("ReleaseToPool", 3f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_released) return;
        if (!isMelee)
        {
            if (other.CompareTag("Wall") || other.CompareTag("Floor"))
            {
                if (!isEnemyBullet)
                {
                    ReleaseToPool();
                }
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
