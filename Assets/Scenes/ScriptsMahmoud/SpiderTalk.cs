using UnityEngine;

public class SpiderTalk : MonoBehaviour
{
    [Header("Composants")]
    public Animator anim;
    
    [Header("Contrôle du Mode")]
    public bool isTalking = false;

    [Header("Réglages de Transition (en secondes)")]
    [Tooltip("Vitesse de switch entre Talk et Idle (ex: 0.25)")]
    public float modeTransitionTime = 0.25f;
    [Tooltip("Vitesse d'enchaînement des phrases (ex: 0.2)")]
    public float talkLoopTransition = 0.2f; 
    
    [Header("Noms des Animations")]
    public string[] talkingAnimations = { "talking1", "talking2", "talking3" };
    public string idleMain = "IdleOneMain";
    public string idleFidget = "SecondIdle";

    [Header("Timing Idle")]
    public float fidgetInterval = 15f; 

    private int talkIndex = 0;
    private bool lastMode;
    private float fidgetTimer = 0f;
    private bool playingFidget = false;

    // Fix Position
    [HideInInspector] public Vector3 lockedPosition;
    [HideInInspector] public Quaternion lockedRotation;

    [Header("Fix Position")]
    public bool lockPosition = false;

    void Start()
    {
        lockedPosition = transform.position;
        lockedRotation = transform.rotation;

        if (anim == null) anim = GetComponent<Animator>();
        lastMode = isTalking;
    }
    
void Update()
{
    if (anim == null) return;


    // SWITCH MODE (Talk <-> Idle)
    if (isTalking != lastMode) {
        lastMode = isTalking;
        talkIndex = 0;
        fidgetTimer = 0;
        playingFidget = false;
        string target = isTalking ? talkingAnimations[0] : idleMain;
        anim.CrossFadeInFixedTime(target, modeTransitionTime);
        return;
    }

    if (isTalking) {
        HandleTalking();
    } else {
        HandleIdleWithFidget();
    }
}

    void HandleTalking() {
        var state = anim.GetCurrentAnimatorStateInfo(0);
        // On déclenche la transition 0.2s avant la fin réelle
        if (state.normalizedTime % 1f >= 0.92f && !anim.IsInTransition(0)) {
            talkIndex = (talkIndex + 1) % talkingAnimations.Length;
            anim.CrossFadeInFixedTime(talkingAnimations[talkIndex], talkLoopTransition);
        }
    }

    void HandleIdleWithFidget() {
        if (!playingFidget) {
            fidgetTimer += Time.deltaTime;
            if (fidgetTimer >= fidgetInterval) {
                fidgetTimer = 0;
                playingFidget = true;
                anim.CrossFadeInFixedTime(idleFidget, modeTransitionTime);
            }
        } else {
            var state = anim.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(idleFidget) && state.normalizedTime >= 0.95f && !anim.IsInTransition(0)) {
                playingFidget = false;
                anim.CrossFadeInFixedTime(idleMain, modeTransitionTime);
            }
        }
    }

    void OnAnimatorMove()
{
    if (lockPosition)
    {
        // Ignore completement le root motion quand verrouille
        transform.position = lockedPosition;
        transform.rotation = lockedRotation;
    }
    else
    {
        // Applique le root motion normalement pendant le swing
        anim.ApplyBuiltinRootMotion();
    }
}
}