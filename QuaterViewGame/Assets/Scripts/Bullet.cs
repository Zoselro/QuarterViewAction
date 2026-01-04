using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private bool isMelee; // 근접으로 공격하는 몬스터인가?
    [SerializeField] private bool isCase;
    [SerializeField] private bool isEnemyBullet;

    protected bool isRock;
    private bool isBulletDestroy = false;
    private bool isBulletCaseDestroy = false;


    private void OnCollisionEnter(Collision collision)
    {
        if (!isRock && collision.gameObject.tag == "Floor")
        {
            Invoke("ReleaseToPool", 3f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
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
        if (isCase)
        {
            BulletCaseDestroyAfter(this);
        }
        else
        {
            BulletDestroyAfter(this);
        }
    }

    public void BulletCaseDestroyAfter(Bullet bullet)
    {
        if (isBulletCaseDestroy == true)
            return;
        isBulletCaseDestroy = true;
        BulletObjectPool.ReturnBulletCase(bullet);
    }

    public void SetBulletCaseDestroyFalse()
    {
        isBulletCaseDestroy = false;
    }

    public void BulletDestroyAfter(Bullet bullet)
    {
        if (isBulletDestroy == true)
            return;
        isBulletDestroy = true;
        BulletObjectPool.ReturnBullet(bullet);
    }

    public void SetIsBulletDestroyFalse()
    {
        isBulletDestroy = false;
    }
}
