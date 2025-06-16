using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class StatueManager : NetworkBehaviour
{
    public static StatueManager Instance;

    private List<Statue> allStatues = new List<Statue>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterStatue(Statue statue)
    {
        if (!allStatues.Contains(statue))
            allStatues.Add(statue);
    }

    public void CheckAllStatuesActivated()
    {
        foreach (var statue in allStatues)
        {
            if (!statue.IsActivated())
                return;
        }

        // ✅ All statues activated
        Debug.Log("✅ All statues are activated!");

        // Do your logic here (cutscene, open door, etc.)
        OnAllStatuesActivatedClientRpc();
    }

    [ClientRpc]
    private void OnAllStatuesActivatedClientRpc()
    {
        // Do something on all clients, e.g., show VFX, unlock something
        Debug.Log("Client: All statues activated. Triggering event!");
    }
}
