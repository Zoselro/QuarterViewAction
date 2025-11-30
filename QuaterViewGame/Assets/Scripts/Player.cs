using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("GameObject")]
    [SerializeField] private GameObject[] weapons;
    [SerializeField] private GameObject[] grenades;
    [SerializeField] private GameObject grenadeObj;
    [SerializeField] private GameObject spawnEnemy;

    [Header("Options")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private bool[] hasWeapons;

    [Header("Item")]
    [SerializeField] private int ammo;
    [SerializeField] private int coin;
    [SerializeField] private int health;
    [SerializeField] private int score;
    [SerializeField] private int hasGrenades;
    
    [Header("ItemOptions")]
    [SerializeField] private int maxAmmo;
    [SerializeField] private int maxCoin;
    [SerializeField] private int maxHealth;
    [SerializeField] private int maxHasGrenades;


    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private InputAction moveAction;
    [SerializeField] private GameManager manager;
    [SerializeField] private AudioSource jumpSound;

    private float velocity;
    private float baseSpeed; // 원래 속도 저장용
    private float fireDelay; // 공격 딜레이
    private string subMachineGunName;
    private float m_MvDelay = 0.0f;


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
    private bool isAttack;
    private bool isDamage; // 플레이어가 몬스터에게 부딛혔을 때 잠깐의 무적타임을 주기 위한 변수.
    //private bool isBorder; // 벽에 부딛히고 있는가?
    private bool isShop; // 상점을 열고 있는가?
    private bool isDead;
    private bool isPickedUp;

    private Vector3 rotation;
    private Vector3 rotation_value; // 행동 후 방향키 변경이 반영되지 않는 버그 수정을 위한 변수
    private Vector3 dodgeRotation;
    private Vector3 dodgeMoveDir; // 회피동작이 끝날 때 까지 이동에 사용될 벡터
    private Vector3 jumpMoveDir; // 점프동작이 끝날 때 까지 이동에 사용될 벡터


    private Animator animator;
    private GameObject nearObject;
    private Weapon equipWeapon;
    private int equipWeaponIndex = -1;
    private MeshRenderer[] meshs;

    public int Coin => coin;
    public int Score => score;
    public int Health => health;
    public int MaxHealth => maxHealth;
    public Weapon EquipWeapon => equipWeapon;
    public bool[] HasWeapons => hasWeapons;
    public int HasGrenades => hasGrenades;


    public int Ammo => ammo;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>(); // 자식 오브젝트의 첫 번째 컴포넌트를 가져옴.
        meshs = GetComponentsInChildren<MeshRenderer>(); // 자식 오브젝트의 컴포넌트들을 가져옴.
        health = maxHealth;
        PlayerPrefs.SetInt("MaxScore", 112500);
    }

    /*private void StopToEnemy()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 3, LayerMask.GetMask("Enemy"));
    }*/
    void FixedUpdate()
    {
        fireDelay += Time.deltaTime;
        baseSpeed = speed;
        
        if(!isDead)
            Move();
        else
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (Mouse.current.leftButton.isPressed && isHoldingAttack)
        {
            StartCoroutine(AttackCoRouine());
        }

        UpdateMouseLook();
        //StopToEnemy();
    }

    // 날개 있는 아이템 추가시 if문 해제.
    public void Move()
    {
        if(0.0f < m_MvDelay)
        {
            m_MvDelay -= Time.deltaTime;
            return;
        }

        // 공격 중에는 이동하지 않음
        if ((isFireReady && !isJump && !isDodge))
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

    public void UpdateMouseLook()
    {
        // 마우스를 찍은 방향으로 공격 할때 회전
        if (Mouse.current.leftButton.isPressed && !isDodge && !isDead)
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

    // 방향키를 눌렀을 때 실행되는 메서드
    public void PlayerMove(InputAction.CallbackContext context)
    {
        if (context.performed && !isDead)
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
        if (isDead)
            return;
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
        if (context.performed && /*rotation == Vector3.zero &&*/ !isJump && !isDodge && !isSwap && !isAttack && !isDead)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            isJump = true;
            animator.SetBool("IsJump", isJump);
            animator.SetTrigger("DoJump");
            keepMovingAfterJump = true;
            jumpMoveDir = rotation;

            jumpSound.Play();
        }
    }

    // 컨트롤키를 눌렀을 때 실행되는 회피 메서드
    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.performed && rotation != Vector3.zero && !isJump && !isDodge && !isSwap && !isFireReady && !isDead)
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
        if (context.performed && nearObject != null && !isJump && !isDead) // 점프 하고있는 상태일 때는 아이템 획득 불가.
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.GetValue();
                hasWeapons[weaponIndex] = true;

                ItemObjectPool.ReturnItem(nearObject, weaponIndex, true);

                nearObject = null;
            }
            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
        }
    }

    public void SwapWeapon(int weaponIndex, InputAction.CallbackContext context)
    {
        if (context.performed && !isJump && !isDodge && !isDead)
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
        if (equipWeapon == null)
            return;
        if (context.performed && equipWeapon.name != subMachineGunName && !isJump && !isShop && !isDead)
        {
            StartCoroutine(AttackCoRouine());
            isHoldingAttack = false;
            isAttack = true;
        }
        // 만약에 SubMachineGun 이라면, 마우스를 꾹 눌렀을 때 계속 발사 되도록 구현하기.
        else if (context.performed && equipWeapon.name == subMachineGunName && !isJump && !isShop && !isDead)
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

    public void GrenadeAttack(InputAction.CallbackContext context)
    {
        if (context.performed && hasGrenades == 0)
            return;
        else if (context.performed && !isReload && !isSwap && !isDead)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10f;

                //Grenade obj = Instantiate(grenadeObj, transform.position, transform.rotation);
                GameObject obj = ThrowGrenadeObjectPool.GetThrowGrenade();
                obj.transform.position = transform.position;
                obj.transform.rotation = transform.rotation;

                Rigidbody rigidGrenade = obj.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);
                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    private IEnumerator AttackCoRouine()
    {
        if (equipWeapon == null)
            yield break;
        isFireReady = equipWeapon.GetRate() < fireDelay;
        if (isFireReady && !isDodge && !isSwap)
        {
            yield return null;

            equipWeapon.Use();

            string setWeapon = null;
            if(equipWeapon.GetWeaponType() == Weapon.Type.Melee)
            {
                setWeapon = "DoSwing";
            }
            else if(equipWeapon.GetWeaponType() == Weapon.Type.Range && equipWeapon.CurAmmo > 0)
            {
                setWeapon = "DoShot";
            }
            animator.SetTrigger(setWeapon);
            //animator.SetTrigger(equipWeapon.GetWeaponType() == Weapon.Type.Melee ? "DoSwing" : "DoShot");
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

    // 개발자 모드 f키 눌렀을 때 바로 앞에 Enemy 소환
    public void SpawnEnemy(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // 1️. Ray를 플레이어의 시점(정면)으로 쏜다
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            // 2️. 거리 5 이내에 무언가가 있으면, 거기 바로 앞에 생성
            if (Physics.Raycast(ray, out hit, 5f))
            {
                // Ray에 맞은 지점 바로 앞에 스폰
                Vector3 spawnPos = hit.point - transform.forward * 0.5f; // 0.5m 뒤쪽 (겹치지 않게)
                Enemy obj = EnemyObjectPool.Instance.GetEnemy(spawnEnemy.GetComponent<Enemy>().GetEnemyType());
                obj.transform.position = spawnPos;
                obj.transform.rotation = Quaternion.identity;
                Enemy enemy = obj.GetComponent<Enemy>();
                enemy.Initialize(transform, manager);
            }
            else
            {
                // 3️. 아무것도 없으면, 플레이어 앞 거리 5 위치에 생성
                Vector3 spawnPos = transform.position + transform.forward * 5f;
                Enemy obj = EnemyObjectPool.Instance.GetEnemy(spawnEnemy.GetComponent<Enemy>().GetEnemyType());
                obj.transform.position = spawnPos;
                obj.transform.rotation = Quaternion.identity;
                Enemy enemy = obj.GetComponent<Enemy>();
                enemy.Initialize(transform, manager);
            }
        }
    }

    // 기능
    private void ReLoadOut()
    {
        int curAmmo = equipWeapon.CurAmmo;   // 현재 무기 탄창 수
        int maxAmmo = equipWeapon.MaxAmmo;   // 무기 최대 탄창 수
        int needAmmo = maxAmmo - curAmmo;    // 장전해야 할 탄약 수

        // 이미 풀탄창이면 리턴
        if (needAmmo <= 0)
        {
            equipWeapon.SetCurAmmo(maxAmmo);
            isReload = false;
            return;
        }

        // 플레이어가 가진 탄약이 부족할 경우
        if (ammo < needAmmo)
        {
            equipWeapon.SetCurAmmo(curAmmo + ammo); // 남은 탄약만큼 채움
            ammo = 0;
        }
        else
        {
            equipWeapon.SetCurAmmo(maxAmmo); // 풀탄창
            ammo -= needAmmo;
        }

        isReload = false; // 장전 끝
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
            int itemIndex = 0;
            switch (item.GetItemType())
            {
                case Item.Type.Heart:
                    itemIndex = 0;
                    health += item.GetValue();
                    if(health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Ammo:
                    ammo += item.GetValue();
                    itemIndex = 1;
                    if (ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Grenade:
                    itemIndex = 2;
                    if (hasGrenades == maxHasGrenades)
                        return;
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.GetValue();
                    break;
                case Item.Type.Coin:
                    coin += item.GetValue();
                    if(coin > maxCoin)
                        coin = maxCoin;
                    break;
            }
            if (item.GetItemType() == Item.Type.Coin)
                ItemObjectPool.ReturnCoin(other.gameObject, item.CoinPoolIndex);
            else
                ItemObjectPool.ReturnItem(other.gameObject, itemIndex, false);
        }
        OnHitByBullet(other);
    }

    public void OnHitByBullet(Collider other)
    {
        if(!other.CompareTag("Item") && !other.CompareTag("Shop"))
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.GetDamage();
                if (health <= 0)
                {
                    health = 0;
                }
                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(other, isBossAtk));
            }

            if (other.GetComponent<Rigidbody>() != null)
            {
                switch (other.tag)
                {
                    case "EnemyBullet":
                        EnemyBulletObejctPool.Instance.ReturnEnemyCBulletPool(other.gameObject.GetComponent<Bullet>());
                        break;
                    case "BossRock":
                        EnemyBulletObejctPool.Instance.ReturnBossRockPool(other.gameObject.GetComponent<BossRock>());
                        break;
                    case "BossMissile":
                        EnemyBulletObejctPool.Instance.ReturnBossBulletPool(other.gameObject.GetComponent<BossMissile>());
                        break;
                    default:
                        Debug.Log("null");
                        break;
                }
            }
        }
    }

    public IEnumerator OnDamage(Collider cdr , bool isBossAtk)
    {
        isDamage = true;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }

        if (isBossAtk)
        {
            rb.AddForce(transform.forward * -25, ForceMode.Impulse);
            m_MvDelay = 0.5f;
        }
        Vector3 reactVector = transform.position - cdr.transform.position;
        reactVector = reactVector.normalized;
        reactVector += Vector3.back;
        rb.AddForce(reactVector * 5, ForceMode.Impulse);

        if (health <= 0 && !isDead)
            onDie();

        yield return new WaitForSeconds(0.5f);

        if (isBossAtk)
            rb.linearVelocity = Vector3.zero;

        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
        isDamage = false;

    }

    private void onDie()
    {
        animator.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon" || other.tag == "Shop")
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
        else if (other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
    }

    public void SetCoin(int coin)
    {
        this.coin = coin;
    }

    internal void SetScore(int score)
    {
        this.score = score;
    }
}
