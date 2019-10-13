using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.MixedReality.Toolkit.UI;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
#if PLATFORM_IOS
using UnityEngine.iOS;
using System.Collections;
#endif

public class SpeechToTextCont : MonoBehaviour
{
    public Text outputText;
    public PressableButtonHoloLens2 recoButton;
    SpeechRecognizer recognizer;
    SpeechConfig config;

    private object threadLocker = new object();
    private bool recognitionStarted = false;
    private string message;

    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {
        lock (threadLocker)
        {
            message = e.Result.Text;  
            //message = "";
        }
    }

    public async void ButtonClick()
    {
        if (recognitionStarted)
        {
            UnityEngine.Debug.Log("Recognition ended");
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            lock (threadLocker)
            {
                recognitionStarted = false;
            }
        }
        else
        {
            UnityEngine.Debug.Log("Recognition started");
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            lock (threadLocker)
            {
                recognitionStarted = true;
            }
        }
    }

    void Start()
    {
        //recoButton.GetComponent<Interactable>().OnClick.AddListener(ButtonClick);
        config = SpeechConfig.FromSubscription("f0001d2b3d7e4273b4184ace56002d32", "westus");
        recognizer = new SpeechRecognizer(config);
        recognizer.Recognizing += RecognizingHandler;
    }

    void Disable()
    {
        recognizer.Recognizing -= RecognizingHandler;
        recognizer.Dispose();
    }

    void Update()
    {
        lock (threadLocker)
        {
            if (outputText != null)
            {
                //UnityEngine.Debug.Log("Recording");
                outputText.text =  message;
            }
        }
    }
}