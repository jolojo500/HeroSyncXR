using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Oculus.Platform;
using UnityEngine;
using UnityEngine.Networking;

public class GroqClient : MonoBehaviour
{
    private const string GROQ_URL = "https://api.groq.com/openai/v1/chat/completions";
    private static string API_KEY => Resources.Load<TextAsset>("Config/groq_key").text.Trim();
    private const string MODEL = "llama-3.3-70b-versatile";

    
    private List<Message> conversationHistory = new List<Message>();

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class ChatRequest
    {
        public string model;
        public List<Message> messages;
        public float temperature = 1f;
        public int max_tokens = 1024;
    }

    public void InitializeConversation(BodyPaintData paintData)
    {
        conversationHistory.Clear();

        string systemPrompt = @"
        You are a friendly medical assistant helping a child describe their symptoms through a Spider-Man themed game.
        The child has used colored 'web shooters' to mark areas of their body:
        - 🔥 Fire (red) = burning, hot, or sharp pain
        - ❄️ Ice (blue) = cold, numb, or dull ache  
        - 🪨 Rock (grey) = pressure, heaviness, or stiffness
        - ⚡ Thunder (yellow) = tingling, electric, or shooting sensation

        The child's markings:
        " + paintData.ToPromptString() + @"

        Your job:
        1. Ask ONE simple, friendly  yes no question at a time (like Akinator)
        2. Questions must be clinically grounded — target symptom duration, triggers, associated symptoms
        3. Keep language fun and Spider-Man themed but medically purposeful
        4. After 6-8 questions, output a JSON block like:
        {""summary"": ""..."", ""possible_paths"": [""...""], ""flags"": [""...""]}
        5. Never diagnose — only surface patterns for the doctor.
        Start by acknowledging what you see and asking your first question.";

        conversationHistory.Add(new Message{ role = "system", content = systemPrompt});
    }

    public IEnumerator SendMessage(string userMessage, System.Action<string> onResponse)
    {
        conversationHistory.Add(new Message { role = "user", content = userMessage});
        
        var requestBody = new ChatRequest
        {
            model = MODEL,
            messages = conversationHistory
        };

        string json = JsonConvert.SerializeObject(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        var request = new UnityWebRequest(GROQ_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {API_KEY}");

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonConvert.DeserializeObject<GroqResponse>(request.downloadHandler.text);
            string assistantMessage = response.choices[0].message.content;

            conversationHistory.Add(new Message {role = "assistant", content = assistantMessage});

            onResponse?.Invoke(assistantMessage);
        }
        else
        {
            Debug.LogError($"Groq error: {request.error}");
            onResponse?.Invoke("Hmm, my spider-sense is off... try again!");
        }
    }

    public IEnumerator RequestFinalSummary(System.Action<string> onSummary)
    {
        yield return StartCoroutine(SendMessage(
             "Please now generate the final JSON summary for the doctor.", 
            onSummary
        ));
    }
}

[System.Serializable]
public class GroqResponse {
    public Choice[] choices;
    [System.Serializable]
    public class Choice {
        public GroqClient.Message message;
    }
}


[System.Serializable]
public class BodyZone {
    public string zone;        // e.g. "chest", "left arm", "abdomen"
    public string symptomType; // "fire", "ice", "rock", "thunder"
    public float intensity;    // 0-1 based on how much they painted
}

[System.Serializable]
public class BodyPaintData {
    public List<BodyZone> zones = new List<BodyZone>();

    public string ToPromptString() {
        var sb = new StringBuilder();
        foreach (var z in zones)
            sb.AppendLine($"- {z.zone}: {z.symptomType} sensation (intensity: {z.intensity:P0})");
        return sb.ToString();
    }
}
