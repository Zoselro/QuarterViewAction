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
    private Vector3 rotation;
    private Vector3 dodgeRotation;

    private bool isWalk;
    private bool isRun;
    private bool isJump;
    private bool isDodge;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {

    }

    void FixedUpdate()
    {
        Debug.Log(rotation);
        Walking();
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
            if (isDodge)
                rotation = dodgeRotation;
            isRun = true;
        }
        else if (context.canceled)
        {
            isRun = false;
            rotation = context.ReadValue<Vector2>().normalized;
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
        }
    }

    // 컨트롤키를 눌렀을 때 실행되는 회피 메서드
    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.performed && rotation != Vector3.zero && !isJump && !isDodge)
        {
            dodgeRotation = rotation;
            speed *= 2;
            isDodge = true;
            animator.SetTrigger("DoDodge");

            Invoke("DodgeOut", 0.5f);
        }
    }
        
    private void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
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
