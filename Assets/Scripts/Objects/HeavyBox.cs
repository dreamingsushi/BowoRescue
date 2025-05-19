using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
public class HeavyBox : NetworkBehaviour, IPushable
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void AddForce(Vector3 force)
    {
        if (IsServer)
        {
            // Server applies force
            rb.AddForce(force, ForceMode.Impulse);
        }
        else
        {
            // Client asks server to apply force
            RequestPushServerRpc(force);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPushServerRpc(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }
}
