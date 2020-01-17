using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenObstacles : MonoBehaviour
{

    public float timer;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("GenObs", 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenObs()
    {
        GameObject obs = Resources.Load<GameObject>("Obstacles/obstacles");
        Material mat = Resources.Load<Material>("Materials/Red");
        Instantiate(obs,transform);
        Invoke("GenObs", timer);
    }
}
