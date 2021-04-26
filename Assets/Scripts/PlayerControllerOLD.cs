using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerOLD : MonoBehaviour
{

    public float orinigalSpeed = 8f;
    public float speed = 8f;
    public float runSpeed = 12f;
    public float gravity = -8f;
    public float jumpPower = 3f;

    public float timeRemaining = 10;
    public bool timerIsRunning = false;
    public TMP_Text timeText;

    public Transform handHold;
    //public GameCamera shake;
    public GameObject light;
    public GameObject torchTimer;

    //public AudioSource warning;
    //public AudioSource torchStart;
    //public AudioSource jump;
    //public AudioSource step;

    private Vector3 currentVelocityModifier;    
    private Quaternion targetRotation;

    public Gun[] guns;

    private bool reloading;
    private Gun currentGun;
    private CharacterController controller;
    private Camera _camera;
    //private GameGUI gUI;

    public Animator animator;

    public Transform groundCheck;

    public LayerMask groundMask;

    public float groundDistance = 0.4f;

    public static bool isGrounded = true;

    Vector3 velocity;

    int gunCheckID;

    PhotonView PV;

    void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
        }

        _camera = Camera.main;
        controller = GetComponent<CharacterController>();

        //gUI = GameObject.FindGameObjectWithTag("GUI").GetComponent<GameGUI>();

        //start game with handgun
        Equip(0);
        animator.SetBool("isIdle", true);
    }

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void Update()
    {
        if(!PV.IsMine)
        {
            return;
        }

        Move();

        //if (currentGun)
        //{
        //    if (Input.GetButtonDown("Shoot"))
        //    {
        //        currentGun.Shoot();
        //    }
        //    else if (Input.GetButton("Shoot"))
        //    {
        //        currentGun.ShootContinuous();
        //    }

        //    if(Input.GetButtonDown("Reload"))
        //    {
        //        if(currentGun.Reload())
        //        {
        //            reloading = true;
        //            //StartCoroutine(ReloadAnimation());
        //        }
        //    }
        //    if(reloading)
        //    {
        //        currentGun.FinishReload();
        //        reloading = false;
        //    }
        //}

        //IEnumerator ReloadAnimation()
        //{
        //    animator.SetBool("isIdle", false);
        //    animator.SetBool("isReloadPistol", true);
        //    yield return new WaitForSeconds(2f);
        //    animator.SetBool("isReloadPistol", false);
        //    animator.SetBool("isIdle", true);
        //}

        //using 1 - 9 to change guns depending on what the palyer has
        for (int gunCheckID = 0; gunCheckID < guns.Length; gunCheckID++)
        {   
            if (Input.GetKeyDown((gunCheckID + 1) + "") || Input.GetKeyDown("[" + (gunCheckID + 1) + "]"))
            {
                Equip(gunCheckID);
                break;
            }
        }

        //Jump
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //jump power calculation
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);
        }

        //adding gravity
        velocity.y += gravity * Time.deltaTime;
        //controller.Move(velocity * Time.deltaTime);

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

        //run
        if (Input.GetButton("Run"))
        {
            speed = runSpeed;
        }
        else
        {
            speed = orinigalSpeed;
        }
    }

    IEnumerator PlayerLight()
    {
        //torchStart.Play();
        torchTimer.SetActive(true);
        light.SetActive(true);
        timerIsRunning = true;
        yield return new WaitForSeconds(10f);
        //warning.Play();
        torchTimer.SetActive(false);
        light.SetActive(false);
        timeRemaining = 10;
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timeText.text = string.Format("{0:00}", seconds);
    }

    void Move()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        //controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        //controller.Move(velocity * Time.deltaTime);
    }



    //equiping a gun to the hand slot!
    void Equip(int i)
    {
        if(currentGun)
        {
            Destroy(currentGun.gameObject);
        }

        currentGun = Instantiate(guns[i], handHold.position, handHold.rotation) as Gun;
        currentGun.transform.parent = handHold;
        //currentGun.gUI = gUI;
    }
}
