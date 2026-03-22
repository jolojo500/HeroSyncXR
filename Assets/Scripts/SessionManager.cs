using UnityEngine;

public class SessionManager : MonoBehaviour
{
    [Header("References")]
    public BodyDataCollector collector;
    public GameFlowManager gameFlow;
    public VRPainter vrPainter;

    [Header("Painting Phase")]
    public GameObject paintingRoot;

    [Header("QA Phase")]
    public GameObject qaRoot;

    void Start()
    {
        paintingRoot.SetActive(true);
        qaRoot.SetActive(false);
    }

    void Update()
    {
        Debug.Log($"LROOAJFOJOELO: {paintingRoot.activeSelf}");
       // if (!paintingRoot.activeSelf) return;

        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
        {
            Debug.Log("Grip pressed detected");
            var paintData = collector.CollectData();
             Debug.Log($"Zones collected: {paintData.zones.Count}");

        if (!paintingRoot.activeSelf)
        {
            Debug.Log("Blocked — paintingRoot not active");
            return;
        }

            if (paintData.zones.Count == 0)
            {
                Debug.Log("[SessionManager] Nothing painted yet.");
                return;
            }

            Debug.Log($"[SessionManager] {paintData.zones.Count} zones collected, switching to Q&A.");

            vrPainter.enabled = false;
            
            paintingRoot.SetActive(false);
            qaRoot.SetActive(true);

            gameFlow.StartSession(paintData);
        }
    }
}