using UnityEngine;
using Unity.Cinemachine;

public class MultiTargetCamera : MonoBehaviour
{
    public static MultiTargetCamera Instance;

    public CinemachineTargetGroup targetGroup;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("More than one MultiTargetCamera detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void AddTarget(Transform target)
    {
        if (targetGroup != null && target != null)
        {
            targetGroup.AddMember(target, 1f, 0.5f);
        }
    }
}
