using UnityEngine;
using Unity.Netcode;

public class BasicItem : NetworkBehaviour, IPickupable
{
    public void OnPickup(Transform holder)
    {
        if (!IsServer)
        {
            ReparentServerRpc(holder.GetComponent<NetworkObject>());
            return;
        }

        FinalizePickup(holder);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReparentServerRpc(NetworkObjectReference holderRef)
    {
        if (holderRef.TryGet(out NetworkObject holder))
        {
            FinalizePickup(holder.transform);
            UpdatePickupClientRpc(holderRef);
        }
    }

    private void FinalizePickup(Transform holder)
    {
        transform.SetParent(holder);
        transform.localPosition = new Vector3(0, -0.6f, 0.8f);
        transform.localRotation = Quaternion.identity;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    [ClientRpc]
    private void UpdatePickupClientRpc(NetworkObjectReference holderRef)
    {
        if (holderRef.TryGet(out NetworkObject holder))
        {
            transform.SetParent(holder.transform);
            transform.localPosition = new Vector3(0, -0.6f, 0.8f);
            transform.localRotation = Quaternion.identity;
            GetComponent<Rigidbody>().isKinematic = true;
            Debug.Log("Client updated basic item pickup.");
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

        NetworkObject holderObj = GetComponentInParent<NetworkObject>();
        FinalizeDrop(dropForce, holderObj.transform);
        UpdateDropClientRpc(dropForce, holderObj);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DropServerRpc(Vector3 dropForce, NetworkObjectReference holderRef)
    {
        if (holderRef.TryGet(out NetworkObject holder))
        {
            FinalizeDrop(dropForce, holder.transform);
            UpdateDropClientRpc(dropForce, holderRef);
        }
    }

    private void FinalizeDrop(Vector3 dropForce, Transform holder)
    {
        transform.SetParent(null);
        transform.position = holder.position;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(holder.forward * dropForce.magnitude, ForceMode.Impulse);
    }

    [ClientRpc]
    private void UpdateDropClientRpc(Vector3 dropForce, NetworkObjectReference holderRef)
    {
        if (holderRef.TryGet(out NetworkObject holder))
        {
            transform.SetParent(null);
            transform.position = holder.transform.position;

            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.AddForce(holder.transform.forward * dropForce.magnitude, ForceMode.Impulse);
        }
    }
}
