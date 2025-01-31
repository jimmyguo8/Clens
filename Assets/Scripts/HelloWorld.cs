﻿using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.MixedReality.Toolkit.UI;

public class HelloWorld : MonoBehaviour
{
    // Hook up the three properties below with a Text, InputField and Button object in your UI.
    public InputField inputField;
    public PressableButtonHoloLens2 speakButton;
    public AudioSource audioSource;

    private object threadLocker = new object();
    private bool waitingForSpeak;
    private string message;

    private SpeechConfig speechConfig;
    private SpeechSynthesizer synthesizer;

    public void ButtonClick()
    {
        lock (threadLocker)
        {
            waitingForSpeak = true;
        }

        string newMessage = string.Empty;

        // Starts speech synthesis, and returns after a single utterance is synthesized.
        using (var result = synthesizer.SpeakTextAsync(inputField.text).Result)
        {
            // Checks result.
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                // Native playback is not supported on Unity yet (currently only supported on Windows/Linux Desktop).
                // Use the Unity API to play audio here as a short term solution.
                // Native playback support will be added in the future release.
                var sampleCount = result.AudioData.Length / 2;
                var audioData = new float[sampleCount];
                for (var i = 0; i < sampleCount; ++i)
                {
                    audioData[i] = (short)(result.AudioData[i * 2 + 1] << 8 | result.AudioData[i * 2]) / 32768.0F;
                }

                // The output audio format is 16K 16bit mono
                var audioClip = AudioClip.Create("SynthesizedAudio", sampleCount, 1, 16000, false);
                audioClip.SetData(audioData, 0);
                audioSource.clip = audioClip;
                audioSource.Play();

                newMessage = "Speech synthesis succeeded!";
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                newMessage = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
            }
        }

        lock (threadLocker)
        {
            message = newMessage;
            waitingForSpeak = false;
        }
    }

    void Start()
    {
        
        if (inputField == null)
        {
            message = "inputField property is null! Assign a UI InputField element to it.";
            UnityEngine.Debug.LogError(message);
        }
        else if (speakButton == null)
        {
            message = "speakButton property is null! Assign a UI Button to it.";
            UnityEngine.Debug.LogError(message);
        }
        else
        {
            // Continue with normal initialization, Text, InputField and Button objects are present.
            inputField.text = "Hello, how are you?";
            message = "Click button to synthesize speech";
            speakButton.GetComponent<Interactable>().OnClick.AddListener(ButtonClick);

            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").
            speechConfig = SpeechConfig.FromSubscription("f0001d2b3d7e4273b4184ace56002d32", "westus");

            // The default format is Riff16Khz16BitMonoPcm.
            // We are playing the audio in memory as audio clip, which doesn't require riff header.
            // So we need to set the format to Raw16Khz16BitMonoPcm.
            speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw16Khz16BitMonoPcm);

            // Creates a speech synthesizer.
            // Make sure to dispose the synthesizer after use!
            synthesizer = new SpeechSynthesizer(speechConfig, null);
        }
    }

    void Update()
    {
        lock (threadLocker)
        {
            if (speakButton != null)
            {
                speakButton.GetComponent<Interactable>().IsEnabled = !waitingForSpeak;
            }

        }
    }

    void OnDestroy()
    {
        synthesizer.Dispose();
    }

}
