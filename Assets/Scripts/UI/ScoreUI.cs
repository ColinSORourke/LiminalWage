using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    private GameManager gameManager;
    private Text scoreText;

    public void Construct(GameManager gameManager)
    {
        this.gameManager = gameManager;
        scoreText = gameObject.GetComponent<Text>();

        UpdateValue();
    }

    public void UpdateValue()
    {
        scoreText.text = "Score: " + gameManager.GetScore().ToString();
    }
}
