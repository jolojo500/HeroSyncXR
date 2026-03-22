using System.Collections;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseClient : MonoBehaviour
{
    private const string FIRESTORE_URL =
        "https://firestore.googleapis.com/v1/projects/herosync-76c50/databases/(default)/documents/sessions";

    [System.Serializable]
    private class FirestoreStringField { public string stringValue; }

    [System.Serializable]
    private class FirestoreFields
    {
        public FirestoreStringField report;
        public FirestoreStringField timestamp;
    }

    [System.Serializable]
    private class FirestoreDocument { public FirestoreFields fields; }

    // Call this from GameFlowManager when you receive the final JSON
    public IEnumerator SendReport(string reportJson, BodyPaintData paintData)
    {
        string enrichedJson = EnrichReport(reportJson, paintData);

        var docBody = new FirestoreDocument
        {
            fields = new FirestoreFields
            {
                report    = new FirestoreStringField { stringValue = enrichedJson },
                timestamp = new FirestoreStringField { stringValue = System.DateTime.UtcNow.ToString("o") }
            }
        };

        string body   = JsonConvert.SerializeObject(docBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);

        var request = new UnityWebRequest(FIRESTORE_URL, "POST");
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("[Firebase] Report sent!");
        else
            Debug.LogError($"[Firebase] Error: {request.error}\n{request.downloadHandler.text}");
    }

    // Injects raw zone data into the LLM JSON so the dashboard can render the body map
    private string EnrichReport(string llmJson, BodyPaintData paintData)
    {
        try
        {
            var report = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(llmJson);
            report["zones"] = paintData.zones;
            return JsonConvert.SerializeObject(report);
        }
        catch
        {
            return llmJson; // if parse fails just send as-is
        }
    }
}
