using UnityEngine;

public class SwordItem : MonoBehaviour, IPickupable
{
    [SerializeField] private GameObject swordInHand; // Reference to the player's sword model
    [SerializeField] private PlayerAttack playerAttack;

    public void OnPickup(Transform holder)
    {
        playerAttack.isHoldingWeapon = true;
        transform.SetParent(holder);
        transform.gameObject.SetActive(false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        GetComponent<Rigidbody>().isKinematic = true;

        // Enable the sword model in the player's hand
        if (swordInHand != null)
        {
            swordInHand.SetActive(true);
        }

        Debug.Log("Sword picked up!");
    }

    public void OnDrop(Vector3 dropForce)
    {
        playerAttack.isHoldingWeapon = false;
        transform.gameObject.SetActive(true);
        transform.SetParent(null);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(dropForce, ForceMode.Impulse);

        // Optionally disable the sword in hand
        if (swordInHand != null)
        {
            swordInHand.SetActive(false);
        }
    }
}
