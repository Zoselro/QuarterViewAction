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
    private float baseSpeed; // ���� �ӵ� �����
    private float fireDelay; // ���� ������
    private string subMachineGunName;

    private bool isWalk;
    private bool isRun;
    private bool isJump;
    private bool isDodge;
    private bool isSwap;
    private bool keepMovingAfterDodge; // ȸ�Ǹ� ���� �� ���� �� ���� �÷��� ����
    private bool keepMovingAfterJump; // ������ ���� �ϰ� ���� �� ���� �÷��� ����
    private bool isFireReady; // ���� ���� �غ�
    private bool isHoldingAttack; // SubMachineGun�� ���, �� ������ �� ��� �߻� �Ǵ� ����.
    private bool isReload; // ������ �Ұ�����?
    private bool isAttack;

    private Vector3 rotation;
    private Vector3 rotation_value; // �ൿ �� ����Ű ������ �ݿ����� �ʴ� ���� ������ ���� ����
    private Vector3 dodgeRotation;
    private Vector3 dodgeMoveDir; // ȸ�ǵ����� ���� �� ���� �̵��� ���� ����
    private Vector3 jumpMoveDir; // ���������� ���� �� ���� �̵��� ���� ����

    private GameObject nearObject;
    private Weapon equipWeapon;
    private int equipWeaponIndex = -1;


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
        UpdateMouseLook();
    }

    // ���� �ִ� ������ �߰��� if�� ����.
    public void Move()
    {
        // ���� �߿��� �̵����� ����
        if (isFireReady && !isJump && !isDodge)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        if (isDodge && keepMovingAfterDodge)
        {
            // Ű���带 ���� dodgeMoveDir �� �̵�
            float moveSpeed = baseSpeed * Time.deltaTime;
            rb.linearVelocity = new Vector3(dodgeMoveDir.x * moveSpeed,
                                            rb.linearVelocity.y,
                                            dodgeMoveDir.y * moveSpeed);

            transform.LookAt(transform.position + new Vector3(dodgeMoveDir.x, 0f, dodgeMoveDir.y));
        }
        else if (isJump && keepMovingAfterJump)
        {
            // Ű���带 ���� jumpMoveDir �� �̵�
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

    public void UpdateMouseLook()
    {
        // ���콺�� ���� �������� ���� �Ҷ� ȸ��
        if (Mouse.current.leftButton.isPressed && !isDodge)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit rayHit;
            if(Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0f;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    // ����Ű�� ������ �� ����Ǵ� �޼���
    public void PlayerMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //if (isFireReady && !isJump && !isDodge)
            //    return;

            rotation = context.ReadValue<Vector2>().normalized;
            rotation_value = rotation;

            // ���࿡ ȸ�ǰ� ������ �����ٸ�, ���� �Է°� �ٽ� �ֱ�.
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

    // ���� ����ƮŰ�� �������� ����Ǵ� �޼���
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

    // �����̽��ٸ� ������ �� ����Ǵ� ���� �޼���
    public void Jumb(InputAction.CallbackContext context)
    {
        if (context.performed && /*rotation == Vector3.zero &&*/ !isJump && !isDodge && !isSwap && !isAttack)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            isJump = true;
            animator.SetBool("IsJump", isJump);
            animator.SetTrigger("DoJump");
            keepMovingAfterJump = true;
            jumpMoveDir = rotation;
        }
    }

    // ��Ʈ��Ű�� ������ �� ����Ǵ� ȸ�� �޼���
    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.performed && rotation != Vector3.zero && !isJump && !isDodge && !isSwap && !isFireReady)
        {
            // ȸ�� ���� �� ���� �Է� ������ �״�� ����
            dodgeMoveDir = rotation;
            // ���࿡, ȸ�����̶��, ���� ���� ���� �״�� ����.
            dodgeRotation = rotation;
            speed *= 2;
            isDodge = true;
            animator.SetTrigger("DoDodge");

            Invoke("DodgeOut", 0.5f); // ȸ�ǰ� ������ �� ����Ǵ� �Լ�.
        }
    }

    // �������� ȹ�� �ϴ� Ű
    public void Interaction(InputAction.CallbackContext context)
    {
        if (context.performed && nearObject != null && !isJump) // ���� �ϰ��ִ� ������ ���� ������ ȹ�� �Ұ�.
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
            // ���࿡ �̹� ���Ⱑ ����ִٸ�, �������� ��Ȱ��ȭ ���� Ȱ��ȭ
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeaponIndex = weaponIndex;

            weapons[weaponIndex].SetActive(true);

            animator.SetTrigger("DoSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f); // Swap�� ������ �� ����Ǵ� �Լ�.
        }
    }
    // ---- Input System ���ε��� ----
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
        if (context.performed && equipWeapon.name != subMachineGunName && !isJump)
        {
            StartCoroutine(AttackCoRouine());
            isHoldingAttack = false;
            isAttack = true;
        }
        // ���࿡ SubMachineGun �̶��, ���콺�� �� ������ �� ��� �߻� �ǵ��� �����ϱ�.
        else if(context.performed && equipWeapon.name == subMachineGunName && !isJump)
        {
            isHoldingAttack = true;
            isAttack = true;
        }
        else if (context.canceled)
        {
            isHoldingAttack = false;
            isAttack = false;
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

    // �Ѿ��� ������ �ϴ� �޼���.
    public void ReLoad(InputAction.CallbackContext context)
    {
        if (context.performed && !isReload)
        {
            // �鸰 ���Ⱑ ���� ��
            if (equipWeapon == null)
            return;
            // ���� ���Ⱑ ��� ���� ��
            if (equipWeapon.GetWeaponType() == Weapon.Type.Melee)
                return;
            // ���� �ִ� �Ѿ��� �ϳ��� ���� ��
            if (ammo == 0)
                return;
            // ������ źâ�� �ִ� ���� �� ��
            if (equipWeapon.IsAmmoFull())
                return;

            if(!isJump && !isDodge && !isSwap)
            {
                animator.SetTrigger("DoReload");
                isReload = true;
                Debug.Log("����");
                Invoke("ReLoadOut", 3f);
            }
        }
    }

    // ���
    private void ReLoadOut()
    {
        // źâ + �κ��丮 ź�� �ջ� ��� ��ź ������ ����� ���� ����
        //int reAmmo = ammo < equipWeapon.MaxAmmo ? ammo : equipWeapon.MaxAmmo;
        //equipWeapon.SetCurAmmo(reAmmo);
        //ammo -= reAmmo;
        //isReload = false;

        int curAmmo = equipWeapon.CurAmmo;   // ���� ���� źâ ��
        int maxAmmo = equipWeapon.MaxAmmo;   // ���� �ִ� źâ ��
        int needAmmo = maxAmmo - curAmmo;    // �����ؾ� �� ź�� ��

        // �̹� Ǯźâ�̸� ����
        if (needAmmo <= 0)
        {
            equipWeapon.SetCurAmmo(maxAmmo);
            isReload = false;
            return;
        }

        // �÷��̾ ���� ź���� ������ ���
        if (ammo < needAmmo)
        {
            equipWeapon.SetCurAmmo(curAmmo + ammo); // ���� ź�ุŭ ä��
            ammo = 0;
        }
        else
        {
            equipWeapon.SetCurAmmo(maxAmmo); // Ǯźâ
            ammo -= needAmmo;
        }

        isReload = false; // ���� ��
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
