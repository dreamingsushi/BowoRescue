using UnityEngine;

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

        Vector3 center = box.center + respawnArea.position; // world center
        Vector3 size = box.size * 0.5f;

        float x = Random.Range(-size.x, size.x);
        float z = Random.Range(-size.z, size.z);

        return new Vector3(center.x + x, respawnArea.position.y, center.z + z);
    }
}
