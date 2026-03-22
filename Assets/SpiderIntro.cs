using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SpiderIntro : MonoBehaviour
{
    [Header("References")]
    public Animator      anim;
    public SpiderTalkTTS spiderTTS;
    public SpiderTalk    spiderTalk;

    [Header("Audio Sources (GameObjects enfants séparés)")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Animations")]
    public string introAnim = "Backflip";
    public string idleAnim  = "IdleOneMain";

    [Header("Audio Clips")]
    public AudioClip webShooterSound;
    public AudioClip wooHooSound;
    public AudioClip spiderManTheme;

    [Header("Audio Volumes")]
    [Range(0f, 1f)] public float webShooterVolume = 1f;
    [Range(0f, 1f)] public float wooHooVolume     = 0.5f;
    [Range(0f, 1f)] public float themeVolume      = 1f;

    [Header("Timing")]
    public float pauseBetweenLines = 0.1f;
    public float ttsStartTimeout   = 5f;

    private readonly string[] lines =
    {
        "Ouch! Okay... note to self: brick walls are harder than they look. Hey! Perfect timing, I really need a hand here.",
        "Listen, my suit took a serious hit during a fight earlier and it's acting all glitchy. But here's the crazy part: it's actually synced up to YOUR vitals right now!",
        "Science, right? Basically, if you're feeling a sting, a burn, or a heavy feeling anywhere, my suit is picking it up as a system error on my holographic twin here.",
        "I need you to be my Tech-Hero. Use your web-shooter to paint exactly where the suit is acting up on my model. Then, pick the emoji that matches what you're feeling.",
        "You help me pinpoint the glitches, and we'll get this suit fixed in no time. Ready to save the day, partner? Your friendly neighborhood Spider-Man is counting on you!"
    };

    private enum State { WaitingToStart, PlayingIntro, Storytelling, Finished }
    private State     _state    = State.WaitingToStart;
    private Coroutine _sequence;

    void Awake()
    {
        if (anim       == null) anim       = spiderTTS?.GetComponent<Animator>();
        if (spiderTalk == null) spiderTalk = spiderTTS?.GetComponent<SpiderTalk>();

        if (sfxSource == null)
        {
            var go = new GameObject("_SFXSource");
            go.transform.SetParent(this.transform);
            sfxSource = go.AddComponent<AudioSource>();
            sfxSource.playOnAwake  = false;
            sfxSource.spatialBlend = 0f;
        }

        if (musicSource == null)
        {
            var go = new GameObject("_MusicSource");
            go.transform.SetParent(this.transform);
            musicSource = go.AddComponent<AudioSource>();
            musicSource.playOnAwake  = false;
            musicSource.spatialBlend = 0f;
        }
    }

    void Update()
    {
        bool startPressed = false;
        bool resetPressed = false;

        // ── Éditeur Mac : clavier ──────────────────────────────────────────────
#if UNITY_EDITOR
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame) startPressed = true;
            if (Keyboard.current.rKey.wasPressedThisFrame)     resetPressed = true;
        }
#endif

        // ── Quest 2 : manettes OVR ─────────────────────────────────────────────
        // Bouton A (manette droite) ou X (manette gauche) → démarre
        // Bouton B (manette droite) ou Y (manette gauche) → reset
        if (OVRInput.GetDown(OVRInput.Button.One))   startPressed = true; // A
        if (OVRInput.GetDown(OVRInput.Button.Three)) startPressed = true; // X
        if (OVRInput.GetDown(OVRInput.Button.Two))   resetPressed = true; // B
        if (OVRInput.GetDown(OVRInput.Button.Four))  resetPressed = true; // Y

        if (startPressed && _state == State.WaitingToStart)
            _sequence = StartCoroutine(RunFullSequence());

        if (resetPressed)
            RestartAll();
    }

    private IEnumerator RunFullSequence()
    {
        _state = State.PlayingIntro;
        SetLockPosition(false);

        if (webShooterSound != null)
            sfxSource.PlayOneShot(webShooterSound, webShooterVolume);

        yield return new WaitForSeconds(0.2f);

        if (wooHooSound != null)
            sfxSource.PlayOneShot(wooHooSound, wooHooVolume);

        if (spiderManTheme != null)
        {
            musicSource.clip   = spiderManTheme;
            musicSource.volume = themeVolume;
            musicSource.Play();
            StartCoroutine(FadeOutMusic(1.8f, 1f));
        }

        anim.CrossFadeInFixedTime(introAnim, 0.1f);
        yield return StartCoroutine(WaitForAnimationComplete(introAnim));

        anim.applyRootMotion = false;
        if (spiderTalk != null)
        {
            spiderTalk.lockedPosition = spiderTTS.transform.position;
            spiderTalk.lockedRotation = spiderTTS.transform.rotation;
        }
        SetLockPosition(true);

        anim.CrossFadeInFixedTime(idleAnim, 0.3f);
        yield return new WaitForSeconds(0.3f);

        _state = State.Storytelling;
        yield return StartCoroutine(RunStorytelling());

        _state = State.Finished;
        Debug.Log("[SpiderIntro] Terminé. B ou Y pour recommencer.");
    }

    private IEnumerator FadeOutMusic(float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay);
        float startVol = musicSource.volume;
        float elapsed  = 0f;
        while (elapsed < fadeDuration)
        {
            musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        musicSource.volume = 0f;
        musicSource.Stop();
    }

    private IEnumerator RunStorytelling()
    {
        foreach (string line in lines)
        {
            Debug.Log($"[SpiderIntro] → \"{line.Substring(0, Mathf.Min(50, line.Length))}...\"");

            yield return StartCoroutine(WaitUntilIdle());
            spiderTTS.Speak(line);

            float elapsed = 0f;
            while (!spiderTTS.IsBusy && elapsed < ttsStartTimeout)
            { elapsed += Time.deltaTime; yield return null; }

            if (!spiderTTS.IsBusy)
            {
                Debug.LogWarning($"[SpiderIntro] TTS pas démarré : \"{line.Substring(0, Mathf.Min(30, line.Length))}\"");
                continue;
            }

            yield return StartCoroutine(WaitUntilIdle());
            Debug.Log("[SpiderIntro] Réplique terminée ✓");
            yield return new WaitForSeconds(pauseBetweenLines);
        }
        Debug.Log("[SpiderIntro] Storytelling terminé !");
    }

    private IEnumerator WaitUntilIdle()
    {
        while (spiderTTS.IsBusy)
            yield return null;
    }

    private IEnumerator WaitForAnimationComplete(string stateName, float fallback = 2f)
    {
        yield return null;
        yield return null;
        float len = GetClipLength(stateName);
        if (len > 0f) yield return new WaitForSeconds(len * 0.95f);
        else          yield return new WaitForSeconds(fallback);
    }

    private float GetClipLength(string stateName)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return 0f;
        foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
            if (clip.name == stateName) return clip.length;
        return 0f;
    }

    private void SetLockPosition(bool v) { if (spiderTalk != null) spiderTalk.lockPosition = v; }

    private void RestartAll()
    {
        if (_sequence != null) StopCoroutine(_sequence);
        spiderTTS.StopSpeaking();
        SetLockPosition(false);
        anim.applyRootMotion = false;
        anim.CrossFadeInFixedTime(idleAnim, 0.2f);
        musicSource.Stop();
        musicSource.volume = themeVolume;
        _state = State.WaitingToStart;
        Debug.Log("[SpiderIntro] Reset !");
    }
}
