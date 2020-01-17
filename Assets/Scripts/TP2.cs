using System.Drawing;
using System.IO;
using System;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections.Generic;

public class TP2 : MonoBehaviour
{
    VideoCapture webCam;
    EventHandler eventHandler;
    Mat imgWebCam;
    // Start is called before the first frame update
    void Start()
    {
        imgWebCam = new Mat();
        webCam = new VideoCapture(0);
        webCam.ImageGrabbed += new EventHandler(handleWebcamGrab);
    }

    void handleWebcamGrab(object sender, EventArgs e)
    {
        if(webCam.IsOpened)
        {
            webCam.Retrieve(imgWebCam);
            CvInvoke.CvtColor(imgWebCam, imgWebCam, ColorConversion.Bgr2Gray);
            CvInvoke.Imshow("Cam", imgWebCam);
        }
        else 
        {
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(webCam.IsOpened)
        {
            webCam.Grab();
        }
        else 
        {
            return;
        }
    }

    void OnDestroy()
    {
        webCam.Dispose();
        CvInvoke.DestroyAllWindows();
    }
}
