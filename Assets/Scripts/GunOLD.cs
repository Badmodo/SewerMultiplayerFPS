using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GunOLD : MonoBehaviour
{
    public enum GunType { Semi, Burst, Auto};
    public GunType gunType;

    public float damage = 10f;
    public float range = 200f;
    public float fireRate = 20f;
    public float gunID;
    public float rpm;
    public float cameraShaker = 0.4f;

    public int totalAmmo = 40;
    public int ammoMag = 10;

    public Transform spawn;
    public Transform shellEjectionPort;
    public Rigidbody shell;
    public LayerMask collisionMask;
    public AudioSource GunFire;
    public AudioSource hitTarget;

    //[HideInInspector]
    //public GameGUI gUI;

    private float secondsBetweenShots;
    private float nextPossibleShootTime;
    [HideInInspector]
    public int currentAmmoInMag;
    private bool reloading;

    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    //public CameraShake cameraShake;

    private float NextTimToFire = 0f;

    private void Start()
    {
        secondsBetweenShots = 60 / rpm;

        currentAmmoInMag = ammoMag;

        //if (gUI)
        //{
        //    gUI.SetAmmoInfo(totalAmmo, currentAmmoInMag);
        //}
    }

    private void Update()
    {
        fpsCam = transform.GetComponentInParent<Camera>();
        //cameraShake = transform.GetComponentInParent<CameraShake>();
    }

    public void Shoot()
    {
        if (CanShoot())
        {
            muzzleFlash.Play();

            //StartCoroutine(cameraShake.Shake(.15f, cameraShaker));

            RaycastHit hit;
            if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
            {
                Debug.Log(hit.transform.name);

                //Target target = hit.transform.GetComponent<Target>();
                //if (target != null)
                //{
                //    target.TakeDamage(damage);
                //}

                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }

            nextPossibleShootTime = Time.time + secondsBetweenShots;

            currentAmmoInMag--;

            //if(gUI)
            //{
            //    gUI.SetAmmoInfo(totalAmmo, currentAmmoInMag);
            //}

            ////test ammo going down
            //Debug.Log(currentAmmoInMag + "      " + totalAmmo);

            GunFire.Play();

            Rigidbody newShell = Instantiate(shell, shellEjectionPort.position, Quaternion.identity) as Rigidbody;
            newShell.AddForce(shellEjectionPort.forward * Random.Range(150f, 200f) + spawn.forward * Random.Range(-10, 10));

            ////for testing
            //Debug.DrawRay(ray.origin, ray.direction * shotDistance, Color.red, 1);
        }
    }

    public void ShootContinuous()
    {
        if(gunType == GunType.Auto)
        {
            Shoot();
        }
    }

    public bool CanShoot()
    {
        bool canShoot = true;

        if (Time.time < nextPossibleShootTime)
        {
            canShoot = false;
        }

        if(currentAmmoInMag == 0)
        {
            canShoot = false;
        }

        if(reloading)
        {
            canShoot = false;
        }

        return canShoot;
    }

    //if we have ammo left and mag is not full we can reload
    public bool Reload()
    {
        if(totalAmmo != 0 && currentAmmoInMag != ammoMag)
        {
            reloading = true;
            return true;
        }
        return false;
    }

    public void FinishReload()
    {
        reloading = false;
        currentAmmoInMag = ammoMag;
        totalAmmo -= ammoMag;
        if(totalAmmo < 0)
        {
            currentAmmoInMag += totalAmmo;
            totalAmmo = 0;
        }

        //if (gUI)
        //{
        //    gUI.SetAmmoInfo(totalAmmo, currentAmmoInMag);
        //}
    }
}
