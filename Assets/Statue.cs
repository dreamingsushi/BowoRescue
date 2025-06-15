using UnityEngine;
using Unity.Netcode;

public class Statue : NetworkBehaviour
{
    public enum GemColor { Red, Blue, Yellow }
    public GemColor requiredGem;
    public GameObject gemObject;

    // Networked activation state
    private NetworkVariable<bool> isActivated = new NetworkVariable<bool>(false);

    private void OnEnable()
    {
        isActivated.OnValueChanged += OnActivationChanged;

        // Sync immediately if already active when client joins
        if (isActivated.Value)
            gemObject.SetActive(true);
    }

    private void OnDisable()
    {
        isActivated.OnValueChanged -= OnActivationChanged;
    }

    // This should only be called by the SERVER
    [ServerRpc(RequireOwnership = false)]
    public void InsertGemServerRpc(GemColor gem)
    {
        if (isActivated.Value) return;

        if (gem == requiredGem)
        {
            isActivated.Value = true;
            Debug.Log($"{requiredGem} Statue Activated!");
        }
        else
        {
            Debug.Log("Wrong gem.");
        }
    }

    private void OnActivationChanged(bool previousValue, bool newValue)
    {
        gemObject.SetActive(newValue);
    }

    // Optional getter for other logic
    public bool IsActivated()
    {
        return isActivated.Value;
    }
}
