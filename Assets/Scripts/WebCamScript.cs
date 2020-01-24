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
    List<Vector2> lastsPointsR;
    List<Vector2> lastsPointsB;
    private Texture2D tex;
    public Image image;
    public float deadZone;
    public float thresholdArea;

    //public Vector3 hautRed, basRed, hautBlue, basBlue;
    //Red (20,100,100)min et (40,255,255) max
    //Blue (120,100,100)min et (130,255,255) max
    public UnityEngine.Color hautCouleur1,basCouleur1, hautCouleur2, basCouleur2;
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
        lastsPointsR = new List<Vector2>();
        lastsPointsB = new List<Vector2>();

        webCam = new VideoCapture(0);
        webCam.ImageGrabbed += new EventHandler(handleWebcamGrab);

        tex = new Texture2D(webCam.Width, webCam.Height, TextureFormat.BGRA32, false);
    }

    void handleWebcamGrab(object sender, EventArgs e)
    {
        if(webCam.IsOpened)
        {
            webCam.Retrieve(imgWebCam);
            CvInvoke.CvtColor(imgWebCam, imgWebCamHSV, ColorConversion.Bgr2Hsv);

            GetContourBlue();
            GetContourRed();
            GetCentroid();


            //CvInvoke.Imshow("Cam", imgWebCam);
            CvInvoke.Flip(imgWebCam, imgWebCam, FlipType.Vertical);
            CvInvoke.Flip(imgWebCam, imgWebCam, FlipType.Horizontal);
            
            tex.LoadRawTextureData(imgWebCam.ToImage<Bgra, byte>().Bytes);
            tex.Apply();
            image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1.0f);
        }
        else 
        {
            return;
        }
    }

    void GetContourBlue()
    {

        //Seuillage
        Vector3 colorhaut2HSV = new Vector3();
        UnityEngine.Color.RGBToHSV(hautCouleur2, out colorhaut2HSV.x, out colorhaut2HSV.y, out colorhaut2HSV.z);
        colorhaut2HSV *= 255f;

        Vector3 colorBas2HSV = new Vector3();
        UnityEngine.Color.RGBToHSV(basCouleur2, out colorBas2HSV.x, out colorBas2HSV.y, out colorBas2HSV.z);
        colorBas2HSV *= 255f;

        imgGray = imgWebCamHSV.ToImage<Hsv, Byte>().InRange(new Hsv(colorBas2HSV.x/2f, colorBas2HSV.y, colorBas2HSV.z), new Hsv(colorhaut2HSV.x/2f, colorhaut2HSV.y, colorhaut2HSV.z));
        imgWebCamGray = imgGray.Mat;

        //Ouverture 
        CvInvoke.Erode(imgWebCamGray, resMat, structElement, new Point(-1, -1), 7, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Dilate(resMat, resMat, structElement, new Point(-1, -1), 7, BorderType.Constant, new MCvScalar(0));

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

        CvInvoke.DrawContours(imgWebCam, contoursBlue, biggestContourBlueIndex, new MCvScalar(255, 0, 0),3);
        //CvInvoke.Imshow("Cam Blue", imgGray);
    }

    void GetContourRed()
    {
        //Seuillage
        Vector3 colorhaut1HSV = new Vector3();
        UnityEngine.Color.RGBToHSV(hautCouleur1, out colorhaut1HSV.x, out colorhaut1HSV.y, out colorhaut1HSV.z);
        colorhaut1HSV *= 255f;
        
        Vector3 colorBas1HSV = new Vector3();
        UnityEngine.Color.RGBToHSV(basCouleur1, out colorBas1HSV.x, out colorBas1HSV.y, out colorBas1HSV.z);
        colorBas1HSV *= 255f;

        imgGray = imgWebCamHSV.ToImage<Hsv, Byte>().InRange(new Hsv(colorBas1HSV.x, colorBas1HSV.y, colorBas1HSV.z), new Hsv(colorhaut1HSV.x, colorhaut1HSV.y, colorhaut1HSV.z));
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

        CvInvoke.DrawContours(imgWebCam, contoursRed, biggestContourRedIndex, new MCvScalar(0, 0, 255),3);
        //CvInvoke.Imshow("Cam Red", imgGray);
    }

    void GetCentroid()
    {
        Moments blueMoment = new Moments();
        Moments redMoment = new Moments();

        if (biggestContourBlue != null)
            blueMoment = CvInvoke.Moments(biggestContourBlue);

        if (biggestContourRed != null)
            redMoment = CvInvoke.Moments(biggestContourRed);

        centroidBlue = new Point((int)(blueMoment.M10 / blueMoment.M00), (int)(blueMoment.M01 / blueMoment.M00));
        centroidRed = new Point((int)(redMoment.M10 / redMoment.M00), (int)(redMoment.M01 / redMoment.M00));

        CvInvoke.Circle(imgWebCam, centroidBlue, 2, new MCvScalar(0, 0, 0));
        CvInvoke.Circle(imgWebCam, centroidRed, 2, new MCvScalar(0, 0, 0));

        //RED
        float xR = ((float)centroidRed.X / (float)webCam.Width);
        float yR = ((float)centroidRed.Y / (float)webCam.Height);
        xR = 1f - xR;
        yR = 1f - yR;
        Vector2 centroidR = new Vector2(xR, yR);
        
        if (lastsPointsR.Count > 1)
        {
            Vector2 lastCentroid = lastsPointsR[lastsPointsR.Count - 1];

            float distance = Vector2.Distance(centroidR, lastCentroid);
            if (distance > deadZone)
            {
                lastsPointsR.Add(centroidR);
            }
            else
            {
                centroidR = lastCentroid;
            }
        }
        else
        {
            lastsPointsR.Add(centroidR);
        }

        if (lastsPointsR.Count > 40)
        {
            lastsPointsR.RemoveAt(0);
        }        
        //x 10 -10
        //y 7 -7
        //pour du 16/9
        float posXR = centroidR.x * 20f - 10f;
        float posYR = centroidR.y * 14f - 7f;
        if (biggestContourRed !=null && CvInvoke.ContourArea(biggestContourRed) > thresholdArea)
        {
            GameObject.Find("CapsuleRed").transform.position = new Vector3(posXR, posYR, 0);
        }
        

        //BLUE
        float xB = ((float)centroidBlue.X / (float)webCam.Width);
        float yB = ((float)centroidBlue.Y / (float)webCam.Height);
        xB = 1f - xB;
        yB = 1f - yB;
        Vector2 centroidB = new Vector2(xB, yB);

        if (lastsPointsB.Count > 1)
        {
            Vector2 lastCentroid = lastsPointsB[lastsPointsB.Count - 1];

            float distance = Vector2.Distance(centroidB, lastCentroid);
            if (distance > deadZone)
            {
                lastsPointsB.Add(centroidB);
            }
            else
            {
                centroidB = lastCentroid;
            }
        }
        else
        {
            lastsPointsB.Add(centroidB);
        }

        if (lastsPointsB.Count > 40)
        {
            lastsPointsB.RemoveAt(0);
        }
        if (centroidB.x > 1000f)
        {
            centroidB = new Vector2(0.5f, 0.5f);
        }
        float posXB = centroidB.x * 20f - 10f;
        float posYB = centroidB.y * 14f - 7f;
        
        GameObject.Find("CapsuleBlue").transform.position = new Vector3(posXB, posYB, 0);
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

