using UnityEngine;
using UnityEngine.Pool;

public class BulletObjectPool : MonoBehaviour
{
    public static BulletObjectPool Instance = null;

    private IObjectPool<Bullet> bulletPool;
    private IObjectPool<Bullet> bulletCasePool;

    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Bullet bulletCasePrefab;

    private void Awake()
    {
        Instance = this;

        bulletPool = new ObjectPool<Bullet>(
            createFunc: () =>
            {
                Bullet newObj = Instantiate(bulletPrefab, transform);
                newObj.gameObject.SetActive(false);
                return newObj;
            },

            actionOnGet: (b) =>
            {
                b.transform.SetParent(Instance.transform);

                var trail = b.GetComponent<TrailRenderer>();
                trail.Clear();

                b.gameObject.SetActive(true);
            },

            actionOnRelease: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(false);
            },
            maxSize: 30
            );

        bulletCasePool = new ObjectPool<Bullet>(
            createFunc: () =>
            {
                Bullet newObj = Instantiate(bulletCasePrefab, transform);
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
            maxSize: 30
            );

        for (int i = 0; i < 30; i++)
            bulletPool.Release(bulletPool.Get());

        for (int i = 0; i < 30; i++)
            bulletCasePool.Release(bulletCasePool.Get());
    }
    public static void ReturnBullet(Bullet bullet)
    {
        Instance.bulletPool.Release(bullet);
    }

    public static Bullet GetBullet()
    {
        Bullet bullet = Instance.bulletPool.Get();
        return bullet;
    }

    public static void ReturnBulletCase(Bullet bullet)
    {
        Instance.bulletCasePool.Release(bullet);
    }

    public static Bullet GetBulletCase()
    {
        Bullet bullet = Instance.bulletCasePool.Get();
        return bullet;
    }
}
