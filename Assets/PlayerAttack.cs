using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerAttack : MonoBehaviourPunCallbacks
{
    private Animator animator;
    public bool isAttacking = false;

    [SerializeField] private float attackDuration = 0.5f; // How long attack lasts

    void Awake()
    {
        if (!photonView.IsMine) return;
        animator = GetComponent<Animator>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;

        if (context.performed && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }
}
