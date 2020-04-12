using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class MicrophoneManager : MonoBehaviour
{
    //Help to access instance of this object
    public static MicrophoneManager instance;

    //AudioSource component, provides access to mic
    private AudioSource audioSource;

    //Flag indicating mic detection
    private bool microphoneDetected;

    //Component converting speech to text
    private DictationRecognizer dictationRecognizer;

    private void Awake()
    {
        //Set this class to behave like a singleton
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
     //Use Unity Microphone class to detect devices and setup AudioSource
        if(Microphone.devices.Length>0)
        {
            Results.instance.SetMicrophoneStatus("Initializing...");
            audioSource = GetComponent<AudioSource>();
            microphoneDetected = true;
        }
        else
        {
            Results.instance.SetMicrophoneStatus("No Microphone Detected");
        }
    }

    /// <summary>
    /// Start microphone capture. Debugging is delivered to the Results class
    /// </summary>
    public void StartCapturingAudio()
    {
        if(microphoneDetected)
        {
            //Start dictation
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
            dictationRecognizer.Start();

            //Update the UI with mic status
            Results.instance.SetMicrophoneStatus("Capturing...");
        }
    }

    /// <summary>
    /// Stop mic capture. Debugging message passed to Results class.
    /// </summary>
    public void StopCapturingAudio()
    {
        Results.instance.SetMicrophoneStatus("Mic Sleeping");
        Microphone.End(null);
        dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
        dictationRecognizer.Dispose();
    }

    /// <summary>
    /// This handler is called every time the Dictation detects a pause in the speech
    /// Debugging message passed to Results class
    /// </summary>
    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        //Update UI with dication captured
        Results.instance.SetDictationResult(text);

        //Start the coroutine that process the dictation through Azure
        StartCoroutine(Translator.instance.TranslateWithUnityNetworking(text));

    }
}
