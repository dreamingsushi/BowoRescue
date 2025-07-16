using UnityEngine;
using Unity.Netcode;

public class TeamLivesManager : NetworkBehaviour
{
    public static TeamLivesManager Instance;

    [SerializeField] private int maxLives = 5;

    private NetworkVariable<int> currentLives = new NetworkVariable<int>(
        3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public delegate void LivesChangedDelegate(int newLives);
    public event LivesChangedDelegate OnLivesChanged;

    public override void OnNetworkSpawn()
    {
        if (Instance == null) Instance = this;

        currentLives.OnValueChanged += (oldVal, newVal) =>
        {
            OnLivesChanged?.Invoke(newVal);
        };
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReduceLifeServerRpc()
    {
        if (currentLives.Value <= 0) return;

        currentLives.Value--;

        Debug.Log("x" + currentLives.Value);

        if (currentLives.Value <= 0)
        {
            Debug.Log("Team lost all lives!");
            SceneTransitionManager.Instance.StartTransitionAndLoadScene("LobbyScene");
            // TODO: Handle game over here (e.g., transition to GameOver scene)
        }
    }

    public int GetCurrentLives() => currentLives.Value;
}
