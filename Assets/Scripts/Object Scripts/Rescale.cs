using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rescale : MonoBehaviour
{
    private float initialScale;
    public float desiredScale;
    public float timeToScale;
    private float rescaleTimer;
    private float rescaleSpeed;
    private Vector3 rescaleVector;
    public bool isRescaling;

    // Start is called before the first frame update
    void Start()
    {
        initialScale = transform.localScale.x;
        rescaleSpeed = ((initialScale - desiredScale) / timeToScale);
        rescaleTimer = timeToScale;
        rescaleVector = new Vector3(rescaleSpeed, rescaleSpeed, rescaleSpeed);
        isRescaling = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(isRescaling)
        {
            transform.localScale -= rescaleVector * Time.deltaTime;
            if(transform.localScale.x<= desiredScale)
            {
                isRescaling = false;
            }
        }
        if (!isRescaling)
        {
            transform.localScale += rescaleVector * Time.deltaTime;
            if (transform.localScale.x >= initialScale)
            {
                isRescaling = true;
            }
        }

    }
}
