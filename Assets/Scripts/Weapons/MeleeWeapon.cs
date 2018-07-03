using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{

    public int meleeDamage;
	private PlayerClientController owner;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    public void SetOwner(PlayerClientController player)
    {
        this.owner = player;
    }

    public void PerformMeleeAttack()
    {
        Debug.Log("perform melee");

    }


    void OnTriggerEnter(Collider other)
    {
        var hit = other.gameObject;
        var player = hit.GetComponent<PlayerClientController>();
        if (player != null)
        {
            Debug.Log("melee player hit");
			player.TakeDamage(meleeDamage, this.owner);
        }
    }
}
