using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private GameObject StartMenuPanel;
    [SerializeField] private GameObject SettingsPanel;

    void Start()
    {
        ActivatePanel(StartMenuPanel.name);
    }
    public void GoToMainMenu()
    {
        ActivatePanel(MainMenuPanel.name);
    }
    public void GoToSettings()
    {
        ActivatePanel(SettingsPanel.name);
    }

    public void ActivatePanel(string panelToBeActivated)
    {
        MainMenuPanel.SetActive(panelToBeActivated.Equals(MainMenuPanel.name));
        StartMenuPanel.SetActive(panelToBeActivated.Equals(StartMenuPanel.name));
        SettingsPanel.SetActive(panelToBeActivated.Equals(SettingsPanel.name));
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
