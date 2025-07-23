using Unity.Netcode;
using UnityEngine;

public class PlayerStateMachine : NetworkBehaviour
{
    public PlayerController controller;
    public PlayerHealth health;
    public PlayerAttack playerAttack;
    public ClientNetworkAnimator clientNetworkAnimator;
    public Animator animator;
    private BaseState currentState;
    private float stateTimer;
    public float TimeInState => stateTimer; // Track time in current state
    void Start()
    {
        TransitionToState(new IdleState(this)); // Start in Idle state
    }
    void Update()
    {
        if (!IsOwner) return;
        
        stateTimer += Time.deltaTime;
        currentState?.UpdateState(); // Call current state's Update
        float movementSpeed = controller.m_Direction.magnitude; // Get movement intensity
        float dampTime = 0.1f; // Adjust damping for smoothness
        animator.SetFloat("Speed", movementSpeed, dampTime, Time.deltaTime);
    }
    public void TransitionToState(BaseState newState)
    {
        currentState?.ExitState(); // Exit current state
        currentState = newState;   // Assign new state
        stateTimer = 0f;           // Reset state timer
        currentState?.EnterState(); // Enter new state
    }


}