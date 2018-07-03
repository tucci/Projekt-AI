using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngineInternal;

public class HitscanWeapon : BaseWeapon
{


    
    
    


    public int hitDamage = 10;

    private float fireRate;
    private float fireTimer;
    private bool canShoot;
    

    private float trailTime = 0.3f;
    private float timeElaspedSinceShot;
    private Vector3 lastHitScanEndPos;

  

    protected override void Awake()
    {
        weaponEnum = WeaponEnum.Hitscan;
        weaponType = WeaponType.LineRendererType;
        bulletClipCount = 50;
        
        canShoot = true;
        fireRate = 0.25f;
        fireTimer = fireRate;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        nozzleEffect.transform.parent = null;

    }





    // Update is called once per frame
    void Update()
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

    public override void ShowNozzleEffect(WeaponFireData data)
    {
        if (data.shouldShowNozzleEffect)
        {
            nozzleEffect.transform.forward = data.hitPos - bulletSpawn.transform.position;
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
                canShoot = false;
                bulletsRemaining = bulletsRemaining - 1;

                Ray shot = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(shot, out hit))
                {
                    lastHitScanEndPos = hit.point;
                    var player = hit.transform.GetComponent<PlayerClientController>();
                    if (player != null)
                    {
                        owner.SetHitmarker(true);
						player.TakeDamage(hitDamage, this.owner);
                    }
                }
                else
                {
                    lastHitScanEndPos = shot.GetPoint(100);
                }
                lastData.hitPos = lastHitScanEndPos;

            }
        }
    }


    public override void FireHold()
    {
        lastData.shouldShowNozzleEffect = false;
    }
}
