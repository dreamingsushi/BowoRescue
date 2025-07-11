using UnityEngine;

public class GateManager : MonoBehaviour
{
    public Gate gate;
    private bool gateOpened = false;
    public GameObject[] enemies;

    void Update()
    {
        if (gateOpened) return;

        bool allEnemiesDead = true;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                allEnemiesDead = false;
                break;
            }
        }

        if (allEnemiesDead)
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
