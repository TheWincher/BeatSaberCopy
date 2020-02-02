using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTo : MonoBehaviour
{

    public Vector3 destination;
    public float timeToMove;
    public GameObject target;
    private Vector3 direction;
    private float speed;

    // Start is called before the first frame update
    void Start()
    {

        target.transform.position = destination;
        direction = destination - transform.position;
        speed = direction.magnitude / timeToMove;
        direction.Normalize();

        GetComponent<LineRenderer>().SetPosition(0,gameObject.transform.position);
        GetComponent<LineRenderer>().SetPosition(1, destination);

        Rotate rot = target.GetComponent<Rotate>();
        rot.rotationAxis = Vector3.forward;
        rot.rotationSpeed = 360;
        rot.rotationTime = timeToMove;
        rot.pauseTime = 0;
        rot.isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        GetComponent<LineRenderer>().SetPosition(0, gameObject.transform.position);

        target.transform.position -= direction * speed * Time.deltaTime;
    }
}
