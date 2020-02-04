using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public static float score;
    public static float scoreIncrement;
    public static float scoreDecrement;
    public static int comboMultiplier;

    public Text txtScore;
    public Text txtScoreMult;
    public Text txtHighScore;
    public Text txtTimer;
    public float timeBeforeInstantiation;

    public GameObject[] objectsToInstantiate;

    float xBoundMin = -4.5f;
    float yBoundMax = 4.5f;
    float yBoundMin = -4.5f;
    float xBoundMax = 8.5f;

    public float highScore;

    public float gameLength;

    void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        score = 0;
        highScore = PlayerPrefs.GetFloat("highScore");
        txtHighScore.text = "Highscore : " + highScore.ToString();

        scoreIncrement = 1;
        scoreDecrement = 1;
        comboMultiplier = 1;
        SpawnObject();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu");

        }

        gameLength -= Time.deltaTime;
        txtTimer.text = ((int)gameLength/60).ToString() + ":" + ((int)gameLength%60).ToString("00");
        if(gameLength<=0)
        {
            if(score>PlayerPrefs.GetFloat("highScore"))
            {
                PlayerPrefs.SetFloat("highScore", score);
                PlayerPrefs.Save();
            } 
            SceneManager.LoadScene("Menu");
        }
    }

    public void SpawnObject()
    {
        int indexObject = Random.Range(0, objectsToInstantiate.Length);
        float xPos = Random.Range(xBoundMin, xBoundMax);
        float yPos = Random.Range(yBoundMin, yBoundMax);
        Vector3 pos = new Vector3(xPos, yPos, 10);
        GameObject go = Instantiate(objectsToInstantiate[indexObject],pos,Quaternion.identity);
        if(go.GetComponent<MoveTo>()!=null)
        {
            float xPosDest = Random.Range(xBoundMin, xBoundMax);
            float yPosDest = Random.Range(yBoundMin, yBoundMax);
            go.GetComponent<MoveTo>().destination = new Vector3(xPosDest, yPosDest, 10);
        }
        if(go.tag == "Target")
        {
            bool isRed = true;
            if(Random.value<=0.5f)
            {
                isRed = false;
            }
            go.GetComponent<TargetBehaviour>().isTargetR = isRed;
        }
        Invoke("SpawnObject", timeBeforeInstantiation);
    }

    public static void IncreaseScore()
    {
        score += comboMultiplier * scoreIncrement;
        instance.txtScore.text = "Score : " + score.ToString();
    }

    public static void DecreaseScore()
    {
        score -= scoreDecrement;
        instance.txtScore.text = "Score : " + score.ToString();
    }

    public static void IncreaseComboMultiplier()
    {
        comboMultiplier++;
        instance.txtScoreMult.text = "x" + comboMultiplier.ToString();
    }

    public static void ResetComboMultiplier()
    {
        comboMultiplier = 1;
        instance.txtScoreMult.text = "x" + comboMultiplier.ToString();
    }
}
