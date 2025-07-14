using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;

public class CountdownTimer : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float startTime = 120f;
    [SerializeField] private GameObject timesUpTxt;

    private NetworkVariable<double> startTimestamp = new NetworkVariable<double>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool isRunning = false;
    private bool hasFinished = false;

    private void Update()
    {
        if (!isRunning || hasFinished) return;

        double elapsed = NetworkManager.Singleton.ServerTime.Time - startTimestamp.Value;
        float remaining = Mathf.Max(0f, startTime - (float)elapsed);

        UpdateCountdownUI(remaining);

        if (remaining <= 0f)
        {
            isRunning = false;
            hasFinished = true;
            OnCountdownFinished();
        }
    }

    private void UpdateCountdownUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        countdownText.text = $"{minutes}:{seconds:00}";
    }

    private void OnCountdownFinished()
    {
        StartCoroutine(ShowTimesUp());
    }

    private IEnumerator ShowTimesUp()
    {
        ShowTimesUpClientRpc();
        yield return new WaitForSeconds(3f);

        if (IsServer)
        {
            SceneTransitionManager.Instance.StartTransitionAndLoadScene("LobbyScene");
        }
    }

    [ClientRpc]
    private void ShowTimesUpClientRpc()
    {
        timesUpTxt.SetActive(true);
    }

    public void StartCountdown()
    {
        startTimestamp.Value = NetworkManager.Singleton.ServerTime.Time + 3.0;
        isRunning = true;
        hasFinished = false;
    }

    public void StopCountdown()
    {
        isRunning = false;
    }

    public void ResetCountdown()
    {
        startTimestamp.Value = NetworkManager.Singleton.ServerTime.Time + 3.0;
        isRunning = false;
        hasFinished = false;
    }
}
