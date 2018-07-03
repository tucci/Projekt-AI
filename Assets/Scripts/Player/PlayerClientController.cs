using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using Random = UnityEngine.Random;


public class PlayerClientController : NetworkBehaviour
{



    public static int PlayerCount = 0;

    public GameObject meleeObject;
    public GameObject playerUiPrefab;
    public GameObject playerAudioListenerPrefab;
    public GameObject playerSoundsInstance;
    public List<AudioSource> weaponAudioSources;
    public AudioSource playerHitAudioSource;
    public List<AudioClip> HitAudioClips;



    private GameObject meleeInstance;
    private GameObject playerUiInstance;
    private GameObject playerAudioListenerInstance;
    

    
    private PlayerUIScript playerUI;
    private MovementScript movement;
    private MouseLook2 mouseLook;


    public string playerName = "Player";

    [SyncVar]
    public int deaths;

    [SyncVar]
    public int kills;

    public float meleeOutTime = 0.5f;
    public float weaponSwithWaitTime = 0.15f;

    public const int maxHealth = 100;
    public const int maxArmor = 100;


    private Camera spawnCamera;


    // The spawns for our players
    private NetworkStartPosition[] spawnPoints;


    // All the possible guns the player can have
    // They have the guns stored on them, but they may not be accesbile until they have it
    // see hasGun list
    [SerializeField]
    private Transform[] guns;
    private List<GameObject> gunInstances;
    // Whether or not the player has the gun at the corresponding index
    // there is a direct 1-1 correspdonce to gunInstances and hasGun
    private List<bool> hasGun;


    

    



    [SyncVar(hook = "OnGunChange")] public int currentGunIndex;
    [SyncVar] private float meleeTimer;
    [SyncVar] private float weaponSwitchTimer;
    [SyncVar(hook = "OnMeleeChange")] private bool isMeleeing;
    [SyncVar] private bool canSwitchWeapon;

    [SyncVar(hook = "OnChangeHealth")] public float playerHealth;

    [SyncVar(hook = "OnChangeArmor")] public float playerArmor;
    [SyncVar]private bool mouseAlreadyPressed;

    //[SyncVar(hook = "OnChangeAmmo")] public int playerAmmo

    private SyncListInt ammoCache = new SyncListInt();


    [SyncVar(hook = "OnHitMarkerChanged")] private bool hitmakerEnabled;
    private float hitmarkerRemoveAfterTime = 0.5f;
    private float hitmarkerTimer;

    private bool playerIsDead;

    private Transform respawnWaitArea;

    


    [Command]
    void CmdSetupAmmo()
    {
        
        ammoCache.Add(0);
        ammoCache.Add(0);
        ammoCache.Add(0);
        ammoCache.Add(0);
        ammoCache.Add(0);
        RpcSetupAmmo();
        RpcRespawn();


    }

    [ClientRpc]
    void RpcSetupAmmo()
    {
        ammoCache.Callback = OnChangeAmmo;
    }



    [Command]
    public void CmdUiInitilized()
    {
        if (isServer)
        {
            playerHealth = maxHealth;

            // I know this looks weird, but trust me this fixes a lot of problems
            // If we just set playerArmor to 0 first, the network won't realize there is change since it is initilized with 0
            // If we were to do that, then when we spawn the armor text will not show up
            // In order for the OnArmorChange to be called, we need to set it to something besides 0 first, then set it to 0
            playerArmor = 1;
            playerArmor = 0;
            ammoCache[currentGunIndex] = gunInstances[currentGunIndex].GetComponent<BaseWeapon>().GetBulletsRemaining();
        }
        

    }




    // Use this for initialization
    void Start()
    {
        playerName = "Player" + ++PlayerCount;

        ///////////////////////////////////////////////////
        // DO NOT TOUCH THIS PIECE OF CODE. IT SETS UP THE UI
        // IF YOU TRY TO DO IT IN ANY OTHER WAY, IT WILL JUST BREAK THE UI
        // IN UNEXPECTED WAYS
        if (isLocalPlayer)
        {
            spawnPoints = FindObjectsOfType<NetworkStartPosition>();
            playerUiInstance = Instantiate(playerUiPrefab);
            playerUiInstance.transform.parent = this.gameObject.transform;
            playerAudioListenerInstance = Instantiate(playerAudioListenerPrefab, Vector3.zero, Quaternion.identity, this.gameObject.transform);
        }
        if (isClient)
        {
            spawnCamera = GameObject.FindGameObjectWithTag("SpawnScreenCamera").GetComponent<Camera>();
            respawnWaitArea = GameObject.FindGameObjectWithTag("RespawnWaitArea").transform;
            playerUI = GetComponent<PlayerUIScript>();
            playerUI.SetControllerReference(this);
            movement = GetComponent<MovementScript>();
            mouseLook = GetComponent<MouseLook2>();
            mouseLook.SetRespawnCamera(spawnCamera);
            hitmarkerTimer = hitmarkerRemoveAfterTime;
            
            CmdSetupAmmo();
        }


        ///////////////////////////////////////////////////


        // This needs to be called on both the server and client
        gunInstances = new List<GameObject>();
        hasGun = new List<bool>();


        Transform weaponTransform = transform.Find("CameraController").Find("WeaponHolder");
        int i = 0;


        foreach (Transform child in guns)
        {
            gunInstances.Add(child.gameObject);
            gunInstances[i].gameObject.SetActive(false);
            hasGun.Add(false);
            i++;
        }

        meleeInstance = Instantiate(meleeObject, weaponTransform.position, meleeObject.transform.rotation) as GameObject;
        meleeInstance.GetComponent<MeleeWeapon>().SetOwner(this);
        meleeInstance.gameObject.SetActive(false);
        meleeInstance.transform.parent = weaponTransform;


        gunInstances[0].gameObject.SetActive(true);
        gunInstances[0].GetComponent<BaseWeapon>().RefillAmmo();
        hasGun[0] = true;

        isMeleeing = false;
        meleeTimer = meleeOutTime;

        canSwitchWeapon = true;
        weaponSwitchTimer = weaponSwithWaitTime;


        
    }

    // Update is called once per frame
    void Update()
    {

        if (!isLocalPlayer)
        {
            return;
        }

        if (hitmakerEnabled)
        {
            hitmarkerTimer -= Time.deltaTime;
            if (hitmarkerTimer < 0)
            {
                hitmarkerTimer = hitmarkerRemoveAfterTime;
                hitmakerEnabled = false;
                playerUI.SetHitMarkerEnabled(false);
                CmdSetHitMarker(false);
            }
        }        


        if (isMeleeing)
        {
            meleeTimer -= Time.deltaTime;
            if (meleeTimer < 0)
            {
                CmdOnMeleeEnd();
            }
        }
        else
        {
            // Only allow gun firing/switching to happen when we are not meleeing
            if (Input.GetMouseButtonDown(0)){CmdFire();} // Left mouse 
            if (Input.GetMouseButtonDown(1) && !isMeleeing){CmdMelee();} // Middle mouse click
            if (playerIsDead && Input.GetButtonDown("Jump"))
            {
                CmdRespawnPlayer();
            }

            if (!canSwitchWeapon)
{
                weaponSwitchTimer -= Time.deltaTime;
                if (weaponSwitchTimer < 0)
                {
                    canSwitchWeapon = true;
                    weaponSwitchTimer = weaponSwithWaitTime;
                }
            }
            else
            {
                float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
                if (scrollDelta > 0f){PreviousWeapon(); }// scroll up
                else if (scrollDelta < 0f){ NextWeapon();}
                // Number keys for switching weapons
                if (Input.GetKeyDown(KeyCode.Alpha1)){CmdSetWeapon(0);}
                if (Input.GetKeyDown(KeyCode.Alpha2)){CmdSetWeapon(1);}
                if (Input.GetKeyDown(KeyCode.Alpha3)){CmdSetWeapon(2);}
                if (Input.GetKeyDown(KeyCode.Alpha4)){CmdSetWeapon(3);}
                if (Input.GetKeyDown(KeyCode.Alpha5)){CmdSetWeapon(4);}

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    TogglePlayerPauseMenu();
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    ToggleScoreboard();
                }
            }
        }
    }

    


    void LateUpdate()
    {
        if (!isLocalPlayer) return;
        if (Input.GetMouseButton(0))
        {
            CmdFireHold();
            mouseAlreadyPressed = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            mouseAlreadyPressed = false;
            CmdFireHoldRelease();

        }

    }


    // Commands
    // Comands are called by the client
    // But are ran on the server

    [Command]
    public void CmdFire()
    {
        BaseWeapon weapon = gunInstances[currentGunIndex].GetComponent<BaseWeapon>();
        weapon.Fire();
        ammoCache[currentGunIndex] = weapon.GetBulletsRemaining();
        WeaponFireData data = weapon.GetLastWeaponFireData();
        
        if (data.shouldShowNozzleEffect)
        {
            weaponAudioSources[currentGunIndex].PlayOneShot(weapon.weaponFireSound);
        }
        if (weapon.GetWeaponEnum() == WeaponEnum.BeamWeapon && weapon.GetBulletsRemaining() > 0)
        {
            weaponAudioSources[currentGunIndex].loop = true;
            weaponAudioSources[currentGunIndex].clip = weapon.weaponFireSound;
            weaponAudioSources[currentGunIndex].Play();
        }
        RpcClientPostFire(data);

    }

    [ClientRpc]
    public void RpcClientPostFire(WeaponFireData data)
    {
        BaseWeapon weapon = gunInstances[currentGunIndex].GetComponent<BaseWeapon>();
        weapon.ShowNozzleEffect(data);
        if (data.shouldShowNozzleEffect)
        {
            weaponAudioSources[currentGunIndex].PlayOneShot(weapon.weaponFireSound);
        }
        if (weapon.GetWeaponEnum() == WeaponEnum.BeamWeapon && weapon.GetBulletsRemaining() > 0)
        {
            weaponAudioSources[currentGunIndex].loop = true;
            weaponAudioSources[currentGunIndex].clip = weapon.weaponFireSound;
            weaponAudioSources[currentGunIndex].Play();
        }

    }

    [Command]
    public void CmdFireHold()
    {
        BaseWeapon weapon = gunInstances[currentGunIndex].GetComponent<BaseWeapon>();
        weapon.FireHold();
        ammoCache[currentGunIndex] = weapon.GetBulletsRemaining();
        if (weapon.GetWeaponEnum() == WeaponEnum.BeamWeapon || weapon.GetWeaponEnum() == WeaponEnum.QuickProjectileWeapon)
        {
            RpcFireHold();
        }
        if (weapon.GetWeaponEnum() == WeaponEnum.QuickProjectileWeapon)
        {
            if (weapon.GetLastWeaponFireData().shouldShowNozzleEffect)
            {
                weaponAudioSources[currentGunIndex].Play();
                RpcPlayWeaponSound();
                RpcClientPostFire(weapon.GetLastWeaponFireData());
            }

        }

    }

    [ClientRpc]
    public void RpcFireHold()
    {
        BaseWeapon weapon = gunInstances[currentGunIndex].GetComponent<BaseWeapon>();
        
        
        if (weapon.GetWeaponEnum() == WeaponEnum.BeamWeapon)
        {
            weapon.FireHold();
            ammoCache[currentGunIndex] = weapon.GetBulletsRemaining();

            if (weapon.GetBulletsRemaining() <= 0)
            {
                weaponAudioSources[currentGunIndex].loop = false;
                weaponAudioSources[currentGunIndex].Stop();
                weaponAudioSources[currentGunIndex].clip = null;
            }
        }

        weapon.ShowNozzleEffect(weapon.GetLastWeaponFireData());
    }

    [ClientRpc]
    void RpcPlayWeaponSound()
    {
        weaponAudioSources[currentGunIndex].Play();
    }



    [Command]
    public void CmdFireHoldRelease()
    {    
        BaseWeapon weapon = gunInstances[currentGunIndex].GetComponent<BaseWeapon>();
        RpcFireHoldRelease();
    }

    [ClientRpc]
    public void RpcFireHoldRelease()
    {
        BaseWeapon weapon = gunInstances[currentGunIndex].GetComponent<BaseWeapon>();
        weapon.FireHoldRelease();
        if (weapon.GetWeaponEnum() == WeaponEnum.BeamWeapon)
        {
            weaponAudioSources[currentGunIndex].loop = false;
            weaponAudioSources[currentGunIndex].Stop();
            weaponAudioSources[currentGunIndex].clip = null;
        }
    }


    [Command]
    private void CmdMelee()
    {
        isMeleeing = true;
        gunInstances[currentGunIndex].gameObject.SetActive(false);
        meleeInstance.gameObject.SetActive(true);
        meleeInstance.GetComponent<MeleeWeapon>().PerformMeleeAttack();
    }

    [Command]
    private void CmdOnMeleeEnd()
    {
        isMeleeing = false;
        // Reset melee timer
        meleeTimer = meleeOutTime;
        meleeInstance.gameObject.SetActive(false);
        gunInstances[currentGunIndex].gameObject.SetActive(true);
    }

    [Command]
    private void CmdSetWeapon(int index)
    {
        if (!hasGun[index])
        {
            // If the player doesnt have the gun, don't switch to it
            return;
        }

        for (int i = 0; i < gunInstances.Count; i++)
        {
            gunInstances[i].gameObject.SetActive(false);
        }
        canSwitchWeapon = false;
        currentGunIndex = index;
        gunInstances[index].gameObject.SetActive(true);
        ammoCache[currentGunIndex] = gunInstances[currentGunIndex].GetComponent<BaseWeapon>().GetBulletsRemaining();

    }

    [Command]
    private void CmdSetHitMarker(bool enabled)
    {
        hitmakerEnabled = enabled;
    }

    public void SetHitmarker(bool enabled)
    {
        hitmakerEnabled = enabled;
    }

    [Command]
    private void CmdRespawnPlayer()
    {
        RpcRespawn();
    }


    // ClientRpc functions
    // These are called by the server
    // But are ran on the client
    [ClientRpc]
	void RpcOnPlayerDeath()
    {
        
        movement.getRigidBody().velocity = Vector3.zero;
        playerIsDead = true;
        mouseLook.ToggleRespawnCamera(true);
        movement.enabled = false;
        transform.position = respawnWaitArea.position;
        playerUI.ShowRespawnUi(true);
        CmdSetWeapon(0);
    }

    [ClientRpc]
    void RpcRespawn()
    {

        if (isLocalPlayer)
        {
            
            playerIsDead = false;
            movement.enabled = true;
            
            mouseLook.ToggleRespawnCamera(false);
            playerUI.ShowRespawnUi(false);
            // Set the spawn point to origin as a default value
            Vector3 spawnPoint = Vector3.zero;
            Vector3 spawnRotation = Vector3.zero;
                
            // If there is a spawn point array and the array is not empty, pick one at random
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                int randomIndex = Random.Range(0, spawnPoints.Length - 1);
                spawnPoint = spawnPoints[randomIndex].transform.position;
                spawnRotation = spawnPoints[randomIndex].transform.rotation.eulerAngles;
                Debug.Log("Spawn rotation " + spawnRotation);
            }

            // Set the player’s position to the chosen spawn point
            transform.position = spawnPoint;
            mouseLook.SetRotation(spawnRotation);
            movement.getRigidBody().velocity = new Vector3(0, 0, 0);
            CmdSetWeapon(0);
            OnChangeAmmo(SyncListInt.Operation.OP_DIRTY, currentGunIndex);

        }
    }


    [ClientRpc]
    void RpcShowPlayerKillfeed(string enemnyName)
    {
        playerUI.ShowPlayerKill(enemnyName);
    }


    [ClientRpc]
    void RpcSetGameEnd(string playerWhoWon)
    {
        EndGameData.playerWhoWon = playerWhoWon;
        SceneManager.LoadScene("endgame");
        
    }

    void OnPlayerDeath(PlayerClientController sourcePlayer)
    {

        if (sourcePlayer != this)
        {
            sourcePlayer.kills++;
            sourcePlayer.RpcShowPlayerKillfeed(playerName);
        }
        if (sourcePlayer.kills >= Scoreboard.killsToWin)
        {
            EndGameData.playerWhoWon = sourcePlayer.playerName;
            RpcSetGameEnd(sourcePlayer.playerName);
        }

        deaths++;
        
        // Update gun instances on both the client and server
        for (int i = 0; i < hasGun.Count; i++)
        {
                gunInstances[i].GetComponent<BaseWeapon>().resetWeaponForRespawn();
                gunInstances[i].gameObject.SetActive(false);        
                hasGun[i] = false;
                ammoCache[i] = 0;
        }

        gunInstances[0].gameObject.SetActive(true);
        hasGun[0] = true;


        if (isServer)
        {
            playerHealth = maxHealth;
            playerArmor = 0;
            ammoCache[0] = gunInstances[0].GetComponent<BaseWeapon>().RefillAmmo();
            // TODO: instead of respawning right away, show a death screen/respawn screen
            // called on the Server, but invoked on the Clients
			RpcOnPlayerDeath();
        }

        
        

    }

    // Hook functions, These are called by the server
    // but ran on the client
    void OnChangeHealth(float currentHealth)
    {
        playerHealth = currentHealth;
        playerUI.SetHealth((int)currentHealth);
    }

    void OnChangeArmor(float currentArmor)
    {
        playerArmor = currentArmor;
        playerUI.SetArmor((int)currentArmor);
    }

    void OnChangeAmmo(SyncListInt.Operation op, int index)
    {
        playerUI.SetAmmo(ammoCache);
    }

    void OnGunChange(int index)
    {
        
        int oldIndex = currentGunIndex;
        // Special case for when we switch out of the beam gun
        if (gunInstances[oldIndex].GetComponent<BaseWeapon>().GetWeaponEnum() == WeaponEnum.BeamWeapon)
        {
            weaponAudioSources[oldIndex].loop = false;
            weaponAudioSources[oldIndex].Stop();
            weaponAudioSources[oldIndex].clip = null;
            gunInstances[oldIndex].GetComponent<BaseWeapon>().FireHoldRelease();

        }
        currentGunIndex = index;
        for (int i = 0; i < gunInstances.Count; i++)
        {
            gunInstances[i].gameObject.SetActive(false);
        }
        canSwitchWeapon = false;
        gunInstances[index].gameObject.SetActive(true);
        playerUI.SetCurrentGunIndex(index);
        weaponAudioSources[index].clip = gunInstances[index].GetComponent<BaseWeapon>().weaponFireSound;

        //set animation
        switch (index) {
            case 0: //hitscan
                GetComponent<NetworkAnimator>().SetTrigger("isHoldingPistol");
                break;
            default:
                GetComponent<NetworkAnimator>().SetTrigger("isHoldingRifle");
                break;
        }
    }

    void OnMeleeChange(bool melee)
    {
        isMeleeing = melee;
        if (melee)
        {
            gunInstances[currentGunIndex].gameObject.SetActive(false);
            meleeInstance.gameObject.SetActive(true);
        }
        else
        {
            meleeTimer = meleeOutTime;
            meleeInstance.gameObject.SetActive(false);
            gunInstances[currentGunIndex].gameObject.SetActive(true);
        }
    }

    void OnHitMarkerChanged(bool enabled)
    {
        hitmakerEnabled = enabled;
        playerUI.SetHitMarkerEnabled(enabled);
    }



    [ClientRpc]
    void RpcPlayHitSound()
    {
        playerHitAudioSource.clip = HitAudioClips[Random.Range(0, HitAudioClips.Count - 1)];
        playerHitAudioSource.Play();
    }

    // Other functions that can be called anywhere
    // You must manually specify what code runs on the client/server
	public void TakeDamage(float amount, PlayerClientController sourcePlayer)
    {
        if (!isServer)
            return;

        RpcPlayHitSound();
        if (playerArmor >= amount)
        {
            playerArmor -= amount;
        }
        else
        {
            if (playerArmor > 0)
            {
                // Get the remainder damage from the armor, and take the damage to our health
                float remainder = amount - playerArmor;
                playerArmor = 0;
				TakeDamage(remainder, sourcePlayer);
            }
            else
            {
                playerHealth -= amount;
                if (playerHealth <= 0)
                {
                    
					OnPlayerDeath(sourcePlayer);
                }
            } 

        }


    }


    public void GiveWeapon(WeaponEnum weaponEnum)
    {

        PlayPickUpSound();
        int weaponIndex = (int)weaponEnum;
        if (hasGun[weaponIndex])
        {
            
            int ammoFilled = gunInstances[weaponIndex].GetComponent<BaseWeapon>().RefillAmmo();
            // If the gun we just picked up is the same as we have in our hand
            // then we need to update the ammo ui
            if (currentGunIndex == weaponIndex)
            {
                ammoCache[weaponIndex] = ammoFilled; 
            }
            else
            {
                // if it is not the same, then we just want to update the ammo for that gun, but dont update the ui
                // because we are refilling ammo for a gun we are not currently holding
                // TODO: do we want to do weapon priority. Ex when u pick up a stronger weapon, it auto switches?
            }

        }
        else
        {
            // Pick up that weapon for the first and set the gun's ammo
            ammoCache[weaponIndex] = gunInstances[weaponIndex].GetComponent<BaseWeapon>().RefillAmmo();
        }

        hasGun[weaponIndex] = true;
    }
   

    public void GiveHealth(float amount)
    {
        
        playerHealth += amount;
        if (playerHealth > maxHealth)
        {
            playerHealth = maxHealth;
        }
        PlayPickUpSound();
    }

    public void GiveArmor(float amount)
    {
        playerArmor += amount;
        if (playerArmor > maxArmor)
        {
            playerArmor = maxArmor;
        }
        PlayPickUpSound();
    }

    private void PlayPickUpSound()
    {
        if (!isLocalPlayer) return;
        //audioSources[0].clip = pickupSound;
        //audioSources[0].Play();
    }


    private void NextWeapon()
    {
        int nextIndex = currentGunIndex + 1;
        if (nextIndex > gunInstances.Count - 1)
        {
            nextIndex = 0;
        }

        while (!hasGun[nextIndex])
        {

            nextIndex = nextIndex + 1;
            if (nextIndex > gunInstances.Count - 1)
            {
                nextIndex = 0;
            }

        }

        CmdSetWeapon(nextIndex);
    }

    private void PreviousWeapon()
    {

        int nextIndex = currentGunIndex - 1;
        if (nextIndex < 0)
        {
            nextIndex = gunInstances.Count - 1; ;
        }

        while (!hasGun[nextIndex])
        {

            nextIndex = nextIndex - 1;
            if (nextIndex < 0)
            {
                nextIndex = gunInstances.Count - 1; ;
            }

        }
        CmdSetWeapon(nextIndex);
        
    }
    [ClientRpc]
    public void RpcRocketJump(Vector3 direction, float force)
    {
        movement.getRigidBody().AddForce(Vector3.Normalize(direction)* force, ForceMode.Impulse);   
    }


    private void TogglePlayerPauseMenu()
    {
        bool screenIsPaused = playerUI.TogglePauseScreen();
        mouseLook.SetCursorLock(!screenIsPaused);
        movement.enabled = !screenIsPaused;
    }

    private void ToggleScoreboard()
    {
        bool scoraboardIsShown = playerUI.ToggleScoreboard();
    }

    public void OnPlayerDisconnect()
    {   
        MatchInfo match = NetworkManager.singleton.matchInfo;
        NetworkManager.singleton.matchMaker.DropConnection(match.networkId, match.nodeId, 0, OnMatchDropConnection);
        NetworkManager.singleton.StopHost();
    }

    public void OnMatchDropConnection(bool success, string extendedInfo)
    {
        // ...
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "FallOffTrigger")
        {
            TakeDamage(1000.0f, this);
        }
    }



}
