using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("GameObject")]
    [SerializeField] private GameObject[] weapons;
    [SerializeField] private GameObject[] grenades;

    [Header("Options")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;

    [SerializeField] private bool[] hasWeapons;

    [Header("Item")]
    [SerializeField] private int ammo;
    [SerializeField] private int coin;
    [SerializeField] private int health;
    [SerializeField] private int hasGrenades;
    
    [Header("ItemOptions")]
    [SerializeField] private int maxAmmo;
    [SerializeField] private int maxCoin;
    [SerializeField] private int maxHealth;
    [SerializeField] private int maxHasGrenades;


    [Header("Components")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private InputAction moveAction;

    private float velocity;
    private float baseSpeed; // 원래 속도 저장용
    private float fireDelay; // 공격 딜레이
    private string subMachineGunName;

    private bool isWalk;
    private bool isRun;
    private bool isJump;
    private bool isDodge;
    private bool isSwap;
    private bool keepMovingAfterDodge; // 회피를 시작 후 끝날 때 까지 플래그 유지
    private bool keepMovingAfterJump; // 점프가 시작 하고 끝날 때 까지 플래그 유지
    private bool isFireReady; // 근접 공격 준비
    private bool isHoldingAttack; // SubMachineGun일 경우, 꾹 눌렀을 때 계속 발사 되는 변수.
    private bool isReload; // 장전을 할것인지?

    private Vector3 rotation;
    private Vector3 rotation_value; // 행동 후 방향키 변경이 반영되지 않는 버그 수정을 위한 변수
    private Vector3 dodgeRotation;
    private Vector3 dodgeMoveDir; // 회피동작이 끝날 때 까지 이동에 사용될 벡터
    private Vector3 jumpMoveDir; // 점프동작이 끝날 때 까지 이동에 사용될 벡터

    private GameObject nearObject;
    private Weapon equipWeapon;
    private int equipWeaponIndex = -1;

    private Coroutine fireLoopCo;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {

    }

    void FixedUpdate()
    {

        fireDelay += Time.deltaTime;
        baseSpeed = speed;
        Move();
        if (Mouse.current.leftButton.isPressed && isHoldingAttack)
        {
            StartCoroutine(AttackCoRouine());
        }
    }

    // 날개 있는 아이템 추가시 if문 해제.
    public void Move()
    {
        // 공격 중에는 이동하지 않음
        if (isFireReady && !isJump && !isDodge)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        if (isDodge && keepMovingAfterDodge)
        {
            // 키보드를 떼도 dodgeMoveDir 로 이동
            float moveSpeed = baseSpeed * Time.deltaTime;
            rb.linearVelocity = new Vector3(dodgeMoveDir.x * moveSpeed,
                                            rb.linearVelocity.y,
                                            dodgeMoveDir.y * moveSpeed);

            transform.LookAt(transform.position + new Vector3(dodgeMoveDir.x, 0f, dodgeMoveDir.y));
        }
        else if (isJump && keepMovingAfterJump)
        {
            // 키보드를 떼도 jumpMoveDir 로 이동
            float moveSpeed = baseSpeed * Time.deltaTime;
            rb.linearVelocity = new Vector3(jumpMoveDir.x * moveSpeed,
                                            rb.linearVelocity.y,
                                            jumpMoveDir.y * moveSpeed);
            transform.LookAt(transform.position + new Vector3(jumpMoveDir.x, 0f, jumpMoveDir.y));
        }
        else
        {
            Walking();
        }        
    }

    public void Walking()
    {
        velocity = isWalk ? baseSpeed * 0.3f * Time.deltaTime : baseSpeed * Time.deltaTime;
        rb.linearVelocity = new Vector3(rotation.x * velocity, rb.linearVelocity.y , rotation.y * velocity);
        transform.LookAt(transform.position + new Vector3(rotation.x, 0f, rotation.y));
    }

    // 방향키를 눌렀을 때 실행되는 메서드
    public void PlayerMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //if (isFireReady && !isJump && !isDodge)
            //    return;

            rotation = context.ReadValue<Vector2>().normalized;
            rotation_value = rotation;

            // 만약에 회피가 동작이 끝났다면, 방향 입력값 다시 주기.
            if (isDodge)
                rotation = dodgeRotation;
            isRun = true;
        }
        else if (context.canceled)
        {
            isRun = false;
            rotation = Vector3.zero;
            rotation_value = Vector3.zero;
        }
        animator.SetBool("IsRun", isRun);
    }

    // 왼쪽 쉬프트키를 눌렀을때 실행되는 메서드
    public void PlayerWalk(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isWalk = true;
        }
        else if (context.canceled)
        {
            isWalk = false;
        }
        animator.SetBool("IsWalk", isWalk);
    }

    // 스페이스바를 눌렀을 때 실행되는 점프 메서드
    public void Jumb(InputAction.CallbackContext context)
    {
        if (context.performed && /*rotation == Vector3.zero &&*/ !isJump && !isDodge && !isSwap)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            isJump = true;
            animator.SetBool("IsJump", isJump);
            animator.SetTrigger("DoJump");
            keepMovingAfterJump = true;
            jumpMoveDir = rotation;
        }
    }

    // 컨트롤키를 눌렀을 때 실행되는 회피 메서드
    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.performed && rotation != Vector3.zero && !isJump && !isDodge && !isSwap && !isFireReady)
        {
            // 회피 시작 시 현재 입력 방향을 그대로 저장
            dodgeMoveDir = rotation;
            // 만약에, 회피중이라면, 지금 보는 방향 그대로 직진.
            dodgeRotation = rotation;
            speed *= 2;
            isDodge = true;
            animator.SetTrigger("DoDodge");

            Invoke("DodgeOut", 0.5f); // 회피가 끝났을 때 수행되는 함수.
        }
    }

    // 아이템을 획득 하는 키
    public void Interaction(InputAction.CallbackContext context)
    {
        if (context.performed && nearObject != null && !isJump) // 점프 하고있는 상태일 때는 아이템 획득 불가.
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.GetValue();
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }

    public void SwapWeapon(int weaponIndex, InputAction.CallbackContext context)
    {
        if(context.performed && !isJump && !isDodge)
        {
            // 만약에 이미 무기가 들려있다면, 이전무기 비활성화 이후 활성화
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeaponIndex = weaponIndex;

            weapons[weaponIndex].SetActive(true);

            animator.SetTrigger("DoSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f); // Swap이 끝났을 때 수행되는 함수.
        }
    }
    // ---- Input System 바인딩용 ----
    public void SwapKey0(InputAction.CallbackContext context)
    {
        int weaponIndex = 0;
        if (!hasWeapons[weaponIndex] || equipWeaponIndex == weaponIndex)
            return;
        SwapWeapon(weaponIndex, context);
    }
    public void SwapKey1(InputAction.CallbackContext context)
    {
        int weaponIndex = 1;
        if (!hasWeapons[weaponIndex] || equipWeaponIndex == weaponIndex)
            return;
        SwapWeapon(weaponIndex, context);
    }
    public void SwapKey2(InputAction.CallbackContext context)
    {
        int weaponIndex = 2;
        if (!hasWeapons[weaponIndex] || equipWeaponIndex == weaponIndex)
            return;
        SwapWeapon(weaponIndex, context);
        subMachineGunName = weapons[weaponIndex].name;
    }
    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && equipWeapon.name != subMachineGunName)
        {
            StartCoroutine(AttackCoRouine());
            isHoldingAttack = false;
        }
        // 만약에 SubMachineGun 이라면, 마우스를 꾹 눌렀을 때 계속 발사 되도록 구현하기.
        else if(context.performed && equipWeapon.name == subMachineGunName)
        {
            isHoldingAttack = true;
        }
        else if (context.canceled)
        {
            isHoldingAttack = false;
        }
    }

    private IEnumerator AttackCoRouine()
    {
        if (equipWeapon == null)
            yield break;
        isFireReady = equipWeapon.GetRate() < fireDelay;
        if (isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            animator.SetTrigger(equipWeapon.GetWeaponType() == Weapon.Type.Melee ? "DoSwing" : "DoShot");
            fireDelay = 0;
            yield return new WaitForSeconds(equipWeapon.GetWaitTime());
        }
        isFireReady = false;
    }

    // 총알을 재장전 하는 메서드.
    public void ReLoad(InputAction.CallbackContext context)
    {
        if (context.performed && !isReload)
        {
            // 들린 무기가 없을 때
            if (equipWeapon == null)
            return;
            // 근접 무기가 들려 있을 때
            if (equipWeapon.GetWeaponType() == Weapon.Type.Melee)
                return;
            // 갖고 있는 총알이 하나도 없을 때
            if (ammo == 0)
                return;
            // 무기의 탄창이 최대 개수 일 때
            if (equipWeapon.IsAmmoFull())
                return;

            if(!isJump && !isDodge && !isSwap)
            {
                animator.SetTrigger("DoReload");
                isReload = true;
                Debug.Log("장전");
                Invoke("ReLoadOut", 3f);
            }
        }
    }

    // 기능
    private void ReLoadOut()
    {
        // 탄창 + 인벤토리 탄수 합산 대신 잔탄 값으로 덮어쓰는 로직 오류
        int reAmmo = ammo < equipWeapon.MaxAmmo ? ammo : equipWeapon.MaxAmmo;
        equipWeapon.SetCurAmmo(reAmmo);
        ammo -= reAmmo;
        isReload = false;
    }

    private void SwapOut()
    {
        isSwap = false;
    }

    private void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
        rotation = rotation_value;
        keepMovingAfterDodge = true;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isJump = false;
            animator.SetBool("IsJump", isJump);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.GetItemType())
            {
                case Item.Type.Ammo:
                    ammo += item.GetValue();
                    if(ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.GetValue();
                    if(coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.GetValue();
                    if(health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    /*grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.GetValue();
                    if(hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;*/
                    if (hasGrenades == maxHasGrenades)
                        return;
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.GetValue();
                    break;
            }
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon")
        {
            nearObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }
}
