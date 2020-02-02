using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBehaviour : MonoBehaviour
{
    public Color color;
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


    // Start is called before the first frame update
    void Start()
    {
        objectMaterial = GetComponent<Renderer>().material;
        Debug.Log(GetComponentsInChildren<Renderer>().Length); // TODO : faire en sorte que la couleur change bien sur tous les renderers
        foreach(Renderer r in GetComponentsInChildren<Renderer>())
        {
            Debug.Log(r.name);
            r.material.color = color;
        }

        GetComponent<LineRenderer>().material.color = color;
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
            objectMaterial.color = new Color(color.r / maxActiveTimer, color.g, color.b, activeTimer / maxActiveTimer);
            objectMaterial.SetColor("_EmissionColor", new Color(color.r / maxActiveTimer, color.g, color.b, activeTimer / maxActiveTimer));
        }
    }
    
    public void OnTriggerStay(Collider collision)
    {
        if (collision.tag == "Player")
        {
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
