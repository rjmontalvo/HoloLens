using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.WebCam;

public class Hud : MonoBehaviour
{
    public Text TranslatorBox;
    System.Threading.Timer _timer;
    // Start is called before the first frame update
    void Start()
    {
        TranslatorBox.text = "Starting...";

        int secondsInterval = 15;
        _timer = new System.Threading.Timer(Tick, null, 0, secondsInterval * 1000);


    }
    
    //Used for the timer
    private void Tick(object state)
    {
        AnalyzeScene();
    }

    void AnalyzeScene()
    {
        TranslatorBox.text = "Analyzing...";
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    #region PhotoCapture
    //Capture a photo of the scene
    PhotoCapture _photoCaptureObject = null;
    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        _photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        CameraParameters camPar = new CameraParameters();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if(result.success)
        {
            string filename = string.Format(@"text_analysis.jpg");
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            _photoCaptureObject.TakePhotoAsync(filename, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
        }
        else
        {
            TranslatorBox.text = "Unable to analyze";
        }
    }

    private void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if(result.success)
        {
            string filename = string.Format(@"text_analysis.jpg");
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            byte[] image = File.ReadAllBytes(filePath);
            GetTagsAndFaces(image);
            ReadWords(image); 
        }

        else
        {
            TranslatorBox.text = "Failed to save Photo to disk.";
        }
        _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        _photoCaptureObject.Dispose();
        _photoCaptureObject = null;
    }
    #endregion

    #region Cognitive Service Integration

    //string _subscriptionKey = //ENTER SUB KEY HERE
    string _ocrEndpoint = "https://westus.api.cognitive.microsoft.com/vision/v1.0/ocr";
    public void ReadWords(byte[] image)
    {
        coroutine = Read(image);
        StartCoroutine(coroutine);
    }

    IEnumerator Read(byte[] image)
    {
        var headers = new Dictionary<string, string>()
        {
            {"Ocp-Apim-Subscription-Key", _subscriptionKey },
            {"Content-Type", "application/octet-stream" }
        };

        WWW www = new WWW(_ocrEndpoint, image, headers);
        yield return www;

        List<string> words = new List<string>();
        var jsonResults = www.text;
        var myObject = JsonUtility.FromJson<OcrResults>(jsonResults);
        foreach(var region in myObject.regions)
            foreach(var lin in region.lines)
                foreach(var word in line.words)
                {
                    words.Add(word.text);
                }
        string textToRead = string.Join(" ", words.ToArray());
        if(myObject.language != "unk")
        {
            TranslatorBox.text = "(language =" + myObject.language + ")n" + textToRead;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
