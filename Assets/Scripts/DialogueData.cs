using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string npcName;
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    [TextArea] public string text;
    public List<DialogueChoice> choices;
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public int nextLineIndex; // where to go if this choice is selected
}
