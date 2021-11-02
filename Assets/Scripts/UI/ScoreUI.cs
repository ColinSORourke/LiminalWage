using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    private GameManager gameManager;
    private Text scoreText;

    private int addingScore;
    private int displayScore;

    private bool showingAddingScore;

    public void Construct(GameManager gameManager)
    {
        this.gameManager = gameManager;
        scoreText = gameObject.GetComponent<Text>();

        UpdateValue();
    }

    public void TryAddScore(int toAdd)
    {
        StartCoroutine(AddToScore(toAdd));
    }

    private void UpdateValue()
    {
        if(showingAddingScore)
        {
            scoreText.text = "Score: " + displayScore + "+ " + addingScore;
        }
        else
        {
            scoreText.text = "Score: " + displayScore;
        }
    }

    private IEnumerator AddToScore(int toAdd)
    {
        addingScore += toAdd;
        showingAddingScore = true;
        UpdateValue();
        yield return new WaitForSeconds(2f);
        showingAddingScore = false;
        while (displayScore != gameManager.GetScore())
        {
            addingScore--;
            displayScore++;
            UpdateValue();
            yield return new WaitForSeconds(0.01f);
        }
    }
}
