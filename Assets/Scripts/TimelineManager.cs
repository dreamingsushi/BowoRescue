using UnityEngine;
using UnityEngine.Playables;
using Unity.Netcode;
using System.Collections;

public class TimelineManager : NetworkBehaviour
{
    public PlayableDirector timeline;
    public CountdownTimer countdownTimer;
    private bool hasHandled = false;

    private void Start()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.TryGetComponent(out PlayerController controller))
            {
                controller.DisableInputsClientRpc();
            }
        }
        timeline.stopped += OnTimelineFinished;
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.TryGetComponent(out PlayerController controller))
            {
                controller.EnableInputsClientRpc();
            }
        }

        countdownTimer.StartCountdown();

        timeline.stopped -= OnTimelineFinished; // Cleanup
    }
}
