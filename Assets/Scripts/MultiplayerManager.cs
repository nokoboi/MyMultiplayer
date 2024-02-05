using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class MultiplayerManager : NetworkBehaviour
{
    public const int MAX_PLAYER_AMOUNT = 4;
    private const string PLAYER_NAME = "NombreJugador";
    public static MultiplayerManager Instance { get; private set; }


    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;
    public event EventHandler ConnectionApprovalCallBack;


    private NetworkList<PlayerData> playerDataNetworkList;
    private string playerName;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
                
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;

    }


    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
        PlayerPrefs.SetString(PLAYER_NAME, playerName);
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    
    public void StartHost()
    {

        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
        
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }

    

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                // Disconnected!
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            skinIndex = 0,
            color = Color.white
        }); 
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }


    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "La Partida está completa";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();        
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string name, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerName = name;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerId = playerId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }


    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {        
        return (playerIndex < playerDataNetworkList.Count);
    }

    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
                return i;
        }
        return -1;
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
                return playerData;
        }
        return default;
    }

    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public int GetPlayerSkinIndexFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex].skinIndex;
    }


    public void SetPlayerSkin(int skinIndex)
    {
        ChangePlayerSkinServerRpc(skinIndex);
    }



    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerSkinServerRpc(int skinIndex, ServerRpcParams serverRpcParams = default)
    {

        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.skinIndex = skinIndex;

        playerDataNetworkList[playerDataIndex] = playerData;
    }


    public void SetPlayerColor(Color color)
    {
        SetPlayerColorServerRpc(color);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerColorServerRpc(Color color, ServerRpcParams serverRpcParams = default) 
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.color = color;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    public void IsPlayerReady(bool isReady)
    {
        IsPlayerReadyServerRpc(isReady);
    }


    [ServerRpc(RequireOwnership = false)]
    private void IsPlayerReadyServerRpc(bool isReady, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.isPlayerReady = isReady;

        playerDataNetworkList[playerDataIndex] = playerData;
    }


    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);  
    }



}
