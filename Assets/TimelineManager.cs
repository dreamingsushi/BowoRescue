using UnityEngine;
using UnityEngine.Playables;
using Unity.Netcode;
using System.Collections;

public class TimelineManager : NetworkBehaviour
{
    public PlayableDirector timeline;
    private bool hasHandled = false;

    private void Start()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject.TryGetComponent(out PlayerController controller))
            {
                controller.DisableInputs();
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
                controller.EnableInputs();
            }
        }

        timeline.stopped -= OnTimelineFinished; // Cleanup
    }
}
