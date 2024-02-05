using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerController : NetworkBehaviour
{

    // Eventos para sincronización Jugadores
    public static event EventHandler OnAnyPlayerSpawned;

    // Variable para identificar el jugador propio.
    public static PlayerController LocalInstance { get; private set; }
    
    // Variables del controlador del jugador
    [SerializeField] private float velocidad;
    [SerializeField] private float fuerzaSalto;
    [SerializeField] private GameObject bulletSpawner;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject weapon;
    [SerializeField] private List<GameObject> skinList;   


    private Rigidbody2D rb;
    private Animator anim;
    private PlayerData localPlayerData;
    private GameObject localSkin;
    private Camera cam;

    

    private void Awake()
    {
        // Para evitar que el jugador se destruya en los cambios de escena.
        DontDestroyOnLoad(transform.gameObject);
        
    }

    private void Start()
    {
        // Cargamos los datos multijugador
        PlayerSetUp(OwnerClientId);
        rb = GetComponent<Rigidbody2D>();
    }

    // Este método se ejecuta cuando se instancia en red en todos los clientes.
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
            // Asignamos la cámara al jugador local.
            cam = FindObjectOfType<Camera>();
        }
        // Cargamos las configuraciones de los jugadores
        localPlayerData = MultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
        
        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if(IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
        
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        throw new NotImplementedException();
    }


    // Método para establecer la skin del jugador.
    public void SetPlayerSkin(int skin)
    {
        for(int i = 0; i < skinList.Count; i++)
        {
            if(i != skin)
            {
                skinList[i].SetActive(false);
            }
        }
        
        localSkin = skinList[skin];
        localSkin.SetActive(true);
        anim = localSkin.GetComponent<Animator>();
        weapon.transform.SetParent(localSkin.transform);
    }

    // Método para establecer el clor del skin actual.
    public void SetPlayerColor(Color color)
    {
        GetComponentInChildren<SpriteRenderer>().color = color;
    }

    // Método que carga en el jugador la configuración del cliente.
    public void PlayerSetUp(ulong clientId)
    {        
        localPlayerData = MultiplayerManager.Instance.GetPlayerDataFromClientId(clientId);
        SetPlayerSkin(localPlayerData.skinIndex);
        SetPlayerColor(localPlayerData.color);
    }

    
    // Update is called once per frame
    void Update()
    {           
        // Para ejecutar los movimientos sólo en el propietario.   
        if (!IsOwner)
            return;
            
        Move();
        CheckFire();
            
     }

    // Método para gestionar el movimiento.
    public void Move()
    {
        
        if(this.transform.rotation == Quaternion.identity) // Desplazamiento hacia la derecha
        {
            rb.velocity = (transform.right * velocidad * Input.GetAxis("Horizontal")) +
                    (transform.up * rb.velocity.y);
        }
        else { // Desplazamiento hacia la izuquierda
            rb.velocity = -(transform.right * velocidad * Input.GetAxis("Horizontal")) +
                (transform.up * rb.velocity.y);
        }

        // Actualizamos la posición
        this.transform.SetLocalPositionAndRotation(this.transform.position,this.transform.rotation);
        

        // Rotamos el jugador en función de la pulsación de teclas.
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.transform.rotation = Quaternion.identity;
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
            
        // Configuración del salto.
        if (Input.GetKeyDown(KeyCode.Space) && (Mathf.Abs(rb.velocity.y) < 0.2f))
        {           
            rb.AddForce(transform.up * fuerzaSalto);
        }

        // Actualizamos la animación
        anim.SetFloat("velocidadX", Mathf.Abs(rb.velocity.x));
        anim.SetFloat("velocidadY", rb.velocity.y);

        // Seguimiento de cámara
        cam.transform.SetPositionAndRotation(new Vector3(this.transform.position.x + 2, this.transform.position.y + 2, -10), Quaternion.identity); 

    }


    // Método para disparar
    public void CheckFire()
    {
        if (Input.GetKeyDown(KeyCode.R))
            FireServerRpc(localSkin.transform.rotation, bulletSpawner.transform.position);
    }

    // Método que instancia en el servidor la bala. 
    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc(Quaternion rotation, Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);
       
        NetworkObject bulletNetwork = bullet.GetComponent<NetworkObject>();
        bulletNetwork.SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);             

        Destroy(bullet, 2f);
    }
        

        

    /* Válido para modo Sigle
     * 
        public void SelectSkin(int skinIndex)
        {
            if (!IsOwner)
            {
                return;
            }
            // Asignamos los elementos a la nueva skin
            this.skinIndex = skinIndex;
            GameObject newSkin = skinList[skinIndex];            
            newSkin.transform.rotation = localSkin.transform.rotation;
            weapon.transform.SetParent(newSkin.transform);
            
        
            //Desactivamos la anterior
            localSkin.SetActive(false);
            //Activamos el nuevo
            newSkin.SetActive(true);
            
            // Reasignamos nombres.
            localSkin = newSkin;
        
            // Actualizamos la animación
            anim = GetComponentInChildren<Animator>();
        }
    */
    


}
