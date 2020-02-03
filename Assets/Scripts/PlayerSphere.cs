using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSphere : MonoBehaviour
{
    public bool isRed;
    public Color sphereColor;
    // Start is called before the first frame update
    void Start()
    {

        if(isRed)
        {
            sphereColor = Color.HSVToRGB(PlayerPrefs.GetFloat("ColorRed"),PlayerPrefs.GetFloat("SatRed"),PlayerPrefs.GetFloat("ValRed"));
            GetComponent<Renderer>().material.color = sphereColor;

        }
        else
        {
            sphereColor = Color.HSVToRGB(PlayerPrefs.GetFloat("ColorBlue"), PlayerPrefs.GetFloat("SatBlue"), PlayerPrefs.GetFloat("ValBlue"));
            GetComponent<Renderer>().material.color = sphereColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
