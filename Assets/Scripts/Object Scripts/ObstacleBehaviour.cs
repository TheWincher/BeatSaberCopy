using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBehaviour : MonoBehaviour
{

    public float timeUntilDestroy;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timeUntilDestroy -= Time.deltaTime;
        if(timeUntilDestroy <=0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            LevelManager.DecreaseScore();
            LevelManager.ResetComboMultiplier();
        }
    }
}
