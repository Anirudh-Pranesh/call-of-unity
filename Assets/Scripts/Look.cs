using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Look : MonoBehaviourPunCallbacks
{
	#region variables

	public static bool cursorLocked = true;
    public Transform player;
    public Transform cams;
    public float xSensitivity;
    public float ySensitivity;
    public float maxAngle;
    private Quaternion camCentre;
    public Transform weapon;

	#endregion


	#region Monobehavior Callbacks
	void Start()
    {
        camCentre = cams.localRotation;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        setY();
        setX();
        UpdateCursorLock();
    }

	#endregion


	#region private methods
	void setY()
	{
        float t_input = Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;
        Quaternion t_adj = Quaternion.AngleAxis(t_input, -Vector3.right);
        Quaternion t_delta = cams.localRotation * t_adj;

        if(Quaternion.Angle(camCentre, t_delta) < maxAngle)
		{
            cams.localRotation = t_delta;
        }

        weapon.rotation = cams.rotation;
	}

    void setX()
    {
        float t_input = Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
        Quaternion t_adj = Quaternion.AngleAxis(t_input, Vector3.up);
        Quaternion t_delta = player.localRotation * t_adj;
        player.localRotation = t_delta;
    }

    void UpdateCursorLock()
	{
		if (cursorLocked)
		{
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyDown(KeyCode.Escape))
			{
                cursorLocked = false;
			}
		}
		else
		{
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLocked = true;
            }
        }
	}
	#endregion
}

