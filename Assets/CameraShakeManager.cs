using UnityEngine;
using Unity.Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // Optional
    }

    public void Shake(float intensity = 1f)
    {
        impulseSource.GenerateImpulse(intensity);
    }
}
