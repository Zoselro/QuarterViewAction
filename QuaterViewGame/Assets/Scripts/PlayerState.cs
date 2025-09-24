using UnityEngine;

public class PlayerState : MonoBehaviour
{
    enum StateMachine
    {
        Idle,
        Run,
        Dodge,
        Attack,
        Jump,
        Swap
    };
}
