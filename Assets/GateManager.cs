using UnityEngine;

public class GateManager : MonoBehaviour
{
    public Gate gate;
    private bool gateOpened = false;

    void Update()
    {
        if (gateOpened) return;

        // Check if any enemies are still active
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            OpenGate();
        }
    }

    void OpenGate()
    {
        gateOpened = true;
        gate.OpenGate();
    }
}
