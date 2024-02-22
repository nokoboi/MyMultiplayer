using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    // Declaraci�n de variables
    [SerializeField] private Button MultiplayerGameButton;
    [SerializeField] private Button ExitGameButton;
    [SerializeField] private GameObject multiplayerMenuUI;
    



    // Start is called before the first frame update
    void Start()
    {
        MultiplayerGameButton.onClick.AddListener(() => NewMultiplayerGame());
        ExitGameButton.onClick.AddListener(Application.Quit);

    }

    #region M�todos de Bot�n
    
 
    // M�todo para Iniciar nueva partida multijugador
    public void NewMultiplayerGame()
    {
        multiplayerMenuUI.SetActive(true);
    }


    #endregion
}
