using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerTeleporter : NetworkBehaviour
{
    public Vector3 teleportDestination = new Vector3(0, 0, 0); // Set in inspector

    void Start()
    {
        StartCoroutine(TPOnSceneLoad());
    }
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Teleport(teleportDestination);
        }
    }

    public IEnumerator TPOnSceneLoad()
    {
        yield return new WaitUntil(() => IsOwner && IsSpawned && SceneManager.GetActiveScene().name == "Level 1");
        Teleport(teleportDestination);

        yield return new WaitUntil(() => IsOwner && IsSpawned && SceneManager.GetActiveScene().name == "Level 2");
        Teleport(teleportDestination);
    }

    public void Teleport(Vector3 destination)
    {
        var controller = GetComponent<CharacterController>();
        if (controller) controller.enabled = false;

        transform.position = destination;

        if (controller) controller.enabled = true;

        Debug.Log($"Teleported player locally to {destination}");
    }
}
