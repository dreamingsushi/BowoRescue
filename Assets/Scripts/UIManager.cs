using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private GameObject StartMenuPanel;
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject CreditsPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject graphicsPanel;
    [SerializeField] private GameObject quitPanel;

    [SerializeField] private GameObject defaultMainButton;
    [SerializeField] private GameObject defaultSettingsButton;
    [SerializeField] private GameObject defaultQuitButton;
    private GameObject currentDefaultButton;
    public CameraTransition cameraTransition;

    void Start()
    {
        ActivatePanel(StartMenuPanel.name);
    }
    public void GoToStartMenu()
    {
        ActivatePanel(StartMenuPanel.name);
        cameraTransition.UpdateCamera(cameraTransition.startMenuCamera);
        AudioManager.Instance.PlaySFX("Menu");        
    }
    public void GoToMainMenu()
    {
        ActivatePanel(MainMenuPanel.name);
        AudioManager.Instance.PlaySFX("Menu");
    }
    public void GoToSettings()
    {
        ActivatePanel(SettingsPanel.name);
        AudioManager.Instance.PlaySFX("Menu");
    }
    public void GoToCredits()
    {
        ActivatePanel(CreditsPanel.name);
        AudioManager.Instance.PlaySFX("Menu");        
    }

    public void ActivatePanel(string panelToBeActivated)
    {
        MainMenuPanel.SetActive(panelToBeActivated.Equals(MainMenuPanel.name));
        StartMenuPanel.SetActive(panelToBeActivated.Equals(StartMenuPanel.name));
        SettingsPanel.SetActive(panelToBeActivated.Equals(SettingsPanel.name));
        quitPanel.SetActive(panelToBeActivated.Equals(quitPanel.name));
        CreditsPanel.SetActive(panelToBeActivated.Equals(CreditsPanel.name));

        if (panelToBeActivated.Equals(MainMenuPanel.name)) currentDefaultButton = defaultMainButton;
        else if (panelToBeActivated.Equals(SettingsPanel.name)) currentDefaultButton = defaultSettingsButton;
        else if (panelToBeActivated.Equals(quitPanel.name)) currentDefaultButton = defaultQuitButton;


        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(currentDefaultButton);
    }

    public void ActivateSettingsPanel(string panelToBeActivated)
    {
        audioPanel.SetActive(panelToBeActivated.Equals(audioPanel.name));
        graphicsPanel.SetActive(panelToBeActivated.Equals(graphicsPanel.name));
    }

    public void OpenAudioSettings()
    {
        ActivateSettingsPanel(audioPanel.name);
        AudioManager.Instance.PlaySFX("Menu");
    }

    public void OpenGraphicsSettings()
    {
        ActivateSettingsPanel(graphicsPanel.name);
        AudioManager.Instance.PlaySFX("Menu");
    }

    public void OpenQuitMenu()
    {
        ActivatePanel(quitPanel.name);
        AudioManager.Instance.PlaySFX("Menu");
    }

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null && currentDefaultButton != null)
        {
            if (Input.GetAxisRaw("Vertical") != 0 || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                EventSystem.current.SetSelectedGameObject(currentDefaultButton);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape)|| Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            GoToStartMenu();
        }
    }

}
