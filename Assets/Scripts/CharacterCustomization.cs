using UnityEngine;
using System.Collections.Generic;

public class CharacterCustomization : MonoBehaviour
{
    public List<CustomPart> parts;

    public void NextPart(PartType type)
    {
        var part = parts.Find(p => p.partType == type);
        if (part == null || part.options.Count == 0) return;

        part.currentIndex = (part.currentIndex + 1) % part.options.Count;
        part.renderer.mesh = part.options[part.currentIndex];
    }

    public void PrevPart(PartType type)
    {
        var part = parts.Find(p => p.partType == type);
        if (part == null || part.options.Count == 0) return;

        part.currentIndex = (part.currentIndex - 1 + part.options.Count) % part.options.Count;
        part.renderer.mesh = part.options[part.currentIndex];
    }

    public void NextBody() => NextPart(PartType.Body);
    public void PrevBody() => PrevPart(PartType.Body);
    public void NextEyes() => NextPart(PartType.Eyes);
    public void PrevEyes() => PrevPart(PartType.Eyes);
    public void NextHead() => NextPart(PartType.Head);
    public void PrevHead() => PrevPart(PartType.Head);
    public void NextMouth() => NextPart(PartType.Mouth);
    public void PrevMouth() => PrevPart(PartType.Mouth);

}
