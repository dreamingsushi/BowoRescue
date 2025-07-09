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
        if (IsServer)
        {
            if (timeline.state == PlayState.Playing)
            {
                DisableAllPlayerInputs();
                StartCoroutine(WaitForTimelineEnd());
            }
        }
    }

    private void DisableAllPlayerInputs()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var controller = client.PlayerObject.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.DisableInputs();
            }
        }
    }

    private IEnumerator WaitForTimelineEnd()
    {
        yield return new WaitUntil(() => timeline.state != PlayState.Playing);
        EnableAllPlayerInputs();
    }

    private void EnableAllPlayerInputs()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var controller = client.PlayerObject.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.EnableInputs();
            }
        }
    }
}
