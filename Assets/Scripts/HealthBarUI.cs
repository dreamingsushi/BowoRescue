using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private const float minFill = 0.3f;
    private const float maxFill = 1.0f;

    public void SetHealth(int current, int max)
    {
        float percent = (float)current / max;
        float adjusted = Mathf.Lerp(minFill, maxFill, percent);
        fillImage.fillAmount = adjusted;
    }
}
