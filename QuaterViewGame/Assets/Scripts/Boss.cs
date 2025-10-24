using System.Collections;
using UnityEngine;

public class Boss : Enemy
{
    [SerializeField] GameObject missile;
    [SerializeField] Transform missilePortA;
    [SerializeField] Transform missilePortB;

    private Vector3 lookVec; // �÷��̾ ���� ������ �̸� �����ϴ� ����
    private Vector3 tauntVec; // ���� �������� �� �̸� �����ϴ� ����
    private bool isLook; // �÷��̾ �ٶ󺸴� �÷��� ����

    private void Awake()
    {

        transform.localScale = new Vector3(3f, 3f, 3f);
    }

    private void Update()
    {
        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;

            transform.LookAt(target.position + lookVec);
        }
    }

    // ������ �ൿ������ �������ִ� �ڷ�ƾ
    private IEnumerator Thick()
    {
        yield return null;
    }
}
