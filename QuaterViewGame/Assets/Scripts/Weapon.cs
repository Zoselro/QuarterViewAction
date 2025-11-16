using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type
    {
        Melee,
        Range
    };

    [Header("Options")]
    [SerializeField] private Type type;
    [SerializeField] private int damage;
    [SerializeField] private float rate;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float caseRotation;
    [SerializeField] private int maxAmmo; // 실제 최대 탄창 개수

    [Header("Components")]
    [SerializeField] private BoxCollider meleeArea;
    [SerializeField] private TrailRenderer trailEffect;
    [SerializeField] private Transform bulletPos;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletCasePos;
    [SerializeField] private GameObject bulletCase;
    [SerializeField] private int curAmmo; // 현재 탄창의 개수
    
    private float firstWaitTime = 0.1f;
    private float secondWaitTime = 0.3f;
    private float thirdWaitTime = 0.3f;

    public int MaxAmmo => maxAmmo;
    public int CurAmmo => curAmmo;
    private void Start()
    {
        curAmmo = maxAmmo;
    }

    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    private IEnumerator Swing()
    {
        //1
        // yield : 결과를 전달하는 키워드.
        yield return new WaitForSeconds(firstWaitTime); // 1프레임이 아닌 n초 대기라는 뜻
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        //2
        yield return new WaitForSeconds(secondWaitTime);
        meleeArea.enabled = false;
        //3
        yield return new WaitForSeconds(thirdWaitTime);
        trailEffect.enabled = false;
        yield break; // 코루틴 탈출 기능.
    }
    
    private IEnumerator Shot()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit rayHit;
        //Vector3 shootDir = ray.direction;
        //if(Physics.Raycast(ray, out rayHit, 100))
        //{
        //    shootDir = (rayHit.point - bulletPos.position).normalized;
        //}

        // 1. 총알 발사
        //GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Bullet instantBullet = BulletObjectPool.GetBullet();

        instantBullet.transform.position = bulletPos.position;


        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.linearVelocity = bulletPos.forward * bulletSpeed; // shootDir * bulletSpeed;

        // 1프레임 쉬기
        yield return null;

        // 2. 탄피 배출
        //GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Bullet instantCase = BulletObjectPool.GetBulletCase();

        instantCase.transform.position = bulletCasePos.position;
        instantCase.transform.rotation = bulletCasePos.rotation;
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();

        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        //탄피 회전
        caseRigid.AddTorque(Vector3.up * caseRotation);
    }

    //Use() 메인 루틴 -> Swing() 서브루틴 -> Use() 메인루틴
    //Use() 메인 루틴 + Swing() 코루틴 (Co-Op) // 함께 라는 뜻
    public float GetRate()
    {
        return rate;
    }

    public float GetWaitTime()
    {
        float waitTime = firstWaitTime + secondWaitTime + thirdWaitTime;
        return waitTime;
    }

    public Type GetWeaponType()
    {
        return type;
    }

    public void SetCurAmmo(int reAmmo)
    {
        curAmmo = reAmmo;
    }

    public int GetDamage()
    {
        return damage;
    }

    public bool IsAmmoFull()
    {
        if (curAmmo == maxAmmo)
            return true;
        else
            return false;
    }
}
