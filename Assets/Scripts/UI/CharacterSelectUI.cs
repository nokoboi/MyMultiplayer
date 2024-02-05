using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button skin1Button;
    [SerializeField] private Button skin2Button;
    [SerializeField] private TMP_Text lobbyName;
    [SerializeField] private TMP_Text lobbyCode;

    // Botones de los colores
    [SerializeField] private Button noColorButton;
    [SerializeField] private Button whiteButton;
    [SerializeField] private Button blueButton;
    [SerializeField] private Button redButton; 
    [SerializeField] private Button greenButton;
    [SerializeField] private Button purpleButton;

    private void Awake()
    {
        GameManager.Instance.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;


       lobbyName.text = LobbyManager.Instance.GetLobby().Name;
       lobbyCode.text = LobbyManager.Instance.GetLobby().LobbyCode;

        mainMenuButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MenuScene");
        });

        readyButton.onClick.AddListener(() =>
        {
            GameManager.Instance.SetPlayerReady();
            MultiplayerManager.Instance.IsPlayerReady(true);
            readyButton.interactable = false;
            var colors = readyButton.colors;
            colors.disabledColor = new Color(0.5f, 1, 0, 1);
            Debug.Log("Player Ready: " + GameManager.Instance.IsLocalPlayerReady());
            Debug.Log("Game Ready: " + GameManager.Instance.gameReady.Value);
            
        });

        // Cambio de Skin
        skin1Button.onClick.AddListener(() =>
        {
            MultiplayerManager.Instance.SetPlayerSkin(0);
        });

        skin2Button.onClick.AddListener(() =>
        {
            MultiplayerManager.Instance.SetPlayerSkin(1);
        });



        // Cambio de Color
        noColorButton.onClick.AddListener(() =>
        {
            MultiplayerManager.Instance.SetPlayerColor(new Color(1,1,1,0.5f));
        });

        whiteButton.onClick.AddListener(() =>
        {
            MultiplayerManager.Instance.SetPlayerColor(new Color(1, 1, 1, 1));
        });

        blueButton.onClick.AddListener(() =>
        {
            MultiplayerManager.Instance.SetPlayerColor(new Color(0, 0, 1, 1));
        });

        redButton.onClick.AddListener(() =>
        {
            MultiplayerManager.Instance.SetPlayerColor(new Color(1, 0, 0, 1));
        });

        greenButton.onClick.AddListener(() =>
        {
            MultiplayerManager.Instance.SetPlayerColor(new Color(0, 1, 0, 1));
        });

        purpleButton.onClick.AddListener(() =>
        {
            MultiplayerManager.Instance.SetPlayerColor(new Color(1, 0, 1, 1));
        });


    }

    private void GameManager_OnLocalPlayerReadyChanged(object sender, EventArgs e)
    {
        if (NetworkManager.Singleton.IsHost && GameManager.Instance.gameReady.Value)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);

        }
    }
}
