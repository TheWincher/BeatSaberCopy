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

public class TP_Marqueur : MonoBehaviour
{
    VideoCapture webCam;
    EventHandler eventHandler;
    Mat imgWebCam, imgWebGray, imgWebSeg;

    [Range(0, 255)]
    public int valSeg; 

    [Range(0,10)]
    public int valSub;

    // Start is called before the first frame update
    void Start()
    {
        imgWebCam = new Mat();
        imgWebGray = new Mat();
        imgWebSeg = new Mat();

        webCam = new VideoCapture(0);
        webCam.ImageGrabbed += new EventHandler(handleWebcamGrab);
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

    void handleWebcamGrab(object sender, EventArgs e)
    {
        if(webCam.IsOpened)
        {
            webCam.Retrieve(imgWebCam);

            CvInvoke.CvtColor(imgWebCam, imgWebGray, ColorConversion.Bgr2Gray);
            CvInvoke.AdaptiveThreshold(imgWebGray, imgWebSeg, valSeg, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 5, valSub);
            CvInvoke.Imshow("Cam seg", imgWebSeg);

            CvInvoke.Imshow("Cam", imgWebCam);
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
