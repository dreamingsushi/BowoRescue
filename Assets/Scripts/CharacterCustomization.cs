using UnityEngine;
using System.Collections.Generic;

public class CharacterCustomization : MonoBehaviour
{
    public List<CustomPart> parts;

    void Start()
    {
        LoadCustomization();
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
    public void NextHead() => NextPart(PartType.Head);
    public void PrevHead() => PrevPart(PartType.Head);
    public void NextHair() => NextPart(PartType.Hair);
    public void PrevHair() => PrevPart(PartType.Hair);
    public void NextHelmet() => NextPart(PartType.Helmet);
    public void PrevHelmet() => PrevPart(PartType.Helmet);
}
