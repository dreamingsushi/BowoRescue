using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private Image progressBar;
    
    public void SetProgress(float value)
    {
        value = Mathf.Clamp01(value); // Ensure value is between 0 and 1
        progressBar.fillAmount = value;
    }
}
