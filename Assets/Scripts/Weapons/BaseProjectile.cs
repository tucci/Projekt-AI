using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BaseProjectile : NetworkBehaviour
{

    protected PlayerClientController owner;


    protected virtual void Awake()
    {

    }

    // Use this for initialization
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }



    public void SetPlayerOwner(PlayerClientController player)
    {
        owner = player;
    }


}
