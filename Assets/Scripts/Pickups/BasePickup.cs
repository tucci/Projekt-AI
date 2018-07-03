using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;



public abstract class BasePickup : NetworkBehaviour
{

    public GameObject pickupObjectPrefab;
    public float spawnTime = 30.0f;
    public Vector3 spawnOffset;
    public Vector3 spawnRotation;
    public float bobSpeed = 1.0f;
    public float bobHeight = 0.15f;
    public float rotationSpeed = 50.0f;
    private AudioSource pickupAudioSource;
    public AudioClip pickUpSound;
    protected BoxCollider collider;



    protected GameObject pickupObjectInstance;
    private float spawnY;
    private float currentSpawnTimer;
    private bool isHidden;
    

    
    

	// Use this for initialization
	protected virtual void Start ()
	{
	    collider = GetComponent<BoxCollider>();
	    pickupObjectInstance = Instantiate(pickupObjectPrefab, transform.position + spawnOffset,  Quaternion.Euler(spawnRotation));
        pickupObjectInstance.transform.parent = this.gameObject.transform;
	    spawnY = transform.position.y;
     //   audioSource = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
	    //audioSource.pitch = 1.5f;
	    //audioSource.playOnAwake = false;
	    //audioSource.clip = pickupSound;
        currentSpawnTimer = spawnTime;
	    isHidden = false;

        pickupAudioSource = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
	    pickupAudioSource.spatialBlend = 1.0f;
	    pickupAudioSource.clip = pickUpSound;


	}


    // Update is called once per frame
    protected virtual void Update () {

        // We still want to update even when hidden, because when it respawns, it wont jump it's transform
        // Bob up and down
        pickupObjectInstance.transform.position = new Vector3(
            pickupObjectInstance.transform.position.x,
            spawnY + (bobHeight) + (bobHeight * Mathf.Sin(bobSpeed * Time.time)),
            pickupObjectInstance.transform.position.z);

        // Rotate 
        pickupObjectInstance.transform.rotation = Quaternion.Euler(
            spawnRotation.x + pickupObjectInstance.transform.rotation.x,
            spawnRotation.y + rotationSpeed * Time.time % 360,
            spawnRotation.z + pickupObjectInstance.transform.rotation.z);

        if (isHidden)
        {
            currentSpawnTimer -= Time.deltaTime;
            if (currentSpawnTimer < 0)
            {
                isHidden = false;
                pickupObjectInstance.gameObject.SetActive(true);
                currentSpawnTimer = spawnTime;
                collider.enabled = true;
                OnRespawn();
            }
        }

        
    }

    protected virtual void OnPickup(Collider other){}
    protected virtual void OnRespawn(){}

    protected virtual void PlayPickupSound()
    {
        pickupAudioSource.Play();
        RpcPlayPickupSound();
        
    }

    [ClientRpc]
    void RpcPlayPickupSound()
    {
        pickupAudioSource.Play();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            currentSpawnTimer = spawnTime;
            isHidden = true;
            pickupObjectInstance.gameObject.SetActive(false);
            OnPickup(other);
            collider.enabled = false;
            PlayPickupSound();
        }

    }



}
