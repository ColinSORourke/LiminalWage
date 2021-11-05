using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Customer;
using Utility;

public class GameManager : MonoBehaviour
{
    private int score;

    private ScoreUI scoreUI;
    private PlayerManager playerManager;
    private CustomerManager customerManager;
    private StreetManager streetManager;

    private void Awake()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        playerManager.Construct(this);

        streetManager = FindObjectOfType<StreetManager>();
        streetManager.Construct(playerManager.GetPlayerTransform());

        customerManager = FindObjectOfType<CustomerManager>();
        customerManager.Construct(playerManager.GetDeliver());

        scoreUI = FindObjectOfType<ScoreUI>();
        scoreUI.Construct(this);
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreUI.TryAddScore(amount);
    }

    public int GetScore()
    {
        return score;
    }
}
