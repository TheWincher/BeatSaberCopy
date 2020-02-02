using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotationSpeed;
    public float rotationTime;
    private float rotationTimer;
    public float pauseTime;
    private float pauseTimer;
    public bool isPaused;

    public Vector3 rotationAxis;
    // Start is called before the first frame update
    void Start()
    {
        rotationTimer = rotationTime;
        pauseTimer = pauseTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isPaused)
        {
            transform.Rotate(rotationAxis, Time.deltaTime * rotationSpeed);
            rotationTimer -= Time.deltaTime;
            if (rotationTimer <= 0)
            {
                rotationTimer = rotationTime;
                isPaused = true;
            }
        }
        else
        {
            pauseTimer -= Time.deltaTime;
            if(pauseTimer<=0)
            {
                pauseTimer = pauseTime;
                isPaused = false;
            }
        }
    }
}
