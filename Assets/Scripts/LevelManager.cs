using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public float timeBeforeInstantiation;


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
        scoreIncrement = 1;
        scoreDecrement = 1;
        comboMultiplier = 1;
    }

    // Update is called once per frame
    void Update()
    {

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
