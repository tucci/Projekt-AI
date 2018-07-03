using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RocketProjectile : BaseProjectile
{

    public float splashRadius = 15;
    public float splashDamage = 40.0f;

    private bool alreadyTriggered;



    public GameObject explosionPrefab;
    private ParticleSystem trailParticle;
    private AudioSource boomAudio;

    

    void Awake()
    {
        trailParticle = GetComponentInChildren<ParticleSystem>();
        boomAudio = GetComponent<AudioSource>();
        alreadyTriggered = false;

    }

    // Use this for initialization
	void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	void Update () {
        base.Update();
    }

    public void Explode()
    {
        
        GameObject explosion = Instantiate(explosionPrefab, transform.position + 2 * transform.forward, Quaternion.identity);

        PlayExplosionSound();
        NetworkServer.Spawn(explosion);
        float destoyExplosionTime = explosion.GetComponent<ParticleSystem>().main.duration;
        Destroy(explosion, destoyExplosionTime);
        Destroy(gameObject, destoyExplosionTime + trailParticle.main.duration + boomAudio.clip.length);
        
        
    }

    void PlayExplosionSound()
    {
        boomAudio.Play();
        RpcPlayExplosionSound();
    }

    [ClientRpc]
    void RpcPlayExplosionSound()
    {
        boomAudio.Play();
    }




    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (alreadyTriggered) return;
        alreadyTriggered = true;

        GetComponent<MeshRenderer>().enabled = false;
        Explode();

        // Though this is not the hit point, this is a good enough approximation
        // We are using on trigger enter instead of onCollisionEnter to avoid pushing the other player
        Vector3 hitPoint = transform.position;

        




        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.position = hitPoint;
        //sphere.transform.localScale = new Vector3(splashRadius, splashRadius, splashRadius);


        // Apply splash damange
        Collider[] objects = Physics.OverlapSphere(hitPoint, splashRadius);

        foreach (Collider collider in objects)
        {
            var p = collider.GetComponent<PlayerClientController>();
            if (p != null)
            {
                float distance = (hitPoint - p.transform.position).magnitude;

                float effect = (1 - (distance / splashRadius));
                int damage = (int)(splashDamage * effect);
                if (damage <= 0) return;
                Debug.Log("distance " + distance + ", effect " + effect + " damage " + damage);
                owner.SetHitmarker(true);
				p.TakeDamage((int)(splashDamage * effect), this.owner);

                if (p == owner)
                {
                    owner.RpcRocketJump(-transform.forward, damage);
                }
            }
        }

        
        
    }


}
