using UnityEngine;

public class Statue : MonoBehaviour
{
    public enum GemColor { Red, Blue, Yellow }
    public GemColor requiredGem;
    public GameObject gem;
    private bool isActivated = false;

    public void InsertGem(GemColor gem)
    {
        if (isActivated) return;

        if (gem == requiredGem)
        {
            isActivated = true;
            Debug.Log($"{requiredGem} Statue Activated!");
        }
        else
        {
            Debug.Log("Wrong gem.");
        }
    }

    public bool IsActivated()
    {
        
        return isActivated;
    }
}
