using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Tiny;
using UnityEngine.SceneManagement;

public class PlayerAttack : NetworkBehaviour
{
    public GameObject[] weapons; 
    public GameObject equippedWeapon;
    public bool isHoldingWeapon = false;
    public bool isAttacking = false;
    public bool canAttack = true;
    public Trail vfx;
    public Collider dmgCollider;

    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float attackCooldown = 0.5f;
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset weapon state
        if (equippedWeapon != null)
            equippedWeapon.SetActive(false);

        equippedWeapon = null;
        isHoldingWeapon = false;
        vfx = null;
        dmgCollider = null;
        isAttacking = false;
        canAttack = true;
    }

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
        AudioManager.Instance.PlaySFX("Swing");
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

    public void EquipWeapon(GameObject weaponObject)
    {
        foreach (var w in weapons)
            {
                if (w.CompareTag(weaponObject.tag))
                {
                    equippedWeapon = w;
                    equippedWeapon.SetActive(true);

                    vfx = equippedWeapon.GetComponentInChildren<Trail>();
                    dmgCollider = equippedWeapon.GetComponentInChildren<Collider>();

                    if (vfx != null) vfx.enabled = false;
                    if (dmgCollider != null) dmgCollider.enabled = false;

                    break;
                }
            }

            isHoldingWeapon = true;
            weaponObject.SetActive(false);
    }

}
