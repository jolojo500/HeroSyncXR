using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if META_VOICE_SDK
using Meta.WitAi.TTS.Utilities;
using Meta.WitAi.TTS.Data;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
using System;
#endif

[RequireComponent(typeof(SpiderTalk))]
public class SpiderTalkTTS : MonoBehaviour
{
    [Header("Mode TTS")]
    public bool useMetaTTS = true;

    [Header("Meta TTS — Wit.ai")]
    public TTSSpeaker ttsSpeaker;

    [Header("Classic TTS — Android")]
    public string classicLanguage = "en-US";
    [Range(0.5f, 2f)] public float speechRate  = 1f;
    [Range(0.5f, 2f)] public float speechPitch = 1f;

    [Header("Timing")]
    [Tooltip("Timeout de sécurité absolu par phrase (secondes).")]
    public float sentenceTimeout = 15f;
    [Tooltip("Petite pause entre chaque phrase dans une réplique")]
    public float pauseBetweenSentences = 0.05f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // ── État public ────────────────────────────────────────────────────────────
    public bool IsBusy => _isBusy;

    private bool       _isBusy = false;
    private SpiderTalk _spiderTalk;
    private Coroutine  _speakCoroutine;
    private bool       _sentenceDone = false;

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject _androidTTS;
    private bool   _androidTTSReady    = false;
    private string _pendingAndroidText = null;
#endif

    void Awake()
    {
        _spiderTalk = GetComponent<SpiderTalk>();
#if META_VOICE_SDK
        if (useMetaTTS && ttsSpeaker == null)
            ttsSpeaker = GetComponentInChildren<TTSSpeaker>();
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!useMetaTTS) InitAndroidTTS();
#endif
    }

    void Start()
    {
#if META_VOICE_SDK
        if (useMetaTTS && ttsSpeaker != null)
            Log("Meta Wit.ai TTS prêt.");
        else if (useMetaTTS)
        {
            LogWarning("TTSSpeaker introuvable — basculement Classic TTS.");
            useMetaTTS = false;
        }
#else
        if (useMetaTTS)
        {
            LogWarning("META_VOICE_SDK non défini — basculement Classic TTS.");
            useMetaTTS = false;
        }
#endif
    }

    // ── API publique ───────────────────────────────────────────────────────────

    public void Speak(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        Log($"Speak() → \"{text}\"");
        if (_speakCoroutine != null) StopCoroutine(_speakCoroutine);
        SetBusy(true);
        _speakCoroutine = StartCoroutine(SpeakAllSentences(text));
    }

    public void StopSpeaking()
    {
        if (_speakCoroutine != null) { StopCoroutine(_speakCoroutine); _speakCoroutine = null; }
#if META_VOICE_SDK
        if (useMetaTTS && ttsSpeaker != null)
        {
            ttsSpeaker.Events.OnPlaybackComplete.RemoveListener(OnTTSPlaybackComplete);
            ttsSpeaker.StopSpeaking();
        }
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!useMetaTTS && _androidTTS != null) _androidTTS.Call<int>("stop");
#endif
        _sentenceDone = true;
        SetBusy(false);
    }

    // ── Parle phrase par phrase ────────────────────────────────────────────────
    private IEnumerator SpeakAllSentences(string fullText)
    {
        string[] sentences = SplitIntoSentences(fullText);
        Log($"Découpé en {sentences.Length} phrase(s).");

        foreach (string sentence in sentences)
        {
            string s = sentence.Trim();
            if (string.IsNullOrEmpty(s)) continue;
            Log($"  → \"{s}\"");

            if (useMetaTTS)
                yield return StartCoroutine(SpeakOneSentenceMeta(s));
            else
                yield return StartCoroutine(SpeakOneSentenceClassic(s));

            if (pauseBetweenSentences > 0f)
                yield return new WaitForSeconds(pauseBetweenSentences);
        }

        Log("Toutes les phrases terminées.");
        SetBusy(false);
        _speakCoroutine = null;
    }

    // ── Meta TTS : UNE phrase ─────────────────────────────────────────────────
    private IEnumerator SpeakOneSentenceMeta(string sentence)
    {
#if META_VOICE_SDK
        if (ttsSpeaker == null) yield break;

        _sentenceDone = false;
        ttsSpeaker.Events.OnPlaybackComplete.AddListener(OnTTSPlaybackComplete);

        ttsSpeaker.Speak(sentence);

        float elapsed = 0f;
        while (!_sentenceDone && elapsed < sentenceTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!_sentenceDone)
            LogWarning($"Timeout ({sentenceTimeout}s) pour : \"{sentence}\"");
        else
            Log($"  ✓ fini en {elapsed:F1}s");

        ttsSpeaker.Events.OnPlaybackComplete.RemoveListener(OnTTSPlaybackComplete);
#else
        yield return null;
#endif
    }

#if META_VOICE_SDK
    // Signature correcte : (TTSSpeaker speaker, TTSClipData clipData)
    private void OnTTSPlaybackComplete(TTSSpeaker speaker, TTSClipData clipData)
    {
        Log($"  [Event] OnPlaybackComplete → \"{clipData?.textToSpeak}\"");
        _sentenceDone = true;
    }
#endif

    // ── Classic TTS : UNE phrase ──────────────────────────────────────────────
    private IEnumerator SpeakOneSentenceClassic(string sentence)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!_androidTTSReady) { _pendingAndroidText = sentence; yield break; }

        _androidTTS.Call<int>("speak", sentence, 0, null, "spidertalk_utt");

        yield return new WaitForSeconds(0.2f);
        while (_androidTTS.Call<bool>("isSpeaking"))
            yield return new WaitForSeconds(0.05f);
#else
        int wc = sentence.Split(' ').Length;
        float duration = Mathf.Max(0.5f, wc * 0.35f);
        yield return new WaitForSeconds(duration);
        Log($"  [Éditeur] simulé {duration:F1}s pour \"{sentence}\"");
#endif
    }

    // ── Découpage en phrases ──────────────────────────────────────────────────
    private string[] SplitIntoSentences(string text)
    {
        const string ELLIPSIS = "\x01\x01\x01";
        text = text.Replace("...", ELLIPSIS);

        string[] parts = Regex.Split(text, @"(?<=[.!?])(?=\s|$)");

        var result = new List<string>();
        foreach (string part in parts)
        {
            string s = part.Replace(ELLIPSIS, "...").Trim();
            if (string.IsNullOrEmpty(s)) continue;

            if (s.Split(' ').Length > 10)
                result.AddRange(SplitOnMiddleComma(s));
            else
                result.Add(s);
        }
        return result.Count > 0 ? result.ToArray() : new[] { text };
    }

    private List<string> SplitOnMiddleComma(string sentence)
    {
        var result = new List<string>();
        var commas = Regex.Matches(sentence, @",\s*");
        if (commas.Count == 0) { result.Add(sentence); return result; }

        int mid = sentence.Length / 2;
        int bestPos = -1, bestDist = int.MaxValue;
        foreach (Match m in commas)
        {
            int dist = Mathf.Abs(m.Index - mid);
            if (dist < bestDist) { bestDist = dist; bestPos = m.Index + m.Length; }
        }

        if (bestPos > 0 && bestPos < sentence.Length)
        {
            result.Add(sentence.Substring(0, bestPos).Trim().TrimEnd(','));
            result.Add(sentence.Substring(bestPos).Trim());
        }
        else result.Add(sentence);

        return result;
    }

    // ── Android init ──────────────────────────────────────────────────────────
#if UNITY_ANDROID && !UNITY_EDITOR
    private void InitAndroidTTS()
    {
        try
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            _androidTTS     = new AndroidJavaObject("android.speech.tts.TextToSpeech",
                                  activity, new AndroidTTSInitListener(this));
        }
        catch (Exception e) { LogWarning($"Échec init Android TTS : {e.Message}"); }
    }

    public void OnAndroidTTSReady(bool success)
    {
        if (!success) { LogWarning("Android TTS init échoué."); return; }
        var localeClass = new AndroidJavaClass("java.util.Locale");
        AndroidJavaObject locale;
        if (classicLanguage.Contains("-"))
        {
            var parts = classicLanguage.Split('-');
            locale = new AndroidJavaObject("java.util.Locale", parts[0], parts[1]);
        }
        else locale = localeClass.CallStatic<AndroidJavaObject>("forLanguageTag", classicLanguage);
        _androidTTS.Call<int>("setLanguage", locale);
        _androidTTS.Call<int>("setSpeechRate", speechRate);
        _androidTTS.Call<int>("setPitch", speechPitch);
        _androidTTSReady = true;
        if (_pendingAndroidText != null)
        {
            var p = _pendingAndroidText; _pendingAndroidText = null;
            if (_speakCoroutine != null) StopCoroutine(_speakCoroutine);
            _speakCoroutine = StartCoroutine(SpeakAllSentences(p));
        }
    }
#endif

    private void SetBusy(bool busy)
    {
        if (_isBusy == busy) return;
        _isBusy = busy;
        if (_spiderTalk != null) _spiderTalk.isTalking = busy;
        Log($"isTalking → {busy}");
    }

    void OnDestroy()
    {
        if (_speakCoroutine != null) StopCoroutine(_speakCoroutine);
#if META_VOICE_SDK
        if (ttsSpeaker != null)
            ttsSpeaker.Events.OnPlaybackComplete.RemoveListener(OnTTSPlaybackComplete);
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_androidTTS != null) { _androidTTS.Call<int>("stop"); _androidTTS.Call("shutdown"); }
#endif
    }

    private void Log(string msg)        { if (showDebugLogs) Debug.Log($"[SpiderTalkTTS] {msg}"); }
    private void LogWarning(string msg) => Debug.LogWarning($"[SpiderTalkTTS] {msg}");
}

#if UNITY_ANDROID && !UNITY_EDITOR
public class AndroidTTSInitListener : AndroidJavaProxy
{
    private SpiderTalkTTS _owner;
    public AndroidTTSInitListener(SpiderTalkTTS owner)
        : base("android.speech.tts.TextToSpeech$OnInitListener") { _owner = owner; }
    public void onInit(int status)
    {
        UnityMainThreadDispatcher.Enqueue(() => _owner.OnAndroidTTSReady(status == 0));
    }
}
#endif