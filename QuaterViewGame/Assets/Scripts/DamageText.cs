using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float destroyTime;
    [SerializeField] Transform target;
    
    private Camera cam;

    private float elapsed;
    private float yOffset;
    private float upSpeed = 50f;
    private float baseYOffset = 40f;
    private float targetPositionY;
    private float elapsedDestroyTime;

    private Color startColor;
    private Color convertColor;
    private void Awake()
    {
        elapsedDestroyTime = destroyTime;
        cam = Camera.main;
        startColor = damageText.color;
        convertColor = startColor;
        convertColor.a = 0.0f;
    }

    private void Start()
    {
        targetPositionY = target.position.y;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / elapsedDestroyTime;
        damageText.color = Color.Lerp(startColor, convertColor, t);

        yOffset += upSpeed * Time.deltaTime;

        Vector3 screenPos = cam.WorldToScreenPoint(target.transform.position);
        damageText.rectTransform.position = screenPos + new Vector3(0f, targetPositionY + yOffset + baseYOffset, 0f);

        elapsedDestroyTime -= Time.deltaTime;



        if (elapsedDestroyTime <= 0)
        {
            //Destroy(this.gameObject);
            DamageTextObejctPool.ReturnDamageText(damageText);
        }
    }

    public void print(string text)
    {
        damageText.text = string.Format("{0}", text);
    }

    public void SetData()
    {
        //targetPositionY = target.position.y;
        yOffset = 0f;
        elapsed = 0f;
        elapsedDestroyTime = destroyTime;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
