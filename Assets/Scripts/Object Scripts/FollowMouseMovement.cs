using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouseMovement : MonoBehaviour
{

    public float depth;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = depth;
        Vector3 pos = Camera.main.ScreenToWorldPoint(mousePos);

        //pos = new Vector3(pos.x, pos.y, depth);
        gameObject.transform.position = pos;
    }
}
