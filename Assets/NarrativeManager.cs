using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NarrativeManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narrativeText;
    [SerializeField] private Image sceneImage;
    [TextArea(3, 10)]
    [SerializeField] private List<string> narrativeLines = new List<string>();
    [SerializeField] private List<Sprite> sceneImages = new List<Sprite>();

    private int currentIndex = 0;

    private void Start()
    {
        DisplayCurrent();
    }

    public void OnClickNext()
    {
        currentIndex++;

        if (currentIndex >= narrativeLines.Count)
        {
            // End of cutscene – maybe load next scene or resume game
            narrativeText.text = "";
            sceneImage.enabled = false;
            return;
        }

        DisplayCurrent();
    }

    private void DisplayCurrent()
    {
        narrativeText.text = narrativeLines[currentIndex];

        if (currentIndex < sceneImages.Count && sceneImages[currentIndex] != null)
        {
            sceneImage.sprite = sceneImages[currentIndex];
            sceneImage.enabled = true;
        }
        else
        {
            sceneImage.enabled = false;
        }
    }

    private void Update()
    {
        // Optional: click anywhere to advance
        if (Input.GetMouseButtonDown(0))
        {
            OnClickNext();
        }
    }
}
