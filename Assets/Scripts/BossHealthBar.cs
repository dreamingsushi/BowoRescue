using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    public Boss boss;

    private void Start()
    {
        if (boss != null)
        {
            boss.currentHealth.OnValueChanged += OnHealthChanged;
            UpdateHealthUI(boss.currentHealth.Value);
        }
    }

    private void OnDestroy()
    {
        if (boss != null)
        {
            boss.currentHealth.OnValueChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        UpdateHealthUI(newValue);

        if (newValue <= 0f)
        {
            gameObject.SetActive(false); // Hides the entire health bar UI
        }
    }

    private void UpdateHealthUI(float health)
    {
        float healthPercent = health / boss.maxHealth;
        fillImage.fillAmount = Mathf.Clamp01(healthPercent);
    }
}
