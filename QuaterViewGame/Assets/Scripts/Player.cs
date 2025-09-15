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
    private float baseSpeed; // ���� �ӵ� �����

    private bool isWalk;
    private bool isRun;
    private bool isJump;
    private bool isDodge;
    private bool keepMovingAfterDodge; // ȸ�Ǹ� ���� �� ���� �� ���� �÷��� ����
    private bool keepMovingAfterJump; // ������ ���� �ϰ� ���� �� ���� �÷��� ����

    private Vector3 rotation;
    private Vector3 rotation_value;
    private Vector3 dodgeRotation;
    private Vector3 dodgeMoveDir; // ȸ�ǵ����� ���� �� ���� �̵��� ���� ����
    private Vector3 jumpMoveDir; // ���������� ���� �� ���� �̵��� ���� ����

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

    // ��Ʈ��Ű�� ������ �� ����Ǵ� ȸ�� �޼���
    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.performed && rotation != Vector3.zero && !isJump && !isDodge)
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
