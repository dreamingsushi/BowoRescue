using UnityEngine;
using Unity.Cinemachine;

public class MultiTargetCamera : MonoBehaviour
{
    public static MultiTargetCamera Instance;

    public CinemachineTargetGroup targetGroup;

    void Awake()
    {
        Instance = this;
    }

    public void AddTarget(Transform target)
    {
        if (targetGroup != null && target != null)
        {
            targetGroup.AddMember(target, 1f, 0.5f);
        }
    }
}
