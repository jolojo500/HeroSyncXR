using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public TMP_Text questionText;
    public Button yesButton;
    public Button noButton;
    public GameObject summaryPanel;
    public TMP_Text summaryText;

    public void ShowQuestion(string question, System.Action onYes, System.Action onNo)
    {
        questionText.text = question;

        // Clear old listeners first
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() => onYes());
        noButton.onClick.AddListener(() => onNo());

        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
    }

    public void HideButtons()
    {
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        questionText.text = "..."; // loading feel
    }

    public void ShowFinalSummary(string json)
    {
        questionText.gameObject.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        summaryPanel.SetActive(true);
        summaryText.text = json; // raw for now, you can pretty-print later
    }

    public void ShowSpideyGoodbye(string line)
    {
        questionText.gameObject.SetActive(true);
        questionText.text = line; // this is what TTS reads later
    }
}
