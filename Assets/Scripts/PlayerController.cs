using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float m_Speed;
    [SerializeField] private float m_RotationSpeed;
    [SerializeField] public Vector2 m_Direction;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 0.2f;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float pickupRadius = 0.5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0.6f, 0.8f);
    [SerializeField] private float pushForce = 5f;

    private Vector3 velocity;
    public bool isWalking = false;
    public bool isJumping = false;
    public bool isDashing = false;
    public bool isPushing { get; private set; }
    private bool canDash = true;
    private bool isHeld = false;
    private GameObject heldItem = null;
    [SerializeField] public CharacterController controller;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    private PlayerInput playerInput;
    private Vector2 serverInput;
    private Vector3 pushDirection;
    private Vector2 lockedPushInput = Vector2.zero;
    

    [ServerRpc]
    void SendInputServerRpc(Vector2 input)
    {
        serverInput = input;
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(WaitForCamera());
    }

    private IEnumerator WaitForCamera()
    {
        while (MultiTargetCamera.Instance == null)
            yield return null;

        MultiTargetCamera.Instance.AddTarget(transform);
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (isPushing)
        {
            if (Mathf.Abs(pushDirection.x) > 0) // pushing along X
            {
                input.y = 0;

                if (lockedPushInput != Vector2.zero && Mathf.Sign(input.x) != Mathf.Sign(lockedPushInput.x))
                    input.x = 0;
            }
            else if (Mathf.Abs(pushDirection.z) > 0) // pushing along Z
            {
                input.x = 0;

                if (lockedPushInput != Vector2.zero && Mathf.Sign(input.y) != Mathf.Sign(lockedPushInput.y))
                    input.y = 0;
            }
        }
        else
        {
            lockedPushInput = Vector2.zero;
        }

        m_Direction = input;
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash && controller.isGrounded)
        {
            if (IsOwner)
            {
                StartCoroutine(Dash());
            }
        }
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            if (context.performed)
            {
                if (isHeld)
                {
                    DropItem();
                }
                else
                {
                    PickupItem();
                }

                if (!isPushing)
                {
                    PushBox();
                }
                else
                {
                    isPushing = false;
                }
            }

        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        
        if (context.performed && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
        }
    }

    public void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public void DisableInputs()
    {
        playerInput.DeactivateInput();
    }

    public void EnableInputs()
    {
        playerInput.ActivateInput();
    }


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }


    void FixedUpdate()
    {
        if (IsOwner)
        {
            SendInputServerRpc(m_Direction);
        }

        if (!IsOwner) return;

        Vector3 movement = new Vector3(m_Direction.x, 0 , m_Direction.y);

        isWalking = movement.magnitude > 0.1f; 

        if (movement.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), m_RotationSpeed);
        }

        if (!isDashing)
        {
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            velocity.y += gravity * Time.deltaTime;
            controller.Move((movement * m_Speed + velocity) * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0)
        {
            isJumping = false;
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        Vector3 dashDirection = new Vector3(m_Direction.x, 0, m_Direction.y).normalized;
        float startTime = Time.time;

        while (Time.time < startTime + dashTime)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void PickupItem()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);

        if (Physics.SphereCast(origin, pickupRadius, transform.forward, out RaycastHit hit, pickupRange))
        {
            IPickupable pickupable = hit.collider.GetComponent<IPickupable>();
            if (pickupable != null && !isHeld)
            {
                heldItem = hit.collider.gameObject;
                pickupable.OnPickup(transform);
                heldItem.transform.localPosition = offset;
                isHeld = true;
            }
        }
    }

    void DropItem()
    {
        if (heldItem.TryGetComponent<IPickupable>(out var pickupable))
        {
            pickupable.OnDrop(transform.forward * 2f);
            heldItem = null;
            isHeld = false;
        }
    }

    void PushBox()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);

        if (Physics.SphereCast(origin, pickupRadius, transform.forward, out RaycastHit hit, pickupRange))
        {
            IPushable pushable = hit.collider.GetComponent<IPushable>();
            if (pushable != null && !isPushing)
            {
                isPushing = true;
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!isPushing) return;
        IPushable pushable = hit.collider.GetComponent<IPushable>();
        if (pushable == null) return;

        Vector3 direction = hit.collider.transform.position - transform.position;
        direction.y = 0f;

        Vector3 snapped = Vector3.zero;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            snapped = direction.x > 0 ? Vector3.right : Vector3.left;
        }
        else
        {
            snapped = direction.z > 0 ? Vector3.forward : Vector3.back;
        }

        pushDirection = snapped;

        if (Mathf.Abs(pushDirection.x) > 0)
        lockedPushInput = new Vector2(Mathf.Sign(pushDirection.x), 0);
        else if (Mathf.Abs(pushDirection.z) > 0)
        lockedPushInput = new Vector2(0, Mathf.Sign(pushDirection.z));

        pushable.AddForce(snapped * pushForce);
    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);
        Vector3 endPoint = origin + transform.forward * pickupRange;

        // Draw a line showing pickup range
        Gizmos.DrawLine(origin, endPoint);

        // Draw wire spheres at start and end to visualize the pickup area
        Gizmos.DrawWireSphere(origin, pickupRadius);
        Gizmos.DrawWireSphere(endPoint, pickupRadius);
    }
    
}

