using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform target;

    public void SetTargetTransform(Transform newTarget)
    {
        target = newTarget;
    }

    public void ClearTargetTransform()
    {
        target = null;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
