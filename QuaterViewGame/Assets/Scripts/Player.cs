using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    [SerializeField]
    private float speed;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private Animator animator;

    private float velocity;
    private Vector3 inputValue;
    private bool isWalk;
    private bool isRun;
    private Vector3 rotation;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {

    }

    void FixedUpdate()
    {
        velocity = isWalk ? speed * 0.3f * Time.deltaTime : speed * Time.deltaTime;
        rb.linearVelocity = new Vector3(inputValue.x * velocity, 0f, inputValue.y * velocity);
        transform.LookAt(transform.position + rotation);
    }

    public void PlayerMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isRun = true;
            inputValue = context.ReadValue<Vector2>().normalized;
            rotation = new Vector3(inputValue.x, 0f, inputValue.y);
        }
        else if (context.canceled)
        {
            isRun = false;
            inputValue = context.ReadValue<Vector2>().normalized;
        }
        animator.SetBool("IsRun", isRun);
    }

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
}
