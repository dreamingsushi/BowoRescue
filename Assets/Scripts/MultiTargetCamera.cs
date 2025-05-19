using UnityEngine;
using Unity.Cinemachine;

public class MultiTargetCamera : MonoBehaviour
{
    public static MultiTargetCamera Instance;

    public CinemachineTargetGroup targetGroup;

    public Material normalMaterial;
    public Material ghostMaterial;

    private Camera cam;
    private Renderer rend;

    void Start()
    {
        cam = Camera.main; // Gets the camera that Cinemachine controls
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        Vector3 dir = transform.position - cam.transform.position;
        if (Physics.Raycast(cam.transform.position, dir, out RaycastHit hit))
        {
            if (hit.transform != transform)
                rend.material = ghostMaterial;
            else
                rend.material = normalMaterial;
        }
        else
        {
            rend.material = normalMaterial;
        }
    }

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
