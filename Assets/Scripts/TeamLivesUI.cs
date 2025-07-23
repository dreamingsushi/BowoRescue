using UnityEngine;
using TMPro;

public class TeamLivesUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI livesText;

    private void OnEnable()
    {
        if (TeamLivesManager.Instance != null)
        {
            UpdateText(TeamLivesManager.Instance.GetCurrentLives());
        }
    }


    private void Start()
    {
        // Listen for changes in team lives
        if (TeamLivesManager.Instance != null)
        {
            UpdateText(TeamLivesManager.Instance.GetCurrentLives());
            TeamLivesManager.Instance.OnLivesChanged += UpdateText;
        }
    }

    private void UpdateText(int lives)
    {
        livesText.text = "x" + lives;
    }

    private void OnDestroy()
    {
        if (TeamLivesManager.Instance != null)
        {
            TeamLivesManager.Instance.OnLivesChanged -= UpdateText;
        }
    }
}
