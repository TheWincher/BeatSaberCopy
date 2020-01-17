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

public class TP1 : MonoBehaviour
{

    VideoCapture webCam;
    Mat image, imgGray, imgHSV, flippedImg, gaussianImg, resMat, structElement;
    Image<Gray, Byte> resImg;
    int webCamID = 0;

    VectorOfVectorOfPoint contours;
    VectorOfPoint biggestContour;
    int biggestContourIndex;
    double biggestContourArea = 0;
    List<Point> lastsPoints;

    [Range(0,180)]
    public int seuilBas, seuilHaut;

    [Range(0, 255)]
    public int seuilSatBas, seuilSatHaut, seuilBriBas, seuilBriHaut;

    Texture2D texture, texture2;
    Sprite sprite, sprite2;
    // Start is called before the first frame update
    void Start()
    {
        structElement = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(2 * 1 + 1, 2 * 1 + 1), new Point(1, 1));
        contours = new VectorOfVectorOfPoint();
        lastsPoints = new List<Point>();

        //Use for load video
        //webCam = new VideoCapture("C:\\Users\\elouchez\\Videos\\2019-11-07 16-38-37.mp4");

        //Use webcam
        webCam = new VideoCapture(webCamID);
    }

    // Update is called once per frame
    void Update()
    {
        //Get image from webCam
        image = webCam.QueryFrame();

        //Clone img befor flip
        flippedImg = image.Clone();

        //Flip image
        CvInvoke.Flip(image, flippedImg, FlipType.Vertical);

        imgHSV = flippedImg.Clone();
        gaussianImg = flippedImg.Clone();
        resMat = image.Clone();

        //Convert BGR img to Gray img
        //CvInvoke.CvtColor(image, imgGray, ColorConversion.Bgr2Gray);

        //Convert BGR img to HSV img
        CvInvoke.CvtColor(image, imgHSV, ColorConversion.Bgr2Hsv);

        //Floutage
        //CvInvoke.MedianBlur(imgHSV, gaussianImg, 7);
        //CvInvoke.GaussianBlur(imgHSV, gaussianImg, new Size(5,5), 0.5);
        //CvInvoke.CvtColor(gaussianImg, resImg, ColorConversion.Hsv2Bgr);

        //Seuillage
        resImg =  imgHSV.ToImage<Hsv, Byte>().InRange(new Hsv(seuilBas,seuilSatBas,seuilBriBas), new Hsv(seuilHaut,seuilSatHaut,seuilBriHaut));
        imgGray = resImg.Mat;

        //Ouverture 
        CvInvoke.Erode(imgGray, resMat, structElement, new Point(-1, -1), 3, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Dilate(resMat, resMat, structElement, new Point(-1, -1), 3, BorderType.Constant, new MCvScalar(0));

        CvInvoke.FindContours(imgGray, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);

        if(contours.Size > 0)
        {
            biggestContour = contours[0];
            biggestContourIndex = 0;
            biggestContourArea = CvInvoke.ContourArea(contours[0]);
            for (int i = 1; i < contours.Size; i++)
            {
                //if (contours[i].Size > biggestContour.Size)
                if (CvInvoke.ContourArea(contours[i]) > biggestContourArea)
                {
                    biggestContour = contours[i];
                    biggestContourIndex = i;
                    biggestContourArea = CvInvoke.ContourArea(contours[i]);
                }
            }
            //CvInvoke.DrawContours(image, contours, biggestContourIndex, new MCvScalar(0, 0, 255));

            Point center = new Point(0, 0);
            for(int i = 0; i < biggestContour.Size; i++)
            {
                center.X += biggestContour[i].X;
                center.Y += biggestContour[i].Y;
            }

            center.X /= biggestContour.Size;
            center.Y /= biggestContour.Size;

            if (lastsPoints.Count >= 40)
                lastsPoints.RemoveAt(0);
            lastsPoints.Add(center);

            List<Point> region = croissanceRegion(center);
            Image<Bgr, Byte> imgRes = image.ToImage<Bgr, Byte>();
            foreach (Point p in region)
            {
                imgRes.Data[p.X, p.Y, 0] = 255;  
                imgRes.Data[p.X, p.Y, 1] = 0;  
                imgRes.Data[p.X, p.Y, 2] = 0;  
            }

            image = imgRes.Mat;

            foreach (Point p in lastsPoints)
            {
                CvInvoke.Circle(image, p, 2, new MCvScalar(0, 0, 0));
            }
        }

        CvInvoke.Imshow("View camera", image);
        CvInvoke.Imshow("Seuillage", resMat);


        //Show img on unity
        //Convert Mat into texture2D
        //texture = new Texture2D(flippedImg.Width, flippedImg.Height, TextureFormat.BGRA32, false);
        //texture.LoadRawTextureData(flippedImg.ToImage<Bgra, Byte>().Bytes);
        //texture.Apply();

        //texture2 = new Texture2D(resImg.Width, resImg.Height, TextureFormat.BGRA32, false);
        //texture2.LoadRawTextureData(resImg.Convert<Bgra,Byte>().Bytes);
        //texture2.Apply();

        //Create sprite with texture2D
        //sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1.0f);
        //sprite2 = Sprite.Create(texture2, new Rect(0.0f, 0.0f, texture2.width, texture2.height), new Vector2(0.5f, 0.5f), 1.0f);

        //Set img with new sprite
        //img.sprite = sprite;
        //go_traitement.GetComponent<Image>().sprite = sprite2;
    }

    private void OnDestroy()
    {
        webCam.Dispose();
        CvInvoke.DestroyAllWindows();
    }

    List<Point> croissanceRegion(Point seed)
    {
        Image<Hsv,Byte> img = image.ToImage<Hsv, Byte>();
        Queue<Point> S = GetNeighboors(seed);

        List<Point> region = new List<Point> { seed};
        float sumRegion = img.Data[seed.X, seed.Y, 0];
        float moy = 0;

        while(S.Count > 0)
        {
            Point p = S.Dequeue();
            moy = img.Data[p.X, p.Y, 0] - (sumRegion / (float) region.Count);
            if(moy < Mathf.Epsilon)
            {
                region.Add(p);
                sumRegion += img.Data[p.X, p.Y, 0];
                foreach(Point neighboor in GetNeighboors(p))
                {
                    if (!S.Contains(neighboor))
                        S.Enqueue(neighboor);
                }
            }
        }
        Debug.Log("fini");
        return region;
    }

    Queue<Point> GetNeighboors(Point seed, int connexite = 0)
    {
        Queue<Point> res = new Queue<Point>();

        if(seed.X - 1 >= 0)
            res.Enqueue(new Point(seed.X - 1, seed.Y));

        if (seed.X + 1 < image.Width)
            res.Enqueue(new Point(seed.X + 1, seed.Y));

        if (seed.Y - 1 >= 0)
            res.Enqueue(new Point(seed.X, seed.Y - 1));

        if (seed.Y + 1 < image.Height)
            res.Enqueue(new Point(seed.X, seed.Y + 1));

        if(connexite == 1)
        {
            if (seed.X - 1 >= 0 && seed.Y - 1 >= 0)
                res.Enqueue(new Point(seed.X - 1, seed.Y - 1));

            if (seed.X + 1 < image.Width && seed.Y - 1 >= 0)
                res.Enqueue(new Point(seed.X + 1, seed.Y - 1));

            if (seed.X - 1 >= 0 && seed.Y + 1 < image.Height)
                res.Enqueue(new Point(seed.X - 1, seed.Y + 1));

            if (seed.X + 1 < image.Width && seed.Y + 1 < image.Height)
                res.Enqueue(new Point(seed.X + 1, seed.Y + 1));
        }

        return res;
    }
}
