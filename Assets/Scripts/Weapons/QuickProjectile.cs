using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class QuickProjectile : BaseProjectile
{




    public GameObject collisionParticleObject;
    private int hitDamage = 20;
    private AudioSource hitSound;
    private float destroyTime;

    void Awake()
    {
        hitSound = GetComponent<AudioSource>();
        
    }


    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        // Though this is not the hit point, this is a good enough approximation
        // We are using on trigger enter instead of onCollisionEnter to avoid pushing the other player
        var p = other.GetComponent<PlayerClientController>();
        if (p != null)
        {
            owner.SetHitmarker(true);
			p.TakeDamage(hitDamage, this.owner);
        }
        PlayHitSound();
        GameObject hitEffect = Instantiate(collisionParticleObject, transform.position - 3 * transform.up, Quaternion.identity);
        NetworkServer.Spawn(hitEffect);
        GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        destroyTime = hitEffect.GetComponent<ParticleSystem>().main.duration + hitSound.clip.length;
        Destroy(hitEffect, destroyTime);
        Destroy(gameObject, destroyTime);



    }

    void PlayHitSound()
    {
        hitSound.Play();
        RpcPlayHitSound();
    }

    [ClientRpc]
    void RpcPlayHitSound()
    {
        hitSound.Play();
    }

}
