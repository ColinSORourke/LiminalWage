using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;

    private void Awake()
    {
        if(!playerManager)
        {
            Debug.LogError(this + "playerManager is null");
        }
        playerManager.Construct();
    }
}
