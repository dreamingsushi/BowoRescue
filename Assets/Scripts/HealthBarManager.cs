using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HealthBarManager : MonoBehaviour
{
    [SerializeField] private List<HealthBarUI> healthBarSlots; // Assign in Inspector: 4 slots

    public static HealthBarManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu" || scene.name == "EndScene" || scene.name == "LobbyScene")
        {
            HideAllHealthBars();
        }
        else if (scene.name == "Level 1" || scene.name == "Level 2" || scene.name == "Level 3 (boss)")
        {
            ShowAllActiveHealthBars();
        }
    }


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

    public void HideAllHealthBars()
    {
        foreach (var bar in healthBarSlots)
        {
            if (bar != null)
                bar.gameObject.SetActive(false);
        }
    }

    private void ShowAllActiveHealthBars()
    {
        foreach (var bar in healthBarSlots)
        {
            if (bar != null && bar.IsAssigned()) // You can add this method in HealthBarUI
            {
                bar.gameObject.SetActive(true);
            }
        }
    }


}
