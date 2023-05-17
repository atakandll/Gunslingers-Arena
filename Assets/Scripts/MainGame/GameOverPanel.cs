using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private Button returnToLobbyButton;
    [SerializeField] private GameObject childObject;

    private void Start()
    {
        if (GlobalManagers.instance != null)
        {
            GlobalManagers.instance.gameManager.OnGameIsOver += OnMatchIsOver;
            returnToLobbyButton.onClick.AddListener(() => GlobalManagers.instance.networkRunnerController.ShutDownRunner());
        }


    }

    private void OnMatchIsOver()
    {
        childObject.SetActive(true);
    }
    private void OnDestroy()
    {
        if (GlobalManagers.instance != null)
        {
            GlobalManagers.instance.gameManager.OnGameIsOver -= OnMatchIsOver;

        }


    }
}
