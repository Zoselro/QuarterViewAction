using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class DamageTextObejctPool : MonoBehaviour
{
    public static DamageTextObejctPool Instance;

    private IObjectPool<TextMeshProUGUI> damageTextPool;

    [SerializeField] private TextMeshProUGUI damageTextPrefab;
    [SerializeField] private Canvas tarGetCanvas;
    private void Awake()
    {
        Instance = this;

        damageTextPool = new ObjectPool<TextMeshProUGUI>(
            createFunc: () =>
            {
                TextMeshProUGUI newObj = Instantiate(damageTextPrefab, tarGetCanvas.transform);
                newObj.gameObject.SetActive(false);
                return newObj;
            },

            actionOnGet: (b) =>
            {
                b.GetComponent<DamageText>().SetData();
                b.transform.SetParent(tarGetCanvas.transform);
                b.gameObject.SetActive(true);
            },

            actionOnRelease: (b) =>
            {
                b.transform.SetParent(tarGetCanvas.transform);
                b.gameObject.SetActive(false);
            },
            maxSize: 30
            );
    }

    public static void ReturnDamageText(TextMeshProUGUI damageText)
    {
        Instance.damageTextPool.Release(damageText);
    }

    public static TextMeshProUGUI GetDamageText()
    {
        TextMeshProUGUI damageText = Instance.damageTextPool.Get();
        return damageText;
    }
}
