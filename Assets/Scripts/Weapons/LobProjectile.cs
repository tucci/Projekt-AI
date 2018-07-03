using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LobProjectile : BaseProjectile {

    private int hitDamage = 10;
    public float addedDownForce = 45;
    private Rigidbody rb;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        rb.velocity += Vector3.down * addedDownForce;
    }


    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        // Though this is not the hit point, this is a good enough approximation
        // We are using on trigger enter instead of onCollisionEnter to avoid pushing the other player
        Vector3 hitPoint = transform.position;
        var p = other.GetComponent<PlayerClientController>();
        if (p != null)
        {
            owner.SetHitmarker(true);
			p.TakeDamage(hitDamage, this.owner);
        }

        Destroy(gameObject);
    }
}
