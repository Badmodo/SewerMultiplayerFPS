using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotGun : Gun
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

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
		ray.origin = cam.transform.position;
		if(Physics.Raycast(ray, out RaycastHit hit))
		{
			hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
			PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
		}

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
		if(colliders.Length != 0)
		{
			GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
			Destroy(bulletImpactObj, 10f);
			bulletImpactObj.transform.SetParent(colliders[0].transform);
		}
	}
}
