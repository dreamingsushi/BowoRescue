using UnityEngine;
using Unity.Netcode;

public class SwordItem : NetworkBehaviour, IPickupable
{
    [SerializeField] private PlayerAttack playerAttack;

    public void OnPickup(Transform holder)
    {
        if (!IsServer)
        {
            ReparentServerRpc(holder.GetComponent<NetworkObject>());
            return;
        }

        FinalizePickup(holder);
        UpdateSwordClientRpc(holder.GetComponent<NetworkObject>());
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReparentServerRpc(NetworkObjectReference holderRef)
    {
        if (holderRef.TryGet(out NetworkObject holder))
        {
            FinalizePickup(holder.transform);
            UpdateSwordClientRpc(holderRef);
        }
    }


    public void FinalizePickup(Transform holder)
    {
        playerAttack = holder.GetComponent<PlayerAttack>();
        playerAttack.isHoldingWeapon = true;

        transform.SetParent(holder);
        transform.localPosition = new Vector3(0, -0.6f, 0.8f);
        transform.localRotation = Quaternion.identity;
        transform.gameObject.SetActive(false);
        GetComponent<Rigidbody>().isKinematic = true;
        
        playerAttack.EquipWeapon(gameObject);

        Debug.Log("Sword picked up!");
    }

    [ClientRpc]
    private void UpdateSwordClientRpc(NetworkObjectReference holderRef)
    {
        if (holderRef.TryGet(out NetworkObject holder))
        {
            transform.SetParent(holder.transform);
            transform.localPosition = new Vector3(0, -0.6f, 0.8f);
            transform.localRotation = Quaternion.identity;
            GetComponent<Rigidbody>().isKinematic = true;
            gameObject.SetActive(false);

            PlayerAttack attack = holder.GetComponent<PlayerAttack>();
            if (attack != null)
            {
                attack.EquipWeapon(gameObject);
                attack.isHoldingWeapon = true;
            }

            Debug.Log("Client updated sword pickup.");
        }
    }

    public void OnDrop(Vector3 dropForce)
    {
        if (!IsServer)
        {
            var netObj = GetComponentInParent<NetworkObject>();
            DropServerRpc(dropForce, netObj);
            return;
        }

        NetworkObject holderObj = playerAttack != null ? playerAttack.GetComponent<NetworkObject>() : GetComponentInParent<NetworkObject>();
        FinalizeDrop(dropForce, holderObj.transform);
        UpdateSwordDropClientRpc(dropForce, holderObj);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropServerRpc(Vector3 dropForce, NetworkObjectReference holderRef)
    {
        if (holderRef.TryGet(out NetworkObject holder))
        {
            FinalizeDrop(dropForce, holder.transform);
            UpdateSwordDropClientRpc(dropForce, holderRef);
        }
    }

    private void FinalizeDrop(Vector3 dropForce, Transform holder)
    {
        var attack = holder.GetComponent<PlayerAttack>();
        attack.isHoldingWeapon = false;
        attack.equippedWeapon.SetActive(false);

        transform.SetParent(null);
        transform.gameObject.SetActive(true);

        transform.position = holder.position;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(holder.forward * dropForce.magnitude, ForceMode.Impulse);
    }

    [ClientRpc]
    private void UpdateSwordDropClientRpc(Vector3 dropForce, NetworkObjectReference holderRef)
    {
        if (holderRef.TryGet(out NetworkObject holder))
        {
            var attack = holder.GetComponent<PlayerAttack>();
            if (attack != null)
            {
                attack.isHoldingWeapon = false;
                attack.equippedWeapon.SetActive(false);
            }
        }

        transform.SetParent(null);
        transform.gameObject.SetActive(true);

        transform.position = holder.transform.position;


        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(holder.transform.forward * dropForce.magnitude, ForceMode.Impulse);

        Debug.Log("Client updated sword drop.");
    }


    // public void OnDrop(Vector3 dropForce)
    // {
    //     playerAttack.isHoldingWeapon = false;
    //     transform.gameObject.SetActive(true);
    //     transform.SetParent(null);
    //     Rigidbody rb = GetComponent<Rigidbody>();
    //     rb.isKinematic = false;
    //     rb.AddForce(dropForce, ForceMode.Impulse);

    //     playerAttack.weapon.SetActive(false);
    // }
}