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

public class WebCamScript : MonoBehaviour
{
    VideoCapture webCam;
    EventHandler eventHandler;
    Mat imgWebCam, imgWebCamHSV, imgWebCamGray, structElement, resMat;
    Image<Gray,Byte> imgGray;
    VectorOfVectorOfPoint contoursBlue, contoursRed;
    VectorOfPoint biggestContourBlue, biggestContourRed;
    Point centroidRed, centroidBlue;
    int biggestContourBlueIndex, biggestContourRedIndex;
    double biggestContourBlueArea = 0, biggestContourRedArea = 0;
    List<Point> lastsPoints;

    public Vector3 hautRed, basRed, hautBlue, basBlue;
    // Start is called before the first frame update
    void Start()
    {
        imgWebCam = new Mat();
        imgWebCamHSV = new Mat();
        imgWebCamGray = new Mat();
        resMat = new Mat();

        structElement = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(2 * 1 + 1, 2 * 1 + 1), new Point(1, 1));
        contoursBlue = new VectorOfVectorOfPoint();
        contoursRed = new VectorOfVectorOfPoint();
        lastsPoints = new List<Point>();

        webCam = new VideoCapture(0);
        webCam.ImageGrabbed += new EventHandler(handleWebcamGrab);
    }

    void handleWebcamGrab(object sender, EventArgs e)
    {
        if(webCam.IsOpened)
        {
            webCam.Retrieve(imgWebCam);
            CvInvoke.CvtColor(imgWebCam, imgWebCamHSV, ColorConversion.Bgr2Hsv);

            GetContourBlue();
            GetContourRed();

            CvInvoke.Imshow("Cam", imgWebCam);
        }
        else 
        {
            return;
        }
    }

    void GetContourBlue()
    {
        //Seuillage
        imgGray = imgWebCamHSV.ToImage<Hsv, Byte>().InRange(new Hsv(basBlue.x, basBlue.y, basBlue.z), new Hsv(hautBlue.x, hautBlue.y, hautBlue.z));
        imgWebCamGray = imgGray.Mat;

        //Ouverture 
        CvInvoke.Erode(imgWebCamGray, resMat, structElement, new Point(-1, -1), 3, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Dilate(resMat, resMat, structElement, new Point(-1, -1), 3, BorderType.Constant, new MCvScalar(0));

        //Trouve les contours dans l'image
        CvInvoke.FindContours(imgGray, contoursBlue, null, RetrType.List, ChainApproxMethod.ChainApproxNone);

        //On récupère le plus grand contour
        if(contoursBlue.Size > 0)
        {
            biggestContourBlue = contoursBlue[0];
            biggestContourBlueIndex = 0;
            biggestContourBlueArea = CvInvoke.ContourArea(contoursBlue[0]);
            for (int i = 1; i < contoursBlue.Size; i++)
            {
                if (CvInvoke.ContourArea(contoursBlue[i]) > biggestContourBlueArea)
                {
                    biggestContourBlue = contoursBlue[i];
                    biggestContourBlueIndex = i;
                    biggestContourBlueArea = CvInvoke.ContourArea(contoursBlue[i]);
                }
            }
        }

        CvInvoke.DrawContours(imgWebCam, contoursBlue, biggestContourBlueIndex, new MCvScalar(0, 0, 255));
        CvInvoke.Imshow("Cam Blue", imgGray);
    }

    void GetContourRed()
    {
        //Seuillage
        imgGray = imgWebCamHSV.ToImage<Hsv, Byte>().InRange(new Hsv(basRed.x, basRed.y, basRed.z), new Hsv(hautRed.x, hautRed.y, hautRed.z));
        imgWebCamGray = imgGray.Mat;

        //Ouverture 
        CvInvoke.Erode(imgWebCamGray, resMat, structElement, new Point(-1, -1), 3, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Dilate(resMat, resMat, structElement, new Point(-1, -1), 3, BorderType.Constant, new MCvScalar(0));

        //Trouve les contours dans l'image
        CvInvoke.FindContours(imgGray, contoursRed, null, RetrType.List, ChainApproxMethod.ChainApproxNone);

        //On récupère le plus grand contour
        if(contoursRed.Size > 0)
        {
            biggestContourRed = contoursRed[0];
            biggestContourRedIndex = 0;
            biggestContourRedArea = CvInvoke.ContourArea(contoursRed[0]);
            for (int i = 1; i < contoursRed.Size; i++)
            {
                if (CvInvoke.ContourArea(contoursRed[i]) > biggestContourRedArea)
                {
                    biggestContourRed = contoursRed[i];
                    biggestContourRedIndex = i;
                    biggestContourRedArea = CvInvoke.ContourArea(contoursRed[i]);
                }
            }
        }

        CvInvoke.DrawContours(imgWebCam, contoursRed, biggestContourRedIndex, new MCvScalar(0, 0, 255));
        CvInvoke.Imshow("Cam Red", imgGray);
    }

    void GetCentroid()
    {
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

