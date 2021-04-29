using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    [SerializeField] Camera cam;

    PhotonView PV;

    public ParticleSystem muzzleFlash;
    public float cameraShaker = 0.4f;
    public CameraShake cameraShake;

    public AudioSource GunFire;
    public GameObject muzzleFlashLight;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        bulletOffsets = new List<Vector3>()
        {
            new Vector3(0.5f, 0.5f, 0.0f),
            new Vector3(0.5f, 0.55f, 0.0f),
            new Vector3(0.5f, 0.45f, 0.0f),
            new Vector3(0.53f, 0.5f, 0.0f),
            new Vector3(0.47f, 0.5f, 0.0f),
        };
    }

    public override void Use()
    {
        Shoot();
    }

    void Shoot()
    {
        muzzleFlash.Play();


        StartCoroutine(muzzleFlashFlash());
        StartCoroutine(cameraShake.Shake(.15f, cameraShaker));

        List<Ray> rays = new List<Ray>();

        foreach (Vector3 bulletOffset in bulletOffsets)
        {
            rays.Add(fpsCam.ViewportPointToRay(bulletOffset));
        }

        // fire the rays
        foreach (Ray ray in rays)
        {
            Fire(ray);
        }

        //Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        //ray.origin = cam.transform.position;
        //if (Physics.Raycast(ray, out RaycastHit hit))
        //{
        //    hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
        //    PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
        //}

        GunFire.Play();
    }

    IEnumerator muzzleFlashFlash()
    {
        muzzleFlashLight.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        muzzleFlashLight.SetActive(false);
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }

    public int shotgunForce = 50;
    public int shotgunDamage = 10;

    public Transform gunEnd;
    public Camera fpsCam;
    public GameObject impactEffect;

    private List<Vector3> bulletOffsets;


    //private void Update()
    //{
    //    // Check if the player has pressed the fire button and if enough time has elapsed since they last fired
    //    if (Input.GetKey(KeyCode.Mouse0) && Time.time > nextFire)
    //    {
    //        // Update the time when our player can fire next
    //        nextFire = Time.time + fireRate;
    //        PlayerFireWeapon();
    //    }
    //}

    //void PlayerFireWeapon()
    //{

    //    make a new list of rays
    //    List<Ray> rays = new List<Ray>();

    //    create the rays based on the configured offsets
    //    foreach (Vector3 bulletOffset in bulletOffsets)
    //    {
    //        rays.Add(fpsCam.ViewportPointToRay(bulletOffset));
    //    }

    //    // fire the rays
    //    foreach (Ray ray in rays)
    //    {
    //        Fire(ray);
    //    }
    //}

    // assuming all the bullets will do the same thing
    private void Fire(Ray ray)
    {
        RaycastHit hit;
        // Check if our raycast has hit anything
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (Physics.Raycast(ray, out RaycastHit Hit))
            {
                Hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
                PV.RPC("RPC_Shoot", RpcTarget.All, Hit.point, Hit.normal);
            }

            // if the object hit has as target script attached, run the take damage function
            //Target target = hit.transform.GetComponent<Target>();
            //if (target != null)
            //{
            //    target.takeDamage(shotgunDamage);
            //}

            // Check if the object we hit has a rigidbody attached
            if (hit.rigidbody != null)
            {
                // Add force to the rigidbody we hit, in the direction from which it was hit
                hit.rigidbody.AddForce(-hit.normal * shotgunForce);
            }
        }
    }
}
