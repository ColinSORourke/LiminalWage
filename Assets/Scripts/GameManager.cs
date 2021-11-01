using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Customer;

public class GameManager : MonoBehaviour
{
    private int score;

    private ScoreUI scoreUI;
    private PlayerManager playerManager;
    private CustomerManager customerManager;

    private void Awake()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        playerManager.Construct(this);

        customerManager = FindObjectOfType<CustomerManager>();
        customerManager.Construct(playerManager.GetDeliver());

        scoreUI = FindObjectOfType<ScoreUI>();
        scoreUI.Construct(this);
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreUI.UpdateValue();
    }

    public int GetScore()
    {
        return score;
    }
}
