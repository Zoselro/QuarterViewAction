using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float destroyTime;
    [SerializeField] Transform target;
    
    private Camera cam;

    private float yOffset;
    private float upSpeed = 50f;
    private float baseYOffset = 40f;
    private float targetPositionY;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        targetPositionY = target.position.y;
    }

    private void Update()
    {
        yOffset += upSpeed * Time.deltaTime;

        Vector3 screenPos = cam.WorldToScreenPoint(target.transform.position);
        damageText.rectTransform.position = screenPos + new Vector3(0f, targetPositionY + yOffset + baseYOffset, 0f);
        destroyTime -= Time.deltaTime;

        if (destroyTime <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void print(string text)
    {
        damageText.text = string.Format("{0}", text);
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
