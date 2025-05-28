using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public struct CustomizationData : INetworkSerializable
{
    public int bodyIndex;
    public int headIndex;
    public int hairIndex;
    public int helmetIndex;
    public int leftArmIndex;
    public int rightArmIndex;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref bodyIndex);
        serializer.SerializeValue(ref headIndex);
        serializer.SerializeValue(ref hairIndex);
        serializer.SerializeValue(ref helmetIndex);
        serializer.SerializeValue(ref leftArmIndex);
        serializer.SerializeValue(ref rightArmIndex);
    }
}

public class NetworkCharacterCustomization : NetworkBehaviour
{
    public CharacterCustomization customization;

    public void ApplyCustomization(CustomizationData data)
    {
        if (customization == null)
        {
            Debug.LogWarning("Customization reference is missing!");
            return;
        }
        customization.SetPartIndex(PartType.Body, data.bodyIndex);
        customization.SetPartIndex(PartType.Head, data.headIndex);
        customization.SetPartIndex(PartType.Hair, data.hairIndex);
        customization.SetPartIndex(PartType.Helmet, data.helmetIndex);
        customization.SetPartIndex(PartType.LeftArm, data.leftArmIndex);
        customization.SetPartIndex(PartType.RightArm, data.rightArmIndex);        
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitCustomizationServerRpc(CustomizationData data, ServerRpcParams rpcParams = default)
    {
        ApplyCustomization(data); // On server
        ApplyCustomizationClientRpc(data); // To all clients
    }

    [ClientRpc]
    public void ApplyCustomizationClientRpc(CustomizationData data)
    {
        ApplyCustomization(data);
    }
    

}
