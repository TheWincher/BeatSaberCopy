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
using System.Collections;

public class TargetBehaviour : MonoBehaviour
{
    public UnityEngine.Color color;
    public Collider trigger;
    public float activationTimer;
    public float maxActivationTimer;
    public float activeTimer;
    public float maxActiveTimer;
    public GameObject activationImage;
    public float activationImageMinScale;
    Material imageMaterial;
    Material objectMaterial;
    public float comboTimer;
    public float playerOnTargetTimer;

    public bool isTargetR;
    // Start is called before the first frame update
    void Start()
    {
        objectMaterial = GetComponent<Renderer>().material;
        Debug.Log(GetComponentsInChildren<Renderer>().Length);
        if(isTargetR)
        {
            Hsv HSVR = new Hsv(PlayerPrefs.GetFloat("ColorRed"), PlayerPrefs.GetFloat("SatRed"), PlayerPrefs.GetFloat("ValRed"));
            color = UnityEngine.Color.HSVToRGB((float)HSVR.Hue / 180f, 1, 1, true);

        }
        else
        {
            Hsv HSVB = new Hsv(PlayerPrefs.GetFloat("ColorBlue"), PlayerPrefs.GetFloat("SatBlue"), PlayerPrefs.GetFloat("ValBlue"));
            color = UnityEngine.Color.HSVToRGB((float)HSVB.Hue / 180f, 1, 1, true);
        }


        foreach(Renderer r in GetComponentsInChildren<Renderer>())
        {
            Debug.Log(r.name);
            r.material.color = color;
        }

        float activationImageScale = activationImageMinScale + (activationTimer / maxActivationTimer);
        activationImage.transform.localScale = new Vector3(activationImageScale, activationImageScale, activationImageScale);
        imageMaterial = activationImage.GetComponent<Renderer>().material;
        imageMaterial.color = color/activationImageScale;
        StartCoroutine("DestroyAfterTime");
    }

    // Update is called once per frame
    void Update()
    {
        activationTimer -= Time.deltaTime;
        if (activationTimer > 0)
        {
            float activationImageScale = activationImageMinScale + (activationTimer / maxActivationTimer);
            activationImage.transform.localScale = new Vector3(activationImageScale, activationImageScale, activationImageScale);
            imageMaterial.color = color / activationImageScale;
        }
        else
        {
            activeTimer -= Time.deltaTime;
            objectMaterial.color = new UnityEngine.Color(color.r / maxActiveTimer, color.g, color.b, activeTimer / maxActiveTimer);
            objectMaterial.SetColor("_EmissionColor", new UnityEngine.Color(color.r / maxActiveTimer, color.g, color.b, activeTimer / maxActiveTimer));
        }
    }
    
    public void OnTriggerStay(Collider collision)
    {
        if (collision.tag == "Player")
        {
            if((collision.name == "SphereRed"&& isTargetR)|| (collision.name == "SphereBlue" && !isTargetR))
            LevelManager.IncreaseScore();
            playerOnTargetTimer += Time.deltaTime;
            if(playerOnTargetTimer >= comboTimer)
            {
                playerOnTargetTimer = 0;
                LevelManager.IncreaseComboMultiplier();
            }
        }
    }

    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(maxActivationTimer);
        Destroy(activationImage);
        objectMaterial.color = (color);
        objectMaterial.EnableKeyword("_EMISSION");
        objectMaterial.SetColor("_EmissionColor", color);
        GetComponent<Collider>().enabled = true;
        yield return new WaitForSeconds(maxActiveTimer);
        Destroy(gameObject);
    }
}
