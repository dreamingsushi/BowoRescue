using UnityEngine;
using Unity.Cinemachine;

public class MultiTargetCamera : MonoBehaviour
{
    public CinemachineTargetGroup targetGroup;

    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        targetGroup.Targets.Clear(); // Clear any existing targets

        foreach (GameObject player in players)
        {
            targetGroup.AddMember(player.transform, 1f, 0.5f);
        }
    }
}
