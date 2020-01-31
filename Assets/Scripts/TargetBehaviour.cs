using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBehaviour : MonoBehaviour
{
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
        float activationImageScale = activationImageMinScale + (activationTimer / maxActivationTimer);
        activationImage.transform.localScale = new Vector3(activationImageScale, activationImageScale, activationImageScale);
        imageMaterial = activationImage.GetComponent<Renderer>().material;
        imageMaterial.color = new Color(1.0f/activationImageScale, 0, 0, 1.0f/activationImageScale);
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
            imageMaterial.color = new Color(1.0f / activationImageScale, 0, 0, 1.0f / activationImageScale);
        }
        else
        {
            activeTimer -= Time.deltaTime;
            objectMaterial.color = new Color(activeTimer / maxActiveTimer, 0, 0, activeTimer / maxActiveTimer);
            objectMaterial.SetColor("_EmissionColor",new Color(activeTimer/maxActiveTimer, 0, 0));
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
        objectMaterial.color = (Color.red);
        objectMaterial.EnableKeyword("_EMISSION");
        objectMaterial.SetColor("_EmissionColor", Color.red);
        GetComponent<Collider>().enabled = true;
        yield return new WaitForSeconds(maxActiveTimer);
        Destroy(gameObject);
    }
}
