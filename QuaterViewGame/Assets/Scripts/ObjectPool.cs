using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool bulletSubMachineGun = null;
    public static ObjectPool bulletCase = null;


    private IObjectPool<Bullet> bulletSubMachineGunPool;
    private IObjectPool<GameObject> bulletCasePool; 

    [SerializeField] private Bullet bulletSubMachineGunPrefab;
    [SerializeField] private GameObject bulletCasePrefab;

    private void Awake()
    {
        bulletSubMachineGun = this;

        bulletSubMachineGunPool = new ObjectPool<Bullet>(
            createFunc: () =>
            {
                Bullet newObj = Instantiate(bulletSubMachineGunPrefab, transform);
                newObj.gameObject.SetActive(false);
                return newObj;
            },

            actionOnGet: (b) =>
            {
                b.transform.SetParent(null);
                b.gameObject.SetActive(true);
            },

            actionOnRelease: (b) =>
            {
                b.transform.SetParent(bulletSubMachineGun.transform);
                b.gameObject.SetActive(false);
            },
            maxSize: 10
            );

        bulletCasePool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject newObj = Instantiate(bulletCasePrefab, transform);
                newObj.gameObject.SetActive(false);
                return newObj;
            },
            actionOnGet: (b) =>
            {
                b.transform.SetParent(null);
                b.gameObject.SetActive(true);
            },

            actionOnRelease: (b) =>
            {
                b.transform.SetParent(bulletSubMachineGun.transform);
                b.gameObject.SetActive(false);
            },
            maxSize: 10
            );
    }
    public static void ReturnBullet(Bullet bullet)
    {
        bulletSubMachineGun.bulletSubMachineGunPool.Release(bullet);
    }

    // 예전 GetObj() 그대로 유지 가능
    public static Bullet GetBullet()
    {
        return bulletSubMachineGun.bulletSubMachineGunPool.Get();
    }
}
