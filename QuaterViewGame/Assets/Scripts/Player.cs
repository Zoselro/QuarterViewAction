using Unity.VisualScripting;
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

    private float velocity;
    private float baseSpeed; // ���� �ӵ� �����

    private bool isWalk;
    private bool isRun;
    private bool isJump;
    private bool isDodge;
    private bool isSwap;
    private bool keepMovingAfterDodge; // ȸ�Ǹ� ���� �� ���� �� ���� �÷��� ����
    private bool keepMovingAfterJump; // ������ ���� �ϰ� ���� �� ���� �÷��� ����
    

    private Vector3 rotation;
    private Vector3 rotation_value;
    private Vector3 dodgeRotation;
    private Vector3 dodgeMoveDir; // ȸ�ǵ����� ���� �� ���� �̵��� ���� ����
    private Vector3 jumpMoveDir; // ���������� ���� �� ���� �̵��� ���� ����

    private GameObject nearObject;
    private GameObject equipWeapon;
    private int equipWeaponIndex = -1;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        baseSpeed = speed;
    }

    void FixedUpdate()
    {
        Debug.Log("hasGrenades : " + hasGrenades);
        Debug.Log("grenades.Length : " + grenades.Length);
        Move();
    }

    // ���� �ִ� ������ �߰��� if�� ����.
    public void Move()
    {
        if (isDodge && keepMovingAfterDodge)
        {
            // Ű���带 ���� dodgeMoveDir �� �̵�
            float moveSpeed = isDodge ? speed * Time.deltaTime
                                      : baseSpeed * Time.deltaTime;
            rb.linearVelocity = new Vector3(dodgeMoveDir.x * moveSpeed,
                                            rb.linearVelocity.y,
                                            dodgeMoveDir.y * moveSpeed);

            transform.LookAt(transform.position + new Vector3(dodgeMoveDir.x, 0f, dodgeMoveDir.y));
        }
        else if (isJump && keepMovingAfterJump)
        {
            // Ű���带 ���� dodgeMoveDir �� �̵�
            float moveSpeed = isJump ? speed * Time.deltaTime
                                      : baseSpeed * Time.deltaTime;
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
        velocity = isWalk ? speed * 0.3f * Time.deltaTime : speed * Time.deltaTime;
        rb.linearVelocity = new Vector3(rotation.x * velocity, rb.linearVelocity.y , rotation.y * velocity);
        transform.LookAt(transform.position + new Vector3(rotation.x, 0f, rotation.y));
    }

    // ����Ű�� ������ �� ����Ǵ� �޼���
    public void PlayerMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
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

    // ��Ʈ��Ű�� ������ �� ����Ǵ� ȸ�� �޼���
    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.performed && rotation != Vector3.zero && !isJump && !isDodge && !isSwap)
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
                equipWeapon.SetActive(false);
            equipWeapon = weapons[weaponIndex];
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
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.GetValue();
                    if(hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
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
