using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] GameObject meshObj;
    [SerializeField] GameObject effectObj;
    [SerializeField] Rigidbody rigid;
    [SerializeField] private float explosionTime;

    void Start()
    {
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(explosionTime);
        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        meshObj.SetActive(false);
        effectObj.SetActive(true);

        // ������Ʈ ������ �ִ� "Enemy" ���̾ ���� ������Ʈ�� �������� �޼���
        // Physics.SphereCastAll([�� ��ġ], [����], [����], [���̾ ��� ����], [������ ���̾�])
        RaycastHit[] raycastHits =
            Physics.SphereCastAll(transform.position,
                                    15, Vector3.up, 0f, LayerMask.GetMask("Enemy"));

        foreach (RaycastHit hitObj in raycastHits)
        {
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }

        Destroy(gameObject, 5f);
    }
}
