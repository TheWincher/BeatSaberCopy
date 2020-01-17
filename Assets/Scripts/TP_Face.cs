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

public class TP_Face : MonoBehaviour
{
    VideoCapture webCam;
    EventHandler eventHandler;
    Mat imgWebCam, imgWebGray;
    CascadeClassifier frontFacesCascadeClassifier;
    string pathFrontFacesCascadesClassifer = "/Haarcascade/lbpcascade_frontalface_improved.xml";
    Rectangle[] frontFaces;
    int MIN_FACE_SIZE = 50, MAX_FACE_SIZE = 200; 

    // Start is called before the first frame update
    void Start()
    {
        imgWebCam = new Mat();
        imgWebGray = new Mat();

        webCam = new VideoCapture(0);
        webCam.ImageGrabbed += new EventHandler(handleWebcamGrab);

        frontFacesCascadeClassifier = new CascadeClassifier(Application.dataPath + pathFrontFacesCascadesClassifer);
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
            CvInvoke.Imshow("Cam Grey", imgWebGray);

            frontFaces = frontFacesCascadeClassifier.DetectMultiScale(imgWebGray, 1.1, 5, new Size(MIN_FACE_SIZE, MIN_FACE_SIZE), new Size(MAX_FACE_SIZE,MAX_FACE_SIZE));
            
            for(int i = 0; i < frontFaces.Length; i++)
            {
                CvInvoke.Rectangle(imgWebCam, frontFaces[i], new MCvScalar(0, 180, 0), 5);
            }
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
