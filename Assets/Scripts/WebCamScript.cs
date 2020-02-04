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

    GameObject sphereRed, sphereBlue;


    //public Vector3 hautRed, basRed, hautBlue, basBlue;
    //Red (20,100,100)min et (40,255,255) max
    //Blue (120,100,100)min et (130,255,255) max
    public Hsv hautCouleur1,basCouleur1, hautCouleur2, basCouleur2;
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
        sphereRed = GameObject.Find("SphereRed");
        sphereBlue = GameObject.Find("SphereBlue");

        
        Hsv HSVR = new Hsv(PlayerPrefs.GetFloat("ColorRed"), PlayerPrefs.GetFloat("SatRed"), PlayerPrefs.GetFloat("ValRed"));
        Hsv HSVB = new Hsv(PlayerPrefs.GetFloat("ColorBlue"), PlayerPrefs.GetFloat("SatBlue"), PlayerPrefs.GetFloat("ValBlue"));
        
        
        //Confirmation des couleurs
        Material matRed = sphereRed.GetComponent<MeshRenderer>().material;
        matRed.color = UnityEngine.Color.HSVToRGB((float)HSVR.Hue/180f,1,1,true);
        Material matBlue = sphereBlue.GetComponent<MeshRenderer>().material;
        matBlue.color = UnityEngine.Color.HSVToRGB((float)HSVB.Hue / 180f, 1,1, true);



        Hsv seuil = new Hsv(10f, 10f, 0);

        hautCouleur1 = new Hsv(nfmod(HSVR.Hue + seuil.Hue, 180d), 255f, 255f);
        basCouleur1 = new Hsv(nfmod(HSVR.Hue - seuil.Hue, 180d), HSVR.Satuation, HSVR.Value);
        hautCouleur2 = new Hsv(nfmod(HSVB.Hue + seuil.Hue, 180d), 255, 255);
        basCouleur2 = new Hsv(nfmod(HSVB.Hue - seuil.Hue, 180d), HSVB.Satuation, HSVB.Value);
    }

    void handleWebcamGrab(object sender, EventArgs e)
    {
        if(webCam.IsOpened)
        {
            //On récupère l image de la webcam
            webCam.Retrieve(imgWebCam);
            //Conversion en hsv
            CvInvoke.CvtColor(imgWebCam, imgWebCamHSV, ColorConversion.Bgr2Hsv);

            //Récupération des contours
            GetContourBlue();
            GetContourRed();
            GetCentroid();

            //Flip et affichage de la webcam dans une image2D
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
        //On converti notre image en nuance de gris en seuillant selon notre couleur
        imgGray = imgWebCamHSV.ToImage<Hsv, Byte>().InRange(basCouleur2, hautCouleur2);
        imgWebCamGray = imgGray.Mat;

        //Erosion et dilatation de l image pour enlever des pixel isolé
        CvInvoke.Erode(imgWebCamGray, imgWebCamGray, structElement, new Point(-1, -1), 7, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Dilate(imgWebCamGray, imgWebCamGray, structElement, new Point(-1, -1), 7, BorderType.Constant, new MCvScalar(0));

        //Trouve les contours dans l'image
        CvInvoke.FindContours(imgWebCamGray, contoursBlue, null, RetrType.List, ChainApproxMethod.ChainApproxNone);

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

        //On dessine les contours
        CvInvoke.DrawContours(imgWebCam, contoursBlue, biggestContourBlueIndex, new MCvScalar(255, 0, 0),3);
    }

    void GetContourRed()
    {
        //Meme chose que GetContourBlue mais avec un seuillage différent
        imgGray = imgWebCamHSV.ToImage<Hsv, Byte>().InRange(basCouleur1, hautCouleur1);
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
    }

    void GetCentroid()
    {
        //On récupère le plus grand contours de chaque couleurs
        Moments blueMoment = new Moments();
        Moments redMoment = new Moments();

        if (biggestContourBlue != null)
            blueMoment = CvInvoke.Moments(biggestContourBlue);

        if (biggestContourRed != null)
            redMoment = CvInvoke.Moments(biggestContourRed);

        //On calcule le centroid de chacun de nos contours
        centroidBlue = new Point((int)(blueMoment.M10 / blueMoment.M00), (int)(blueMoment.M01 / blueMoment.M00));
        centroidRed = new Point((int)(redMoment.M10 / redMoment.M00), (int)(redMoment.M01 / redMoment.M00));

        //On dessine un cercle sur le centroid (dans l'image)
        CvInvoke.Circle(imgWebCam, centroidBlue, 2, new MCvScalar(0, 0, 0));
        CvInvoke.Circle(imgWebCam, centroidRed, 2, new MCvScalar(0, 0, 0));

        //RED
        //On normalise le centroid en [0...1]
        float xR = ((float)centroidRed.X / (float)webCam.Width);
        float yR = ((float)centroidRed.Y / (float)webCam.Height);
        xR = 1f - xR;//Inversion de l'axe X de la caméra
        //yR = 1f - yR;
        Vector2 centroidR = new Vector2(xR, yR);
        
        //Vérification de la distance des points pour éviter le tremblotement lorsque l on tient un objet
        //On joue maintenant sur la table, donc le code est moins utile
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

        //LastPointsR et B contienne les 40 dernieres position de notre centroid (C'était une question que vous avez posé en cours)

        //pour du 16/9
        //On ramene la position de 0..1 en coordonnés pour le jeux compris entre -10 et 10 en X et -7 et 7 en Y
        float posXR = centroidR.x * 20f - 10f;
        float posYR = centroidR.y * 14f - 7f;

        //on change la position de la sphère en jeu selon la position référence de notre objet sur la caméra
        sphereRed.transform.position = new Vector3(posXR, posYR, 10);
            

        //BLUE
        //Même chose pour le bleu
        float xB = ((float)centroidBlue.X / (float)webCam.Width);
        float yB = ((float)centroidBlue.Y / (float)webCam.Height);
        xB = 1f - xB;
        //yB = 1f - yB;
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
        
        sphereBlue.transform.position = new Vector3(posXB, posYB, 10);
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

        GetCentroid();
    }

    void OnDestroy()
    {
        webCam.Dispose();
        CvInvoke.DestroyAllWindows();
    }

    double nfmod(double a, double b)
    {
        return a - b * Mathf.Floor((float)(a / b));
    }

    float nfmod(float a, float b)
    {
        return a - b * Mathf.Floor(a / b);
    }
}

