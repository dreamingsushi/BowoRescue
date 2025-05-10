using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Tiny;

public class PlayerAttack : NetworkBehaviour
{
    public bool isHoldingWeapon = false;
    public bool isAttacking = false;
    public bool canAttack = true;
    public Trail vfx;
    public Collider dmgCollider;

    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float attackCooldown = 0.5f;

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (!isHoldingWeapon) return;

        if (context.performed && canAttack)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        vfx.enabled = true;
        dmgCollider.enabled = true;
        canAttack = false;
        yield return new WaitForSeconds(attackDuration);
        dmgCollider.enabled = false;
        vfx.enabled = false;
        isAttacking = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
