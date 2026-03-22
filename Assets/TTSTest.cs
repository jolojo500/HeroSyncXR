using UnityEngine;
using UnityEngine.InputSystem;

public class TTSTest : MonoBehaviour
{
    public SpiderTalkTTS spiderTTS;

    private string[] lines = new string[]
    {
        "Hey there, partner! Listen... I heard about you. Someone told me there's an incredibly brave hero here in this hospital. And honestly? I really need a teammate like you.",

        "Here's the thing. During my last battle, my suit got hit by a strange gadget. I can feel something is wrong somewhere on my body... but I just can't figure out where! It's like all my sensors are scrambled.",

        "I managed to project this copy of my suit. Because of the special bond between superheroes, this white armor feels exactly what YOU feel. If something stings or burns somewhere, the armor feels it too.",

        "You are the only one who can help me find where it hurts. Pick the symbol that looks most like what you feel, and use your web-shooter to draw right on my white suit where it says ouch. That way, I will know exactly where to fix my suit... so we can go on our mission together!"
    };

    private int currentLine = 0;

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (currentLine < lines.Length)
            {
                spiderTTS.Speak(lines[currentLine]);
                currentLine++;
            }
            else
            {
                // Recommence depuis le debut
                currentLine = 0;
                Debug.Log("[TTSTest] Script termine ! Appuyez encore pour recommencer.");
            }
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            spiderTTS.StopSpeaking();
            currentLine = 0;
            Debug.Log("[TTSTest] Reset - Reprise depuis le debut.");
        }
    }
}