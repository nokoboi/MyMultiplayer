using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletController : NetworkBehaviour
{

    
    public static BulletController LocalInstance { get; private set; }

    // Declaración de variables
    private Rigidbody2D rb;
    [SerializeField] private float speed;

   

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
            rb = LocalInstance.GetComponent<Rigidbody2D>(); 
        }

       
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
        
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            rb.MovePosition(transform.position + transform.right * speed * Time.fixedDeltaTime);
            Debug.Log(rb.position);
        }
       
    }

}
