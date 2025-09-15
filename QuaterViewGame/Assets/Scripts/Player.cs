using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    [Header("Options")]
    [SerializeField]
    private float speed;
    [SerializeField]
    private float jumpPower;

    [Header("Components")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Rigidbody rb;

    private float velocity;
    private float baseSpeed; // 원래 속도 저장용

    private bool isWalk;
    private bool isRun;
    private bool isJump;
    private bool isDodge;
    private bool keepMovingAfterDodge; // 회피를 시작 후 끝날 때 까지 플래그 유지
    private bool keepMovingAfterJump; // 점프가 시작 하고 끝날 때 까지 플래그 유지

    private Vector3 rotation;
    private Vector3 rotation_value;
    private Vector3 dodgeRotation;
    private Vector3 dodgeMoveDir; // 회피동작이 끝날 때 까지 이동에 사용될 벡터
    private Vector3 jumpMoveDir; // 점프동작이 끝날 때 까지 이동에 사용될 벡터

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
        Move();
    }

    public void Move()
    {
        if (isDodge && keepMovingAfterDodge)
        {
            // 키보드를 떼도 dodgeMoveDir 로 이동
            float moveSpeed = isDodge ? speed * Time.deltaTime
                                      : baseSpeed * Time.deltaTime;
            rb.linearVelocity = new Vector3(dodgeMoveDir.x * moveSpeed,
                                            rb.linearVelocity.y,
                                            dodgeMoveDir.y * moveSpeed);

            transform.LookAt(transform.position + new Vector3(dodgeMoveDir.x, 0f, dodgeMoveDir.y));
        }
        else if (isJump && keepMovingAfterJump)
        {
            // 키보드를 떼도 dodgeMoveDir 로 이동
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

    // 방향키를 눌렀을 때 실행되는 메서드
    public void PlayerMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
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
        if (context.performed && /*rotation == Vector3.zero &&*/ !isJump && !isDodge)
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
        if (context.performed && rotation != Vector3.zero && !isJump && !isDodge)
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
        
    private void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
        rotation = rotation_value;
        keepMovingAfterDodge = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            isJump = false;
            animator.SetBool("IsJump", isJump);
        }
    }
}
