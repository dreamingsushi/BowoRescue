using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerAttack : MonoBehaviourPunCallbacks
{
    public bool isAttacking = false;
    public bool canAttack = true;

    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Combo Settings")]
    [SerializeField] private int maxCombo = 3;
    [SerializeField] private float comboResetTime = 1.0f;

    private int currentCombo = 0;
    private float lastAttackTime;

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!photonView.IsMine) return;

        if (context.performed && canAttack)
        {
            // Check if within combo window
            if (Time.time - lastAttackTime > comboResetTime)
            {
                currentCombo = 0; // Reset combo if too slow
            }

            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canAttack = false;

        // Do attack logic here (play animation based on currentCombo)
        Debug.Log("Attack Combo #" + (currentCombo + 1));

        lastAttackTime = Time.time;
        currentCombo++;
        if (currentCombo >= maxCombo)
        {
            currentCombo = 0; // Restart after max combo
        }

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }
}
