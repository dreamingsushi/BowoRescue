using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.EventSystems;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float m_Speed;
    private float originalSpeed;
    [SerializeField] private float m_RotationSpeed;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] public Vector2 m_Direction;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 0.2f;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float pickupRadius = 0.5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0.6f, 0.8f);
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private float pullSpeedMultiplier = 0.125f;

    [Header("References")]
    [SerializeField] public CharacterController controller;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject emoteWheel;
    private DialogueManager dialogueManager;
    private bool isMenuOpen = false;
    private Lever nearbyLever;

    // Movement State
    private Vector3 velocity;
    public bool isWalking = false;
    public bool isJumping = false;
    public bool isDashing = false;

    // Interaction State
    public bool isPushing = false;
    public bool isPulling = false;
    private bool canDash = true;
    private bool isHeld = false;
    private NPC nearbyNPC;
    private GameObject heldItem = null;

    // Input State
    private PlayerInput playerInput;
    private Vector2 serverInput;
    private Vector2 lockedPushInput = Vector2.zero;
    private Vector3 pushDirection;
    private GameObject interactingObject;
    private IPushable cachedPushable;


    // --- Unity Lifecycle ---

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        dialogueManager = GetComponent<DialogueManager>();
        originalSpeed = m_Speed;
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

    void FixedUpdate()
    {
        if (IsOwner)
        {
            SendInputServerRpc(m_Direction);
        }

        if (!IsOwner) return;

        Vector3 movement = new Vector3(m_Direction.x, 0, m_Direction.y);
        isWalking = movement.magnitude > 0.1f;

        if (movement.magnitude > 0.1f && !isPushing)
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

            float currentSpeed = m_Speed;
            if (isPulling)
            {
                currentSpeed *= pullSpeedMultiplier;
            }

            controller.Move((movement * currentSpeed + velocity) * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0)
        {
            isJumping = false;
        }
    }

    void Update()
    {
        if (isPushing && interactingObject != null)
        {
            Vector3 objectDir = (interactingObject.transform.position - transform.position).normalized;
            Vector3 moveDir = new Vector3(m_Direction.x, 0, m_Direction.y).normalized;

            // Dot product tells if moving toward or away
            float dot = Vector3.Dot(moveDir, objectDir);

            isPulling = dot < -0.5f; // Pulling if moving away

        }

        if (isPulling)
            cachedPushable.AddForce(-pushDirection * pushForce);
    }

    // --- Input Events ---

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (isPushing)
        {
            if (Mathf.Abs(pushDirection.x) > 0) // X axis
            {
                input.y = 0; // Restrict to X axis
            }
            else if (Mathf.Abs(pushDirection.z) > 0) // Z axis
            {
                input.x = 0; // Restrict to Z axis
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
        if (context.performed && canDash && controller.isGrounded && !isPushing && !isPulling)
        {
            if (IsOwner)
            {
                StartCoroutine(Dash());
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

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
        {
            if (nearbyNPC != null)
            {
                dialogueManager.StartDialogue(nearbyNPC.dialogueData);
                nearbyNPC.StartTalking();
                return; // skip item interaction if talking
            }

            if (nearbyLever != null)
            {
                nearbyLever.TriggerLever();
                return; // skip other interaction
            }

            if (isHeld)
                DropItem();
            else
                PickupItem();

            if (!isPushing)
                PushBox();
            else
            {
                isPushing = false;
                isPulling = false;
                interactingObject = null;
                cachedPushable = null;
            }
        }
    }
    public void OnMenu(InputAction.CallbackContext context)
    {
        if (!IsOwner || !context.performed) return;

        isMenuOpen = !isMenuOpen;

        if (isMenuOpen)
        {
            OpenMenu();
        }
        else
        {
            CloseMenu();
        }
    }

    public void OnEmote(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.started || context.performed)
        {
            emoteWheel.SetActive(true);
        }
        else if (context.canceled)
        {
            // Auto-select the highlighted emote before hiding the wheel
            RMF_RadialMenu rm = emoteWheel.GetComponent<RMF_RadialMenu>();
            if (rm != null && rm.elements.Count > 0)
            {
                int selectedIndex = rm.index;
                if (selectedIndex >= 0 && selectedIndex < rm.elements.Count)
                {
                    // Simulate "submit" for the currently highlighted element
                    var pointer = new PointerEventData(EventSystem.current);
                    ExecuteEvents.Execute(rm.elements[selectedIndex].button.gameObject, pointer, ExecuteEvents.submitHandler);
                }
            }
            emoteWheel.SetActive(false);
        }
    }

    // --- UI Logic ---
    private void OpenMenu()
    {
        pauseMenu.SetActive(true);
    }

    private void CloseMenu()
    {
        pauseMenu.SetActive(false);
    }


    // --- Input Networking ---

    [ServerRpc]
    void SendInputServerRpc(Vector2 input)
    {
        serverInput = input;
    }

    // --- Dash Logic ---

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

    // --- Jump (Manual) ---

    public void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    // --- Item Interactions ---

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
        if (heldItem != null && heldItem.TryGetComponent<IPickupable>(out var pickupable))
        {
            pickupable.OnDrop(transform.forward * 2f);
            heldItem = null;
            isHeld = false;
        }
    }

    // --- Pushable Interactions ---

    void PushBox()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);

        if (Physics.SphereCast(origin, pickupRadius, transform.forward, out RaycastHit hit, pickupRange))
        {
            IPushable pushable = hit.collider.GetComponent<IPushable>();
            if (pushable != null && !isPushing)
            {
                isPushing = true;
                interactingObject = hit.collider.gameObject;
                cachedPushable = pushable;
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
            snapped = direction.x > 0 ? Vector3.right : Vector3.left;
        else
            snapped = direction.z > 0 ? Vector3.forward : Vector3.back;

        pushDirection = snapped;

        if (Mathf.Abs(pushDirection.x) > 0)
            lockedPushInput = new Vector2(Mathf.Sign(pushDirection.x), 0);
        else if (Mathf.Abs(pushDirection.z) > 0)
            lockedPushInput = new Vector2(0, Mathf.Sign(pushDirection.z));

        pushable.AddForce(snapped * pushForce);

        // --- Snap rotation here ---
        Vector3 targetDirection = snapped;
        if (targetDirection != Vector3.zero)
        {
            // Calculate the angle from the forward vector (Vector3.forward) to targetDirection
            float angle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            
            // Snap angle to nearest multiple of 90
            float snappedAngle = Mathf.Round(angle / 90f) * 90f;

            // Apply the snapped rotation to the player (only Y axis)
            transform.rotation = Quaternion.Euler(0, snappedAngle, 0);
        }
    }

    // --- Input Control ---

    public void DisableInputs()
    {
        playerInput.DeactivateInput();
    }

    public void EnableInputs()
    {
        playerInput.ActivateInput();
    }

    // --- NPC Dialogue ---
    public void SetNearbyNPC(NPC npc)
    {
        nearbyNPC = npc;
    }

    public void ClearNearbyNPC()
    {
        nearbyNPC = null;
    }
    // --- Interactables ---
    public void SetNearbyLever(Lever lever)
    {
        nearbyLever = lever;
    }

    // --- Player Status ---

       public void SetSlow(float slowMultiplier, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SlowCoroutine(slowMultiplier, duration));
    }

    private IEnumerator SlowCoroutine(float multiplier, float duration)
    {
        m_Speed *= multiplier;
        yield return new WaitForSeconds(duration);
        m_Speed = originalSpeed;
    }


    // --- Debug Visuals ---

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);
        Vector3 endPoint = origin + transform.forward * pickupRange;

        Gizmos.DrawLine(origin, endPoint);
        Gizmos.DrawWireSphere(origin, pickupRadius);
        Gizmos.DrawWireSphere(endPoint, pickupRadius);
    }
}
