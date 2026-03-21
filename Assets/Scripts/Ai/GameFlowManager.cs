using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("References")]
    public GroqClient groqClient;
    public DialogueUI dialogueUI;// handles showing text + yes/no buttons

    [Header("Test Data - fill in inspector")]
    public List<BodyZone> testZones; //to test hardcoded
    

    private bool waitForAnswer = false;

    void Start()
    {
        var paintData = new BodyPaintData{zones = testZones};

        groqClient.InitializeConversation(paintData);
        StartCoroutine(BeginSession());
    }

    IEnumerator BeginSession()
    {
        yield return StartCoroutine(
            groqClient.SendMessage("The child has finished marking their symptoms.", OnQuestionReceived)
        );
    }

    void OnQuestionReceived(string message)
    {
        if (message.Contains("SPIDEY:") && message.Contains("JSON:")) //aka if llm done
        {
            string spideyLine = message.Split("JSON:")[0].Replace("SPIDEY:","").Trim();
            string json= message.Split("JSON:")[1].Trim();
            Debug.Log("VRO RIGHT HERE VRO:"+spideyLine);
            Debug.Log("AND HERE:"+json);
            dialogueUI.ShowSpideyGoodbye(spideyLine);
            dialogueUI.ShowFinalSummary(json);
            return;
        }
        Debug.Log("VRO we got somem answer VRO:"+message);

        dialogueUI.ShowQuestion(message, onYes: () => OnChildAnswer("Yes"),
                                          onNo:  () => OnChildAnswer("No"));
    }

    void OnChildAnswer(string answer)
    {
        dialogueUI.HideButtons();
        StartCoroutine(groqClient.SendMessage(answer, OnQuestionReceived));
    }

}
