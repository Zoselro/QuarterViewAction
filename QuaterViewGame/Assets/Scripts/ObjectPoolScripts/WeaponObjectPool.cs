using UnityEngine;
using UnityEngine.Pool;

public class WeaponObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefabs;

    public static WeaponObjectPool Instance = null;

    private IObjectPool<GameObject>[] itemPools;

    private void Awake()
    {
        Instance = this;
        itemPools = new IObjectPool<GameObject>[itemPrefabs.Length];

        for (int i = 0; i < itemPools.Length; i++)
        {
            int index = i;

            itemPools[i] = new ObjectPool<GameObject>(


                createFunc: () =>
                {
                    GameObject obj = Instantiate(itemPrefabs[index]);
                    obj.gameObject.SetActive(false);
                    return obj;
                },
                actionOnGet: (i) =>
                {
                    i.transform.SetParent(Instance.transform);
                    i.gameObject.SetActive(true);
                    Item item = i.GetComponent<Item>();
                },
                actionOnRelease: (i) =>
                {
                    i.transform.SetParent(Instance.transform);
                    i.gameObject.SetActive(false);
                },
                maxSize: 3
            );
        }
    }

    public static void ReturnItem(GameObject item, int index)
    {
        Instance.itemPools[index].Release(item);
    }

    public static GameObject GetItem(int index)
    {
        return Instance.itemPools[index].Get();
    }
}
