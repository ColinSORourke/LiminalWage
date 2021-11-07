using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Customer;
using Utility;
using Collectables;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private int score;

    private MoraleUI moraleUI;
    private ScoreUI scoreUI;
    private PlayerManager playerManager;
    private CustomerManager customerManager;
    private StreetManager streetManager;
    private CoinManager coinManager;

    [SerializeField] private GameObject gameOverScreen;

    private Scene thisScene;

    private bool _isGameOver = false;

    public bool isGameOver { private set { } get { return _isGameOver; } }

    private void Awake()
    {
        thisScene = SceneManager.GetActiveScene();

        moraleUI = FindObjectOfType<MoraleUI>();
        moraleUI.Construct();

        playerManager = FindObjectOfType<PlayerManager>();
        playerManager.Construct(this, moraleUI);

        scoreUI = FindObjectOfType<ScoreUI>();
        scoreUI.Construct(this);

        streetManager = FindObjectOfType<StreetManager>();
        streetManager.Construct(playerManager.GetPlayerTransform());

        customerManager = FindObjectOfType<CustomerManager>();
        customerManager.Construct(playerManager.GetPlayerInteract());

        coinManager = FindObjectOfType<CoinManager>();
        coinManager.Construct(playerManager.GetPlayerInteract());
    }

    public void StartGameOver()
    {
        _isGameOver = true;
        gameOverScreen.SetActive(true);
        StartCoroutine(RestartTimer());
    }

    private IEnumerator RestartTimer()
    {
        yield return new WaitForSeconds(1);
        print("thisScene.name: " + thisScene.name);
        SceneManager.LoadScene(thisScene.name);
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
