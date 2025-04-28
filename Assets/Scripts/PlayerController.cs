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
    [SerializeField] private Vector2 m_Direction;
    [SerializeField] float gravity = -9.81f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 0.2f;

    [Header("Interaction")]
    [SerializeField] private Transform itemHolder;

    private Vector3 velocity;
    private bool isDashing = false;
    private bool canDash = true;
    private bool isHeld = false;
    private GameObject heldItem = null;
    private CharacterController controller;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    public void OnMove(InputAction.CallbackContext context)
    {
        m_Direction = context.ReadValue<Vector2>();
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash)
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

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!photonView.IsMine) return;
        cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
        cinemachineCamera.Target.TrackingTarget = transform;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        Vector3 movement = new Vector3(m_Direction.x, 0 , m_Direction.y);

        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_RotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), m_RotationSpeed);
        }

        if (!isDashing)
        {
            if (controller.isGrounded && velocity.y < 0)
                velocity.y = -2f;
            else
                velocity.y += gravity * Time.deltaTime;
            controller.Move((movement * m_Speed + velocity) * Time.deltaTime);
        }
    }

    // void FixedUpdate()
    // {
    //     if (photonView.IsMine)
    //     {
    //         if (m_Direction != Vector2.zero)
    //         {
    //             MovePlayer();
    //         }
    //     }


    // }
    void MovePlayer()
    {
        Vector3 movement = new Vector3(m_Direction.x, 0 , m_Direction.y);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), m_RotationSpeed);
        transform.Translate(movement * m_Speed * Time.deltaTime, Space.World);
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
            //transform.Translate(dashDirection * dashSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void PickupItem()
    {
        float pickupRange = 2f;
        float radius = 0.5f;
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);

        if (Physics.SphereCast(origin, radius, transform.forward, out RaycastHit hit, pickupRange))
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
        heldItem.transform.SetParent(itemHolder); // Set as child of ItemHolder
        heldItem.transform.localPosition = Vector3.zero; // Reset position
        heldItem.transform.localRotation = Quaternion.identity; // Reset rotation
        heldItem.GetComponent<Rigidbody>().isKinematic = true; // Disable physics
        isHeld = true;
    }

    void DropItem()
    {
        heldItem.transform.SetParent(null); // Remove parent
        heldItem.GetComponent<Rigidbody>().isKinematic = false; // Enable physics
        heldItem.GetComponent<Rigidbody>().AddForce(transform.forward * 2f, ForceMode.Impulse); // Add slight throw
        heldItem = null;
        isHeld = false;
    }
    
}

