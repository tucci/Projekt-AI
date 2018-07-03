using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BeamWeapon : BaseWeapon
{

    

    public float beamDamage = 5f;
    public float startWidth = 0.2f;
    public float endWidth = 0.2f;

    private LineRenderer beam;
    public ParticleSystem hitEffect;








    protected override void Awake()
    {
        weaponEnum = WeaponEnum.BeamWeapon;
        weaponType = WeaponType.LineRendererType;
        bulletClipCount = 750;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        beam = GetComponent<LineRenderer>();
        beam.enabled = false;
        beam.startWidth = startWidth;
        beam.endWidth = endWidth;
        hitEffect.transform.parent = null;
        
    }

    public override void Update()
    {
        base.Update();
        
        if (!enabled)
        {
            nozzleEffect.Stop();
        }
        



    }



    public override void Fire()
    {
    }

    public override void ShowNozzleEffect(WeaponFireData data)
    {
        if (data.shouldShowNozzleEffect)
        {


            beam.SetPosition(0, bulletSpawn.transform.position);
            beam.SetPosition(1, data.hitPos);

            // Align the particle effect with the beam direction
            nozzleEffect.transform.right = beam.GetPosition(1) - beam.GetPosition(0);
            nozzleEffect.transform.position = beam.GetPosition(0);
            hitEffect.transform.position = beam.GetPosition(1);

            hitEffect.Play();
            nozzleEffect.Play();

            //beam.enabled = true;
        }
        else
        {
            FireHoldRelease();
        }

    }

    public override void FireHoldRelease()
    {
        
        nozzleEffect.Stop();
        nozzleEffect.Clear();
        hitEffect.Stop();
        hitEffect.Clear();
        //beam.enabled = false;
    }

    IEnumerator FireBeam()
    {



        //lastData.shouldShowNozzleEffect = false;
        //if (bulletsRemaining > 0)
        //{

        //    lastData.shouldShowNozzleEffect = true;
            
        //    bulletsRemaining = bulletsRemaining - 1;

        //    beam.SetPosition(0, bulletSpawn.position);

        //    beam.startWidth =  startWidth + (0.05f * Mathf.Sin(50 * Time.time));
        //    beam.endWidth = beam.startWidth;
            
            

        //    Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        //    //Ray ray = new Ray(transform.position, transform.up);
        //    RaycastHit hit;
            
        //    if (Physics.Raycast(ray, out hit) && (hit.distance > 1))
        //    {
                
        //        beam.SetPosition(1, hit.point);
                
        //        var player = hit.transform.GetComponent<PlayerClientController>();
        //        if (player != null)
        //        {
        //            if (player.playerHealth > 0)
        //            {
        //                owner.SetHitmarker(true);
        //                player.TakeDamage(beamDamage);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        beam.SetPosition(1, ray.GetPoint(100));
        //    }

            yield return null;

        //}
    }



    public override void FireHold()
    {
        //StopCoroutine("FireBeam");
        //StartCoroutine("FireBeam");

        lastData.shouldShowNozzleEffect = false;
        if (bulletsRemaining > 0)
        {

            //bulletSpawn.position = (playerCamera.transform.position + bulletSpawn.position) * 0.5f +
            //                       (0.5f * playerCamera.transform.forward);
            lastData.shouldShowNozzleEffect = true;
            

            bulletsRemaining = bulletsRemaining - 1;

            beam.SetPosition(0, bulletSpawn.position);

            beam.startWidth = startWidth + (0.05f * Mathf.Sin(50 * Time.time));
            beam.endWidth = beam.startWidth;


            

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            //Ray ray = new Ray(transform.position, transform.up);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && (hit.distance > 1))
            {

                beam.SetPosition(1, hit.point);

                var player = hit.transform.GetComponent<PlayerClientController>();
                if (player != null)
                {
                    if (player.playerHealth > 0)
                    {
                        owner.SetHitmarker(true);
						player.TakeDamage(beamDamage, player);
                    }
                }
            }
            else
            {
                beam.SetPosition(1, ray.GetPoint(100));
            }
            lastData.hitPos = beam.GetPosition(1);
        }
    }

}
