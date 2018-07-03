using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorPickup : BasePickup
{
    public int armorAmount = 25;

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
        other.GetComponent<PlayerClientController>().GiveArmor(armorAmount);
    }

    protected override void OnRespawn()
    {

    }
}
