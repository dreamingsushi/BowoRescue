using UnityEngine;

public interface IPickupable
{
    void OnPickup(Transform holder);
    void OnDrop(Vector3 dropForce);
}
