using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerNameInputField;
    [SerializeField] private GameObject MainMenuDisplay;
    [SerializeField] private GameObject RoomOptions;
    [SerializeField] private GameObject CreateRoomDisplay;
    [SerializeField] private GameObject LobbySession;
    [SerializeField] private GameObject Room;

    void Start()
    {
        ActivatePanel(PlayerNameInputField.name);
    }
    public void GoToMainMenu()
    {
        ActivatePanel(MainMenuDisplay.name);
    }
    public void GoToRoomOptions()
    {
        ActivatePanel(RoomOptions.name);
    }
    public void GoToCreateRoomDisplay()
    {
        ActivatePanel(CreateRoomDisplay.name);
    }
    public void GoToLobbySession()
    {
        ActivatePanel(LobbySession.name);
    }
    public void GoToRoom()
    {
        ActivatePanel(Room.name);
    }

    public void ActivatePanel(string panelToBeActivated)
    {
        PlayerNameInputField.SetActive(panelToBeActivated.Equals(PlayerNameInputField.name));
        MainMenuDisplay.SetActive(panelToBeActivated.Equals(MainMenuDisplay.name));
        RoomOptions.SetActive(panelToBeActivated.Equals(RoomOptions.name));
        CreateRoomDisplay.SetActive(panelToBeActivated.Equals(CreateRoomDisplay.name));
        LobbySession.SetActive(panelToBeActivated.Equals(LobbySession.name));
        Room.SetActive(panelToBeActivated.Equals(Room.name));
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
