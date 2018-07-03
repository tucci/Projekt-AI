using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class QuickProjectileWeapon : BaseWeapon {

    public GameObject projectilePrefab;
    
    public float projectileSpeed;

    private float fireRate;
    private float fireTimer;
    private bool canShoot;

    
    private float destroyTime;





    // Use this for initialization
    protected override void Awake() {

        weaponEnum = WeaponEnum.QuickProjectileWeapon;
        weaponType = WeaponType.Projectile;
        projectileSpeed = 50.0f;
        bulletClipCount = 25;
        canShoot = true;
        fireRate = 0.3f;
        fireTimer = fireRate;
        base.Awake();

    }

    protected override void Start()
    {
        base.Start();
        destroyTime = nozzleEffect.main.duration;
        nozzleEffect.Stop();
        nozzleEffect.transform.parent = null;
    }

    // Update is called once per frame
    void Update () {

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

    IEnumerator FireProjectiles()
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
                    projectilePrefab,
                    realBulletSpawn,
                    bulletSpawn.rotation
                );

                //Ray shot = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

                //RaycastHit hit;
                Vector3 dir;
                //if (Physics.Raycast(shot, out hit))
                //{
                //    dir = Vector3.Normalize(hit.point - bulletSpawn.position);
                //}
                //else
                //{
                    
                //    dir = shot.direction;
                //}
                dir = playerCamera.transform.forward;

                // Add velocity to the bullet
                bulletShow.GetComponent<Rigidbody>().velocity = dir * projectileSpeed;
                bulletShow.GetComponent<BaseProjectile>().SetPlayerOwner(owner);


                // Spawn the bullet on the Clients
                NetworkServer.Spawn(bulletShow);
                Destroy(bulletShow, 5);



                yield return null;
            }
        }

    }

    public override void FireHold()
    {
        StopCoroutine("FireProjectiles");
        StartCoroutine("FireProjectiles");
    }
}
