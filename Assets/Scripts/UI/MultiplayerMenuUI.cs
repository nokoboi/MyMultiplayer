using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerMenuUI : MonoBehaviour
{

    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button newLobbyButton;
    [SerializeField] private Button codeJoinButton;
    [SerializeField] private TMP_InputField codeInputField;

    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject multiplayerMenuUI;
    [SerializeField] private GameObject newLobbyUI;
    [SerializeField] private Transform lobbyListUI;
    [SerializeField] private Transform lobbyTemplate;  

    


    // Start is called before the first frame update
    void Start()
    {
        quickJoinButton.interactable = false;
        newLobbyButton.interactable = false;
        codeJoinButton.interactable = false;
        lobbyTemplate.gameObject.SetActive(false);
                
        playerNameInputField.onValueChanged.AddListener(delegate 
        {
            InputValueCheck();
            MultiplayerManager.Instance.SetPlayerName(playerNameInputField.text);
        });

        codeInputField.onValueChanged.AddListener(delegate
        {
            InputValueCheck();
        });


        quickJoinButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.QuickJoin();
        });


        newLobbyButton.onClick.AddListener(() =>
        {
            newLobbyUI.gameObject.SetActive(true);
        });

        codeJoinButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.JoinWithCode(codeInputField.text);
        });


        closeButton.onClick.AddListener(() =>
        {
            multiplayerMenuUI.SetActive(false);
        });



        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        UpdateLobbyList(new List<Lobby>());

    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.LobbyList);
    }

    public void InputValueCheck()
    {
        if(playerNameInputField.text != null && playerNameInputField.text.Length > 0)
        {
            quickJoinButton.interactable = true;
            newLobbyButton.interactable = true;
            if (codeInputField.text != null && codeInputField.text.Length > 0)
            {
                codeJoinButton.interactable = true;
            }
            else
            {
                codeJoinButton.interactable = false;
            }
        }
        else
        {
            quickJoinButton.interactable = false;
            newLobbyButton.interactable = false;
            codeJoinButton.interactable = false;
        }            

    }


    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach(Transform child in lobbyListUI)
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyListUI);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListUI>().SetLobby(lobby);
        }
    }


}
