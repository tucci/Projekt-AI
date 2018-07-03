using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public class RocketWeapon : BaseWeapon
{

    public GameObject rocketProjectilePrefab;
    
    

    public float projectileSpeed;

    private float fireRate;
    private float fireTimer;
    private bool canShoot;

    // Use this for initialization
    protected override void Awake()
    {
        weaponEnum = WeaponEnum.RocketLauncher;
        weaponType = WeaponType.Projectile;
        bulletClipCount = 10;
        
        canShoot = true;
        fireRate = 1f;
        fireTimer = fireRate;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        nozzleEffect.transform.parent = null;

    }

    // Update is called once per frame
    public override void Update () {
		base.Update();

        if (!canShoot)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer < 0)
            {
                canShoot = true;
                fireTimer = fireRate;
            }
        }
    }

    public override void ShowNozzleEffect(WeaponFireData data)
    {
        if (data.shouldShowNozzleEffect)
        {
            nozzleEffect.transform.position = bulletSpawn.transform.position;
            nozzleEffect.Play();
            nozzleEffect.Emit(10);
        }
        
        
        
    }


    public override void Fire()
    {
        lastData.shouldShowNozzleEffect = false;
        if (canShoot)
        {

            if (bulletsRemaining > 0)
            {
                lastData.shouldShowNozzleEffect = canShoot;
                bulletsRemaining = bulletsRemaining - 1;
                canShoot = false;

                Vector3 realBulletSpawn = playerCamera.transform.position + (2 * playerCamera.transform.forward);
               



                var bulletShow = (GameObject) Instantiate(
                    rocketProjectilePrefab,
                    realBulletSpawn,
                    bulletSpawn.rotation
                );


                //Ray shot = new Ray(playerCamera.transform.position, playerCamera.transform.forward);


                //Ray shot = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

                //RaycastHit hit;
                Vector3 dir;
                //if (Physics.Raycast(shot, out hit))
                //{
                //    dir = Vector3.Normalize(hit.point - bulletSpawn.position);
                //}
                //else
                //{
                //    dir = transform.up;
                //}

                dir = playerCamera.transform.forward;

                // Add velocity to the bullet
                bulletShow.GetComponent<Rigidbody>().velocity = dir * projectileSpeed;
                bulletShow.GetComponent<BaseProjectile>().SetPlayerOwner(owner);
                


                // Spawn the bullet on the Clients
                NetworkServer.Spawn(bulletShow);
                if (bulletShow != null)
                {
                    Destroy(bulletShow, 10);
                }
                

                
            }
        }


    }


    public override void FireHold()
    {
        
    }

}
