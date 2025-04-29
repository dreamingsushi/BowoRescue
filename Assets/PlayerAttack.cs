using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Tiny;

public class PlayerAttack : MonoBehaviourPunCallbacks
{
    public bool isAttacking = false;
    public bool canAttack = true;
    public Trail vfx;

    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float attackCooldown = 0.5f;

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;

        if (context.performed && canAttack)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        vfx.enabled = true;
        canAttack = false;
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
        yield return new WaitForSeconds(attackCooldown);
        vfx.enabled = false;
        canAttack = true;
    }
}
