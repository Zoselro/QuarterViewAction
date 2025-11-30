using UnityEngine;
using UnityEngine.Pool;

public class ThrowGrenadeObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject throwGrenade;

    public static ThrowGrenadeObjectPool Instance = null;

    private IObjectPool<GameObject> grenadeObjectPool;

    private void Awake()
    {
        Instance = this;
        grenadeObjectPool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject newObj = Instantiate(throwGrenade);
                return newObj;
            },
            actionOnGet : (g) =>
            {
                Grenade grenade = g.GetComponent<Grenade>();
                g.transform.SetParent(Instance.transform);
                g.gameObject.SetActive(true);
                
                grenade.ReSetState();
            },
            actionOnRelease: (i) =>
            {
                i.transform.SetParent(Instance.transform);
                i.SetActive(false);
            },
            maxSize : 10
        );
    }

    public static GameObject GetThrowGrenade()
    {
        return Instance.grenadeObjectPool.Get();
    }

    public static void ReleaseThrowGrenade(GameObject grenade)
    {
        Instance.grenadeObjectPool.Release(grenade);
    }
}
