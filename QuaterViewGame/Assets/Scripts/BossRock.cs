using System.Collections;
using UnityEngine;

public class BossRock : Bullet
{
    private Rigidbody rigid;
    private float angularPower = 2f;
    private float scaleValue = 0.1f;
    private bool isShot;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower());
    }

    public IEnumerator GainPowerTimer()
    {
        isShot = true;
        yield return new WaitForSeconds(2.2f);
    }

    public IEnumerator GainPower()
    {
        Debug.Log("isShot : " + isShot);
        while(isShot)
         {
            angularPower += 0.02f;
            scaleValue += 0.005f;
            transform.localScale = Vector3.one * scaleValue;
            rigid.AddTorque(transform.right * angularPower, ForceMode.Acceleration);
            yield return null;
        }
    }
}
