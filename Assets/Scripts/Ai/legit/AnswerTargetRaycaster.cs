using UnityEngine;

public class AnswerTargetRaycaster : MonoBehaviour
{
    public Transform rightControllerAnchor;
    public GameFlowManager gameFlow;
    public LayerMask targetLayer;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip answerSound;

    LineRenderer _line;
    bool _fired = false;
    AnswerTarget _lastTarget; // ← track which target was hit

    void Awake()
    {
        _line = gameObject.AddComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth = 0.004f;
        _line.endWidth = 0.001f;
        _line.useWorldSpace = true;
        _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _line.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        Ray ray = new Ray(rightControllerAnchor.position, rightControllerAnchor.forward);
        bool trigger = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

        if (Physics.Raycast(ray, out RaycastHit hit, 15f, targetLayer))
        {
             Debug.Log($"VROTATO CHIPS: Raycast hit: {hit.collider.name}");
            _line.SetPosition(0, ray.origin);
            _line.SetPosition(1, hit.point);
            _line.startColor = new Color(0.6f, 0.9f, 1f, 0.9f);
            _line.endColor   = new Color(0.6f, 0.9f, 1f, 1f);

            if (trigger && !_fired)
            {
                    Debug.Log("VROOO: Trigger pressed");
                _fired = true;

                AnswerTarget target = hit.collider.GetComponent<AnswerTarget>();
                Debug.Log($"vrooo Target found: {target != null}");
                if (target != null)
                {
                    _lastTarget = target;

                    // Show cobweb on the specific target that was shot
                    if (target.cobwebOverlay != null)
                        target.cobwebOverlay.SetActive(true);

                    if (audioSource != null && shootSound != null)
                        audioSource.PlayOneShot(shootSound);

                    if (answerSound != null)
                        Invoke(nameof(PlayAnswerSound), 0.4f);

                    Invoke(nameof(HideCobweb), 1.2f);

                    gameFlow.OnChildAnswer(target.isYes ? "Yes" : "No");
                    Invoke(nameof(ResetFired), 1.5f);
                }
            }
        }
        else
        {
            _line.SetPosition(0, ray.origin);
            _line.SetPosition(1, ray.origin + ray.direction * 5f);
            _line.startColor = new Color(1f, 1f, 1f, 0.3f);
            _line.endColor   = new Color(1f, 1f, 1f, 0.1f);
        }
    }

    void PlayAnswerSound() => audioSource.PlayOneShot(answerSound);

    void HideCobweb()
    {
        if (_lastTarget != null && _lastTarget.cobwebOverlay != null)
            _lastTarget.cobwebOverlay.SetActive(false);
    }

    void ResetFired() => _fired = false;
}