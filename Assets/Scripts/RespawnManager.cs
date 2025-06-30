using UnityEngine;
using UnityEngine.AI;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [SerializeField] private Transform respawnArea;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Vector3 GetSafeRespawnPosition()
    {
        BoxCollider box = respawnArea.GetComponent<BoxCollider>();
        if (box == null) return respawnArea.position;

        for (int i = 0; i < 10; i++) // Try up to 10 times
        {
            // Local random point in box
            Vector3 localPoint = new Vector3(
                Random.Range(-box.size.x / 2f, box.size.x / 2f),
                0,
                Random.Range(-box.size.z / 2f, box.size.z / 2f)
            );

            // Convert to world position
            Vector3 worldPoint = respawnArea.TransformPoint(box.center + localPoint);

            // Check if it's on NavMesh
            if (NavMesh.SamplePosition(worldPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        Debug.LogWarning("Failed to find valid respawn point inside camera safe zone.");
        return respawnArea.position;
    }

}
