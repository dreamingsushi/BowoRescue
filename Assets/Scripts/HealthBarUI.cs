using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private const float minFill = 0.3f;
    private const float maxFill = 1.0f;
    private PlayerHealth target;

    public void Setup(PlayerHealth playerHealth)
    {
        target = playerHealth;
        UpdateBar();
    }

    public void UpdateBar()
    {
        if (target == null) return;
        SetHealth(target.currentHealth.Value, target.maxHealth);
    }

    public void SetHealth(int current, int max)
    {
        float percent = (float)current / max;
        float adjusted = Mathf.Lerp(minFill, maxFill, percent);
        fillImage.fillAmount = adjusted;
    }

    public bool IsAssigned()
    {
        return target != null;
    }


}
