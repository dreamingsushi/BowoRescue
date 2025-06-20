using UnityEngine;
using System.Collections.Generic;

public class HealthBarManager : MonoBehaviour
{
    [SerializeField] private List<HealthBarUI> healthBarSlots; // Assign in Inspector: 4 slots

    public void RegisterPlayer(int playerIndex, PlayerHealth playerHealth)
    {
        if (playerIndex < 0 || playerIndex >= healthBarSlots.Count)
        {
            Debug.LogWarning($"Invalid player index {playerIndex} during health bar registration.");
            return;
        }

        var ui = healthBarSlots[playerIndex];
        ui.gameObject.SetActive(true); // Enable the whole UI slot
        ui.Setup(playerHealth);
        playerHealth.healthBarUI = ui;
    }
}
