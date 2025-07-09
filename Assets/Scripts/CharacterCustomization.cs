using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class CharacterCustomization : MonoBehaviour
{
    public List<CustomPart> parts;
    public TextMeshProUGUI helmetText;
    public TextMeshProUGUI headText;
    //public TMP_InputField nameInputField;
    public GameObject customizeCamera;
    private LobbyUIManager lobbyUIManager;
    private PlayerCharacter playerCharacter;
    private PlayerManager playerManager;

    void Start()
    {
        LoadCustomization();

        lobbyUIManager = FindAnyObjectByType<LobbyUIManager>();
        playerCharacter = FindAnyObjectByType<PlayerCharacter>();
        playerManager = FindAnyObjectByType<PlayerManager>();
    }

    public void NextPart(PartType type) => ChangePart(type, +1);
    public void PrevPart(PartType type) => ChangePart(type, -1);

    private void ChangePart(PartType type, int direction)
    {
        var part = parts.Find(p => p.partType == type);
        if (part == null || part.options.Count == 0) return;

        if (part.options[part.currentIndex] != null)
            part.options[part.currentIndex].SetActive(false);

        part.currentIndex = (part.currentIndex + direction + part.options.Count) % part.options.Count;

        if (part.options[part.currentIndex] != null)
            part.options[part.currentIndex].SetActive(true);

        SaveCustomization();
    }


    private void ChangeArmSet(int direction)
    {
        var left = parts.Find(p => p.partType == PartType.LeftArm);
        var right = parts.Find(p => p.partType == PartType.RightArm);
        if (left == null || right == null || left.options.Count == 0 || right.options.Count == 0) return;

        if (left.options[left.currentIndex] != null) left.options[left.currentIndex].SetActive(false);
        if (right.options[right.currentIndex] != null) right.options[right.currentIndex].SetActive(false);

        left.currentIndex = (left.currentIndex + direction + left.options.Count) % left.options.Count;
        right.currentIndex = (right.currentIndex + direction + right.options.Count) % right.options.Count;

        if (left.options[left.currentIndex] != null) left.options[left.currentIndex].SetActive(true);
        if (right.options[right.currentIndex] != null) right.options[right.currentIndex].SetActive(true);

        SaveCustomization();
    }

    // Save & Load
    public void SaveCustomization()
    {
        foreach (var part in parts)
        {
            PlayerPrefs.SetInt("Customization_" + part.partType.ToString(), part.currentIndex);
        }

        var data = GetCurrentData();
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SavedCustomization", json);

        PlayerPrefs.Save();
    }

    public void LoadCustomization()
    {
        foreach (var part in parts)
        {
            string key = "Customization_" + part.partType.ToString();
            if (PlayerPrefs.HasKey(key))
            {
                if (part.options[part.currentIndex] != null)
                    part.options[part.currentIndex].SetActive(false);

                part.currentIndex = Mathf.Clamp(PlayerPrefs.GetInt(key), 0, part.options.Count - 1);

                if (part.options[part.currentIndex] != null)
                    part.options[part.currentIndex].SetActive(true);
            }
        }
    }

    // Optional helpers
    public void NextArmSet() => ChangeArmSet(+1);
    public void PrevArmSet() => ChangeArmSet(-1);
    public void NextBody() => NextPart(PartType.Body);
    public void PrevBody() => PrevPart(PartType.Body);
    public void NextHair() => NextPart(PartType.Hair);
    public void PrevHair() => PrevPart(PartType.Hair);
    public void NextHelmet() { NextPart(PartType.Helmet); UpdateIndexDisplay(); }
    public void PrevHelmet() { PrevPart(PartType.Helmet); UpdateIndexDisplay(); }
    public void NextHead() { NextPart(PartType.Head); UpdateIndexDisplay(); }
    public void PrevHead() { PrevPart(PartType.Head); UpdateIndexDisplay(); }

    // Networking

    public void SetPartIndex(PartType type, int index)
    {
        var part = parts.Find(p => p.partType == type);
        if (part == null || part.options.Count == 0) return;

        if (part.options[part.currentIndex] != null)
            part.options[part.currentIndex].SetActive(false);

        part.currentIndex = Mathf.Clamp(index, 0, part.options.Count - 1);

        if (part.options[part.currentIndex] != null)
            part.options[part.currentIndex].SetActive(true);
    }

    public (int currentIndex, int totalOptions) GetPartIndexInfo(PartType type)
    {
        var part = parts.Find(p => p.partType == type);
        if (part == null || part.options.Count == 0) return (0, 0);
        return (part.currentIndex + 1, part.options.Count); // +1 for user-friendly index
    }

    public void UpdateIndexDisplay()
    {
        var (helmetIndex, helmetTotal) = GetPartIndexInfo(PartType.Helmet);
        helmetText.text = $"Helmet: {helmetIndex} / {helmetTotal}";

        var (headIndex, headTotal) = GetPartIndexInfo(PartType.Head);
        headText.text = $"Head: {headIndex} / {headTotal}";
    }

    public void DoneCustomize()
    {
        PlayerPrefs.Save();
        customizeCamera.SetActive(false);
        lobbyUIManager.canvas.SetActive(true);
        SendCustomizationToNetworkedPlayer();
        playerCharacter.LoadAndApplySavedCustomization();
        playerManager.NameUpdate();


        // string playerName = nameInputField.text.Trim();
        // if (!string.IsNullOrEmpty(playerName))
        // {
        //     PlayerPrefs.SetString("PlayerName", playerName);
        //     PlayerPrefs.Save();

        //     LobbyManagerZK.Instance.UpdatePlayerName(playerName);
        //     customizeCamera.SetActive(false);
        //     lobbyUIManager.canvas.SetActive(true);
        //     Debug.Log($"Player name set to: {playerName}");
        //     SendCustomizationToNetworkedPlayer();
        //     playerCharacter.LoadAndApplySavedCustomization();
        //     playerManager.NameUpdate();
        // }
        // else
        // {
        //     Debug.LogWarning("Player name is empty.");
        // }
    }

    public CustomizationData GetCurrentData()
    {
        return new CustomizationData
        {
            bodyIndex = parts.Find(p => p.partType == PartType.Body)?.currentIndex ?? 0,
            headIndex = parts.Find(p => p.partType == PartType.Head)?.currentIndex ?? 0,
            hairIndex = parts.Find(p => p.partType == PartType.Hair)?.currentIndex ?? 0,
            helmetIndex = parts.Find(p => p.partType == PartType.Helmet)?.currentIndex ?? 0,
            leftArmIndex = parts.Find(p => p.partType == PartType.LeftArm)?.currentIndex ?? 0,
            rightArmIndex = parts.Find(p => p.partType == PartType.RightArm)?.currentIndex ?? 0
        };
    }

    public void SendCustomizationToNetworkedPlayer()
    {
        var data = GetCurrentData();

        var networkCustomization = GetComponent<NetworkCharacterCustomization>();
        if (networkCustomization != null && networkCustomization.IsOwner)
        {
            networkCustomization.SubmitCustomizationServerRpc(data);
        }
        else
        {
            Debug.LogWarning("NetworkCharacterCustomization not found or not the owner.");
        }
    }



}
