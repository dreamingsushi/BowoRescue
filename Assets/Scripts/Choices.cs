using UnityEngine;

public class Choices : MonoBehaviour
{
    public void PlaySound()
    {
        AudioManager.Instance.PlaySFX("Menu");
    }
}
