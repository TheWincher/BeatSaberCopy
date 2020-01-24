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
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;

public class SettingMenu : MonoBehaviour
{
    public Button backButton, saveButton;
    public GameObject MainMenu;

    public Slider colorSliderRed, satSliderRed, briSliderRed;
    public Slider colorSliderBlue, satSliderBlue, briSliderBlue;

    public Image handleRed, backgroundSliderRed;
    public Image handleBlue, backgroundSliderBlue;
    public Image imgBlue, imgRed, imgWebcam;

    VideoCapture webCam;
    Mat imgWebCam, matHSV, structElement;
    Image<Gray, Byte> imgGray;
    Texture2D texRed, texBlue, texWebcam;

    // Start is called before the first frame update
    void Start()
    {
        imgWebCam = new Mat();
        matHSV = new Mat();

        saveButton.onClick.AddListener(SaveSetting);
        backButton.onClick.AddListener(BackMain);

        colorSliderRed.onValueChanged.AddListener(delegate { ValueRedChangeCheck();});
        colorSliderBlue.onValueChanged.AddListener(delegate { ValueBlueChangeCheck(); });
        Color[] colors = new Color[]
        {
            new Color(1, 0, 0), new Color(1, 1, 0), new Color(0, 1, 0), new Color(0, 1, 1), new Color(0, 0, 1),
            new Color(1, 0, 1), new Color(1, 0, 0)
        };

        Texture2D texSlider = new Texture2D(colors.Length,1);
        texSlider.SetPixels(colors);
        texSlider.Apply();

        backgroundSliderRed.sprite = Sprite.Create(texSlider, new Rect(Vector2.zero,  new Vector2(colors.Length,1)), Vector2.one * 0.5f);
        backgroundSliderBlue.sprite = Sprite.Create(texSlider, new Rect(Vector2.zero,  new Vector2(colors.Length,1)), Vector2.one * 0.5f);

        structElement = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(2 * 1 + 1, 2 * 1 + 1), new Point(1, 1));

        webCam = new VideoCapture(0);
        webCam.ImageGrabbed += new EventHandler(HandleWebcamGrab);

        texWebcam = convertMatToTexture2D(imgWebCam, imgWebCam.Width, imgWebCam.Height);
        imgWebcam.sprite = Sprite.Create(texWebcam, new Rect(0f, 0f, texWebcam.width, texWebcam.height),  new Vector2(0.5f,0.5f), 100f);

        SetHandle();
    }

    // Update is called once per frame
    void Update()
    {
        if (webCam.IsOpened)
        {
            webCam.Grab();
        }
        else
        {
            return;
        }
    }

    void HandleWebcamGrab(object sender, EventArgs e)
    {
        if (webCam.IsOpened)
        {
            webCam.Retrieve(imgWebCam);
            texWebcam = convertMatToTexture2D(imgWebCam, imgWebCam.Width, imgWebCam.Height);
            imgWebcam.sprite = Sprite.Create(texWebcam, new Rect(0f, 0f, texWebcam.width, texWebcam.height), new Vector2(0.5f, 0.5f), 100f);

            CvInvoke.CvtColor(imgWebCam, matHSV, ColorConversion.Bgr2Hsv);
            CvInvoke.Flip(matHSV, matHSV, FlipType.Vertical);
            
            imgGray = matHSV.ToImage<Hsv, Byte>().InRange(new Hsv(nfmod(colorSliderRed.value - 10f, 180), satSliderRed.value, briSliderRed.value), new Hsv(nfmod(colorSliderRed.value + 10f, 180), 255, 255));
            //Ouverture 
            CvInvoke.Erode(imgGray, imgGray, structElement, new Point(-1, -1), 2, BorderType.Constant, new MCvScalar(0));
            CvInvoke.Dilate(imgGray, imgGray, structElement, new Point(-1, -1), 2, BorderType.Constant, new MCvScalar(0));
            
            texRed = convertMatToTexture2D(imgGray.Mat, imgWebCam.Width, imgGray.Height);
            imgRed.sprite = Sprite.Create(texRed, new Rect(0f, 0f, texRed.width, texRed.height), new Vector2(0.5f, 0.5f), 100f);

            imgGray = matHSV.ToImage<Hsv, Byte>().InRange(new Hsv(nfmod(colorSliderBlue.value - 10f, 180), satSliderBlue.value, briSliderRed.value), new Hsv(nfmod(colorSliderBlue.value + 10f, 180), 255, 255));
            //Ouverture 
            CvInvoke.Erode(imgGray, imgGray, structElement, new Point(-1, -1), 2, BorderType.Constant, new MCvScalar(0));
            CvInvoke.Dilate(imgGray, imgGray, structElement, new Point(-1, -1), 2, BorderType.Constant, new MCvScalar(0));

            texBlue = convertMatToTexture2D(imgGray.Mat, imgWebCam.Width, imgGray.Height);
            imgBlue.sprite = Sprite.Create(texBlue, new Rect(0f, 0f, texBlue.width, texBlue.height), new Vector2(0.5f, 0.5f), 100f);
        }
        else
        {
            return;
        }
    }

    Texture2D convertMatToTexture2D(Mat matImg, int width,  int height)
    {
        if(matImg.IsEmpty)
            return new Texture2D(width,height);

        CvInvoke.Resize(matImg, matImg, new Size(width,height));
        CvInvoke.Flip(matImg, matImg, FlipType.Vertical);

        Texture2D texture = new Texture2D(width,height,TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(matImg.ToImage<Rgba, Byte>().Bytes);
        texture.Apply();

        return texture;
    }

    public void ValueRedChangeCheck()
    {
        handleRed.color = Color.HSVToRGB(colorSliderRed.value / 180f, 1, 1);
    }

    public void ValueBlueChangeCheck()
    {
        handleBlue.color = Color.HSVToRGB(colorSliderBlue.value / 180f, 1, 1);
    }

    float nfmod(float a, float b)
    {
        return a - b * Mathf.Floor(a / b);
    }

    void OnDestroy()
    {
        webCam.Dispose();
        CvInvoke.DestroyAllWindows();
    }

    void SaveSetting()
    {
        PlayerPrefs.SetFloat("ColorRed", colorSliderRed.value);
        PlayerPrefs.SetFloat("SatRed", satSliderRed.value);
        PlayerPrefs.SetFloat("ValRed", briSliderRed.value);
        
        PlayerPrefs.SetFloat("ColorBlue", colorSliderBlue.value);
        PlayerPrefs.SetFloat("SatBlue", satSliderBlue.value);
        PlayerPrefs.SetFloat("ValBlue", briSliderBlue.value);

        PlayerPrefs.Save();
    }

    void SetHandle()
    {
        colorSliderRed.value = PlayerPrefs.GetFloat("ColorRed");
        satSliderRed.value = PlayerPrefs.GetFloat("SatRed");
        briSliderRed.value = PlayerPrefs.GetFloat("ValRed");

        colorSliderBlue.value = PlayerPrefs.GetFloat("ColorBlue");
        satSliderBlue.value = PlayerPrefs.GetFloat("SatBlue");
        briSliderBlue.value = PlayerPrefs.GetFloat("ValBlue");
    }

    void BackMain()
    {
       MainMenu.SetActive(true);
       gameObject.SetActive(false);
    }
}
