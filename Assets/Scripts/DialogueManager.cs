using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public GameObject choicesUI;
    public GameObject choicesPanel;
    public Button choiceButtonPrefab;
    public Image portraitImage;

    private DialogueData currentData;
    private int currentLine = 0;
    private NPC currentNPC;

    private bool isTalking = false;
    private bool isTyping = false;
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (currentNPC != null && !isTalking && !isTyping)
        {
            StartDialogue(currentNPC.dialogueData);
            currentNPC.StartTalking();
        }

        if (isTalking && isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            SkipTyping();
        }
    }

    public void SetCurrentNPC(NPC npc)
    {
        currentNPC = npc;
    }

    public void ClearCurrentNPC()
    {
        currentNPC = null;
    }

    public void StartDialogue(DialogueData data)
    {
        AudioManager.Instance.PlaySFX("Mumbling");
        playerController.DisableInputs();
        dialogueUI.SetActive(true);
        currentData = data;
        currentLine = 0;
        isTalking = true;

        nameText.text = data.npcName;
        choicesPanel.SetActive(true);
        DisplayLine();
    }

    void DisplayLine()
    {
        StopAllCoroutines();
        ClearChoices();

        var line = currentData.lines[currentLine];
        dialogueText.text = "";

        StartCoroutine(TypeLine(line.text));

        ShowChoices(line);
    }

    IEnumerator TypeLine(string text)
    {
        isTyping = true;
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.02f);
        }
        isTyping = false;

        if (choicesPanel.transform.childCount > 0)
        {
            GameObject firstButton = choicesPanel.transform.GetChild(0).gameObject;
            EventSystem.current.SetSelectedGameObject(firstButton);
        }
    }

    void SkipTyping()
    {
        StopAllCoroutines();
        dialogueText.text = currentData.lines[currentLine].text;
        isTyping = false;
    }

    void ShowChoices(DialogueLine line)
    {
        if (line.choices == null || line.choices.Count == 0)
        {
            // Show "Continue" button if no choices
            Button btn = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
            btn.onClick.AddListener(() =>
            {
                ClearChoices();
                currentLine++;

                if (currentLine >= currentData.lines.Length)
                    EndDialogue();
                else
                    DisplayLine();
            });

            EventSystem.current.SetSelectedGameObject(btn.gameObject);
        }
        else
        {
            bool isFirst = true;
            foreach (var choice in line.choices)
            {
                Button btn = Instantiate(choiceButtonPrefab, choicesPanel.transform);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;

                int next = choice.nextLineIndex;
                btn.onClick.AddListener(() =>
                {
                    ClearChoices();
                    if (next == -1 || next >= currentData.lines.Length)
                        EndDialogue();
                    else
                    {
                        currentLine = next;
                        DisplayLine();
                    }
                });

                if (isFirst)
                {
                    EventSystem.current.SetSelectedGameObject(btn.gameObject);
                    isFirst = false;
                }
            }
        }
    }

    void ClearChoices()
    {
        foreach (Transform child in choicesPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void EndDialogue()
    {
        playerController.EnableInputs();
        dialogueUI.SetActive(false);
        isTalking = false;
        isTyping = false;

        dialogueText.text = "";
        nameText.text = "";
        ClearChoices();
        choicesPanel.SetActive(false);

        if (currentNPC != null)
            currentNPC.StopTalking();
    }
}
