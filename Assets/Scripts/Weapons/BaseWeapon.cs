using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public struct WeaponFireData
{
    public bool shouldShowNozzleEffect;
    public Vector3 hitPos;
    
};

public abstract class BaseWeapon : MonoBehaviour
{


    
    public int bulletClipCount;
    public AudioClip weaponFireSound;

    public ParticleSystem nozzleEffect;


    protected WeaponEnum weaponEnum = WeaponEnum.MUST_OVERRIDE;
    protected Transform bulletSpawn;
    protected int bulletsRemaining;
    protected PlayerClientController owner;
    [SerializeField]
    protected Camera playerCamera;

    protected WeaponType weaponType;
    protected WeaponFireData lastData;



    public virtual void ShowNozzleEffect(WeaponFireData data)
    {

    }

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        bulletSpawn = transform.Find("BulletSpawn");
        //playerCamera  = GetComponentInParent<Camera>();
        owner = transform.parent.GetComponentInParent<PlayerClientController>();
    }

	
	// Update is called once per frame
	public virtual void Update () {

    }

    public abstract void Fire();

    public abstract void FireHold();

    public virtual void FireHoldRelease()
    {
    }

    public virtual int GetBulletsRemaining()
    {
        return bulletsRemaining;
    }

    public WeaponFireData GetLastWeaponFireData()
    {
        return lastData;
    }


    public WeaponEnum GetWeaponEnum()
    {
        return weaponEnum;
    }

    public WeaponType GetWeaponType()
    {
        return weaponType;
    }

    public int RefillAmmo()
    {
        bulletsRemaining = bulletClipCount;
        return bulletsRemaining;       
    }

    public void resetWeaponForRespawn()
    {
        bulletsRemaining = 0;
    }
}
