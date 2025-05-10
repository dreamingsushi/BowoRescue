using UnityEngine;

public class BasicItem : MonoBehaviour, IPickupable
{
    public void OnPickup(Transform holder)
    {
        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public void OnDrop(Vector3 dropForce)
    {
        transform.SetParent(null);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(dropForce, ForceMode.Impulse);
    }
}
