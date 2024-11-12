using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player : MonoBehaviourPunCallbacks
{
    #region Variables

    public float speed;
    private Rigidbody rb;
    public int max_health;
    private int current_health;
    public float sprintModifier;
    public Camera normalCam;
    public GameObject camParent;
    public Transform weaponParent;
    private float baseFOV;
    private float sprintFOVmodifier = 1.75f;
    public float jumpForce;
    public Transform GrouundDetector;
    public LayerMask ground;
    private Vector3 weaponParentOrigin;
    private float movementCounter;
    private float idleCounter;
    private Vector3 targetWeaponBobPosition;
    private Manager manager;
    private Transform ui_healthbar;


    #endregion

    #region Private Methods
    void HeadBob(float p_z, float p_x_intenity, float p_y_intensity)
	{
        targetWeaponBobPosition =  weaponParentOrigin + new Vector3(Mathf.Cos(p_z) * p_x_intenity, Mathf.Sin(p_z * 2) * p_y_intensity, 0);
	}

    void refresHeathBar()
	{
        float t_health_ration = (float)current_health / (float)max_health;
        ui_healthbar.localScale = Vector3.Lerp(ui_healthbar.localScale, new Vector3(t_health_ration, 1, 1), Time.deltaTime * 8f);
	}

	#endregion

	#region MonoBehavior Callbacks
	void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();

        current_health = max_health;

        camParent.SetActive(photonView.IsMine);

		if (!photonView.IsMine) gameObject.layer = 8;

        baseFOV = normalCam.fieldOfView;
        if(Camera.main) Camera.main.enabled = false;
        rb = GetComponent<Rigidbody>();
        weaponParentOrigin = weaponParent.localPosition;
		if (photonView.IsMine)
		{
            ui_healthbar = GameObject.Find("HUD/Health/bar").transform;
            refresHeathBar();
        }
        
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        //axis

        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");

        //controls

        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);

        //states

        bool isGrounded = Physics.Raycast(GrouundDetector.position, Vector3.down, 0.1f, ground);
        bool isJumping = jump && isGrounded;
        bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;

        //jumping
        if (isJumping)
        {
            rb.AddForce(Vector3.up * jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(10);


        //head bobbing

        if (t_hmove == 0 && t_vmove == 0) 
        { 
            HeadBob(idleCounter, 0.025f, 0.025f); 
            idleCounter += Time.deltaTime;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);

        }
        else if (!isSprinting)
        { 
            HeadBob(movementCounter, 0.035f, 0.035f); 
            movementCounter += Time.deltaTime * 5f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);

        }
		else
		{
            HeadBob(movementCounter, 0.15f, 0.075f);
            movementCounter += Time.deltaTime * 7f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
        }

        //ui refresh
        refresHeathBar();
    }

	void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        //axis

        float t_hmove = Input.GetAxisRaw("Horizontal");
        float t_vmove = Input.GetAxisRaw("Vertical");

        //controls

        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetKeyDown(KeyCode.Space);

        //states

        bool isGrounded = Physics.Raycast(GrouundDetector.position, Vector3.down, 0.1f, ground);
        bool isJumping = jump && isGrounded;
        bool isSprinting = sprint && t_vmove > 0 && !isJumping && isGrounded;


		


        //movement
        Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
        t_direction.Normalize();

        float t_adjustedSpeed = speed;
        if (isSprinting) t_adjustedSpeed *= sprintModifier;


        Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * t_adjustedSpeed * Time.deltaTime;
        t_targetVelocity.y = rb.velocity.y;
        rb.velocity = t_targetVelocity;


        //FOV
		if (isSprinting)
		{
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVmodifier, Time.deltaTime * 8f);
		}
		else
		{
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
		}
    }
	#endregion

	#region Public methods
    public void TakeDamage(int p_damage)
	{
		if (photonView.IsMine)
		{
            current_health -= p_damage;
            refresHeathBar();
            if(current_health <= 0)
			{
                manager.Spawn();
                PhotonNetwork.Destroy(gameObject);
			}
        }
	}


	#endregion
}
