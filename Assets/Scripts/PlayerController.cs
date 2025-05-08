using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviourPunCallbacks
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
    [SerializeField] private Transform itemHolder;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float pickupRadius = 0.5f;

    private Vector3 velocity;
    public bool isWalking = false;
    public bool isJumping = false;
    public bool isDashing = false;
    private bool canDash = true;
    private bool isHeld = false;
    private GameObject heldItem = null;
    [SerializeField] public CharacterController controller;
    private Animator animator;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    private PlayerInput playerInput;

    public void OnMove(InputAction.CallbackContext context)
    {
        m_Direction = context.ReadValue<Vector2>();
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash && controller.isGrounded)
        {
            if (photonView.IsMine)
            {
                StartCoroutine(Dash());
            }
        }
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (photonView.IsMine)
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
            }
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;
        
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
        if (!photonView.IsMine) return;
        cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
        cinemachineCamera.Target.TrackingTarget = transform;
        playerInput = GetComponent<PlayerInput>();  
    }


    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

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
        if (hit.collider.CompareTag("PickupItem"))
        {
            Pickup(hit.collider.gameObject);
        }
    }
    }

    void Pickup(GameObject item)
    {
        heldItem = item;
        heldItem.transform.SetParent(itemHolder);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
        heldItem.GetComponent<Rigidbody>().isKinematic = true;
        isHeld = true;
    }

    void DropItem()
    {
        heldItem.transform.SetParent(null);
        heldItem.GetComponent<Rigidbody>().isKinematic = false;
        heldItem.GetComponent<Rigidbody>().AddForce(transform.forward * 2f, ForceMode.Impulse);
        heldItem = null;
        isHeld = false;
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

