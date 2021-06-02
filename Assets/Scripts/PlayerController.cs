using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//to use the photon hashtable not unitys
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
	[SerializeField] Image healthbarImage;
	[SerializeField] GameObject ui;

	[SerializeField] GameObject cameraHolder;

	[SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;

	[SerializeField] Item[] items;

	int itemIndex;
	int previousItemIndex = -1;

	float verticalLookRotation;
	bool grounded;
	Vector3 smoothMoveVelocity;
	Vector3 moveAmount;

	Rigidbody rb;

	PhotonView PV;

	const float maxHealth = 100f;
	float currentHealth = maxHealth;

	PlayerManager playerManager;

	private float nextFire;
	List<SingleShotGun> gunList;

	//Torch 
	public float timeRemaining = 10;
	public bool timerIsRunning = false;
	public TMP_Text timeText;
	public GameObject light;
	public GameObject torchTimer;

	private bool canShoot = true;


	////Animator
	//public Animator animator;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		PV = GetComponent<PhotonView>();

		gunList = GetComponentsInChildren<SingleShotGun>().ToList();

		playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
	}

	void Start()
	{
		if(PV.IsMine)
		{
			EquipItem(0);
		}
		else
		{
			Destroy(GetComponentInChildren<Camera>().gameObject);
			Destroy(rb);
			Destroy(ui);
		}

		//animator.SetBool("isIdle", true);
	}

	void Update()
	{
		if(!PV.IsMine)
			return;

		for(int i = 0; i < items.Length; i++)
		{
			if(Input.GetKeyDown((i + 1).ToString()))
			{
				EquipItem(i);

				break;
			}
		}

		if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
		{
			if(itemIndex >= items.Length - 1)
			{
				EquipItem(0);
			}
			else
			{
				EquipItem(itemIndex + 1);
			}
		}
		else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
		{
			if(itemIndex <= 0)
			{
				EquipItem(items.Length - 1);
			}
			else
			{
				EquipItem(itemIndex - 1);
			}
		}

		if(Input.GetMouseButtonDown(0) && canShoot) //Time.time > nextFire)
		{
			GunInfo info = items[itemIndex].itemInfo as GunInfo;

			nextFire = Time.time + info.fireRate;

			StartCoroutine(Shoot(info.fireRate));

			canShoot = false;

            //items[itemIndex].Use();
		}

		if(transform.position.y < -10f) // Die if you fall out of the world
		{
			Die();
		}

		//start the characters light
		if (Input.GetKeyDown(KeyCode.Q))
		{
			StartCoroutine(PlayerLight());
		}

		//while timer is running the light stays on
		if (timerIsRunning)
		{
			if (timeRemaining > 0)
			{
				timeRemaining -= Time.deltaTime;
				DisplayTime(timeRemaining);
			}
			else
			{
				timeRemaining = 0;
				timerIsRunning = false;
			}
		}
	}

	IEnumerator Shoot(float _fireRateDelay)
    {
		items[itemIndex].Use();

		yield return new WaitForSeconds(_fireRateDelay);

		canShoot = true;
	}

	IEnumerator PlayerLight()
	{
		torchTimer.SetActive(true);
		light.SetActive(true);
		timerIsRunning = true;
		yield return new WaitForSeconds(10f);
		torchTimer.SetActive(false);
		light.SetActive(false);
		timeRemaining = 10;
	}

	void Look()
	{
		transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

		verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

		cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
	}

	void Move()
	{
		Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

		moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
		rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);

	}

	void Jump()
	{
		if(Input.GetKeyDown(KeyCode.Space) && grounded)
		{
			rb.AddForce(transform.up * jumpForce);
		}
	}
		void DisplayTime(float timeToDisplay)
	{
		timeToDisplay += 1;

		float seconds = Mathf.FloorToInt(timeToDisplay % 60);

		timeText.text = string.Format("{0:00}", seconds);
	}

	void EquipItem(int _index)
	{
		if(_index == previousItemIndex)
			return;

		itemIndex = _index;

		items[itemIndex].itemGameObject.SetActive(true);

		if(previousItemIndex != -1)
		{
			items[previousItemIndex].itemGameObject.SetActive(false);
		}

		previousItemIndex = itemIndex;

		if(PV.IsMine)
		{
			Hashtable hash = new Hashtable();
			hash.Add("itemIndex", itemIndex);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
		}
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if(!PV.IsMine && targetPlayer == PV.Owner)
		{
			//possibly can add in different sprites here for the different guns
			EquipItem((int)changedProps["itemIndex"]);
		}
	}

	public void SetGroundedState(bool _grounded)
	{
		grounded = _grounded;
	}

	void FixedUpdate()
	{
		if(!PV.IsMine)
			return;

		Look();
		Move();
		Jump();
	}

	public void TakeDamage(float damage)
	{
		PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
	}

	[PunRPC]
	void RPC_TakeDamage(float damage)
	{
		if(!PV.IsMine)
			return;

		currentHealth -= damage;

		healthbarImage.fillAmount = currentHealth / maxHealth;

		if(currentHealth <= 0)
		{
			Die();
		}
	}

	void Die()
	{
		playerManager.Die();
	}
}