using UnityEngine;
using UnityEngine.Pool;

public class EnemyBulletObejctPool : MonoBehaviour
{
    public static EnemyBulletObejctPool Instance = null;

    private IObjectPool<Bullet> enemyCBulletPool;
    private IObjectPool<BossMissile> bossBulletPool;
    private IObjectPool<BossRock> bossRockPool;

    [SerializeField] private Bullet enemyCBulletPrefab;
    [SerializeField] private BossMissile bossBulletPrefab;
    [SerializeField] private BossRock bossRockPrefab;

    private void Awake()
    {
        Instance = this;

        enemyCBulletPool = new ObjectPool<Bullet>(
            createFunc: () =>
            {
                Bullet newObj = Instantiate(enemyCBulletPrefab);
                newObj.gameObject.SetActive(false);
                return newObj;
            },
            actionOnGet: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(true);
            },
            actionOnRelease: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(false);
            },
            maxSize: 10
        );

        bossBulletPool = new ObjectPool<BossMissile>(
            createFunc: () =>
            {
                BossMissile newObj = Instantiate(bossBulletPrefab);
                newObj.gameObject.SetActive(false);
                return newObj;
            },
            actionOnGet: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(true);
            },
            actionOnRelease: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(false);
            },
            maxSize: 10
        );

        bossRockPool = new ObjectPool<BossRock>(
            createFunc: () =>
            {
                BossRock newObj = Instantiate(bossRockPrefab);
                newObj.gameObject.SetActive(false);
                return newObj;
            },
            actionOnGet: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(true);
            },
            actionOnRelease: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(false);
            },
            maxSize: 10
        );
    }

    public void ReturnenemyCBulletPool(Bullet bullet)
    {
        Instance.enemyCBulletPool.Release(bullet);
    }

    public Bullet GetEnemyCBulletPool()
    {
        Bullet bullet = Instance.enemyCBulletPool.Get();
        return bullet;
    }

    public void ReturnBossBullet(BossMissile bullet)
    {
        Instance.bossBulletPool.Release(bullet);
    }

    public BossMissile GetBossBulletPool()
    {
        BossMissile bullet = Instance.bossBulletPool.Get();
        return bullet;
    }

    public void ReturnBossRock(BossRock bossRock)
    {
        Instance.bossRockPool.Release(bossRock);
    }

    public BossRock GetBossRockPool()
    {
        BossRock bossRock = Instance.bossRockPool.Get();
        return bossRock;
    }
}
