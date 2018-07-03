using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : BasePickup
{

    public int healthAmount;
    
	// Use this for initialization
	void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
	}

    protected override void OnPickup(Collider other)
    {
        base.OnPickup(other);
        other.GetComponent<PlayerClientController>().GiveHealth(healthAmount);
    }

    protected override void OnRespawn()
    {
        base.OnRespawn();
    }
}
