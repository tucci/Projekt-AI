using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : BasePickup {

    // Use this for initialization
    void Start()
    {
        base.Start();
        
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    protected override void OnPickup(Collider other)
    {
        base.OnPickup(other);
        other.GetComponent<PlayerClientController>().GiveWeapon(pickupObjectInstance.GetComponent<BaseWeapon>().GetWeaponEnum());
    }

    protected override void OnRespawn()
    {
        base.OnRespawn();
    }
}
