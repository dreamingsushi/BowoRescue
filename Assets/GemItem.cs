using UnityEngine;
using Unity.Netcode;

public class GemItem : NetworkBehaviour, IPickupable
{
    public Statue.GemColor gemColor;
    [SerializeField] private LayerMask statueLayer;
    [SerializeField] private float detectionRadius = 1.2f;

    public void OnPickup(Transform holder)
    {
        if (!IsServer)
        {
            var netObj = holder.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
            {
                ReparentServerRpc(new NetworkObjectReference(netObj));
            }
            else
            {
                Debug.LogWarning("Invalid holder in OnPickup.");
            }
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
            Debug.Log("Client updated item pickup.");
        }
    }

    public void OnDrop(Vector3 dropForce)
    {
        NetworkObject holderObj = GetComponentInParent<NetworkObject>();
        if (holderObj == null || !holderObj.IsSpawned)
        {
            Debug.LogWarning("Holder is missing or not spawned. Cannot drop.");
            return;
        }

        if (!IsServer)
        {
            try
            {
                DropServerRpc(dropForce, new NetworkObjectReference(holderObj));
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError("Drop failed: " + e.Message);
            }
            return;
        }

        FinalizeDrop(dropForce, holderObj.transform);
        UpdateDropClientRpc(dropForce, new NetworkObjectReference(holderObj));
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

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, statueLayer);
        foreach (var hit in hits)
        {
            var statue = hit.GetComponent<Statue>();
            if (statue != null)
            {
                if (statue.requiredGem == gemColor && !statue.IsActivated())
                {
                    statue.InsertGemServerRpc(gemColor);
                    DisableObjectClientRpc(); // 👈 disable gem for everyone
                }
                break;
            }
        }
    }

    [ClientRpc]
    private void DisableObjectClientRpc() // ✅ Ends with 'ClientRpc'
    {
        gameObject.SetActive(false);
    }

    [ClientRpc]
    private void UpdateDropClientRpc(Vector3 dropForce, NetworkObjectReference holderRef)
    {
        if (this == null) return;

        if (holderRef.TryGet(out NetworkObject holder))
        {
            transform.SetParent(null);
            transform.position = holder.transform.position;

            if (TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = false;
                rb.AddForce(holder.transform.forward * dropForce.magnitude, ForceMode.Impulse);
            }
        }
    }
}
