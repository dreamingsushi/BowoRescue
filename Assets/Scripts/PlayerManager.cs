using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using Unity.Collections;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private GameObject playerHealthUI;

    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Start()
    {
        StartCoroutine(SpawnPlayer());
        playerNameText.gameObject.SetActive(false);
        playerHealthUI.SetActive(false);
    }

    public IEnumerator SpawnPlayer()
    {
        yield return new WaitUntil(() => IsOwner && IsSpawned && SceneManager.GetActiveScene().name == "GameScene");

        // Assign name only if we're the owner
        if (IsOwner && playerName.Value.Length == 0)
        {
            string nameFromLobby = PlayerPrefs.GetString("PlayerName", "Player");
            playerName.Value = nameFromLobby;
        }

        // Wait until name is received
        yield return new WaitUntil(() => playerName.Value.Length > 0);

        playerNameText.text = playerName.Value.ToString();
        playerNameText.gameObject.SetActive(true);
        playerHealthUI.SetActive(true);
    }

    public void NameUpdate()
    {
        playerNameText.text = playerName.Value.ToString();
    }
}
