using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;


public class PlayerCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TMP_Text readyStatusText;
    [SerializeField] private Button kickPlayerButton;

    public void SetPlayerName(string name)
    {
        playerNameText.text = name;
    }

    public void SetReadyStatus(bool isReady)
    {
        readyStatusText.text = isReady ? "Ready" : "Not Ready";
        readyStatusText.color = isReady ? Color.green : Color.red;
    }

    private Player player;


    private void Awake()
    {
        kickPlayerButton.onClick.AddListener(KickPlayer);
    }

    public void SetKickPlayerButtonVisible(bool visible)
    {
        kickPlayerButton.gameObject.SetActive(visible);
    }

    public void UpdatePlayer(Player player)
    {
        this.player = player;
        playerNameText.text = player.Data[LobbyManagerZK.KEY_PLAYER_NAME].Value;

        if (player.Data.TryGetValue(LobbyManagerZK.KEY_PLAYER_READY, out var readyData))
        {
            bool isReady = readyData.Value == "true";
            SetReadyStatus(isReady);
        }
        else
        {
            SetReadyStatus(false); // Default
        }
    }

    private void KickPlayer()
    {
        if (player != null)
        {
            LobbyManagerZK.Instance.KickPlayer(player.Id);
        }
    }

}
