using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LobWeapon : BaseWeapon {

    public GameObject projectilePrefab;
    public float projectileSpeed;

    private float fireRate;
    private float fireTimer;
    private bool canShoot;


    protected override void Awake()
    {
        weaponEnum = WeaponEnum.LobWeapon;
        weaponType = WeaponType.Projectile;
        bulletClipCount = 10;
        canShoot = true;
        fireRate = .75f;
        fireTimer = fireRate;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        nozzleEffect.transform.parent = null;
    }


    // Update is called once per frame
    public override void Update()
    {
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



    public override void Fire()
    {
        if (canShoot)
        {

            if (bulletsRemaining > 0)
            {
                bulletsRemaining = bulletsRemaining - 1;
                canShoot = false;

                var bulletShow = (GameObject)Instantiate(
                    projectilePrefab,
                    bulletSpawn.position,
                    bulletSpawn.rotation
                );

                // Add velocity to the bullet
                //bulletShow.GetComponent<Rigidbody>().AddForce(bulletSpawn.transform.up * projectileSpeed, ForceMode.Impulse);
                bulletShow.GetComponent<Rigidbody>().velocity = bulletSpawn.transform.up * projectileSpeed;
                bulletShow.GetComponent<BaseProjectile>().SetPlayerOwner(owner);

                // Spawn the bullet on the Clients
                NetworkServer.Spawn(bulletShow);

                // Destroy the bullet after 2 seconds
                Destroy(bulletShow, 2.0f);
            }
        }


    }

    public override void  FireHold()
    {
        
    }
}
