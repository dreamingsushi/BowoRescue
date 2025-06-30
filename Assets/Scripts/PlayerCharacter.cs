using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerCharacter : NetworkBehaviour
{
    public List<CustomPart> parts;

    public override void OnNetworkSpawn()
    {
        LoadAndApplySavedCustomization();
    }

    public void ApplyCustomization(CustomizationData data)
    {
        SetPartIndex(PartType.Body, data.bodyIndex);
        SetPartIndex(PartType.Head, data.headIndex);
        SetPartIndex(PartType.Hair, data.hairIndex);
        SetPartIndex(PartType.Helmet, data.helmetIndex);
        SetPartIndex(PartType.LeftArm, data.leftArmIndex);
        SetPartIndex(PartType.RightArm, data.rightArmIndex);
    }

    public void SetPartIndex(PartType type, int index)
    {
        var part = parts.Find(p => p.partType == type);
        if (part == null || part.options.Count == 0) return;

        foreach (var option in part.options)
            option.SetActive(false);

        part.currentIndex = Mathf.Clamp(index, 0, part.options.Count - 1);

        if (part.options[part.currentIndex] != null)
        {
            part.options[part.currentIndex].SetActive(true);
            Debug.Log($"[{type}] Set to index {part.currentIndex}");
        }
    }

    public void LoadAndApplySavedCustomization()
    {
        if (!PlayerPrefs.HasKey("SavedCustomization")) return;

        string json = PlayerPrefs.GetString("SavedCustomization");
        CustomizationData data = JsonUtility.FromJson<CustomizationData>(json);

        SubmitCustomizationServerRpc(data); // Send to server
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitCustomizationServerRpc(CustomizationData data, ServerRpcParams rpcParams = default)
    {
        ApplyCustomization(data); // On server
        ApplyCustomizationClientRpc(data); // Send to all clients
    }

    [ClientRpc]
    private void ApplyCustomizationClientRpc(CustomizationData data)
    {
        ApplyCustomization(data); // On all clients
    }
}
