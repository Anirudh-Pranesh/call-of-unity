using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Weapon_Sway : MonoBehaviourPunCallbacks
{
    #region Variables

    public float intensity;
    public float smooth;
	private Quaternion origin_rot;
	public bool isMine;
	#endregion

	#region MonoBehavior Callbacks


	private void Start()
	{
		origin_rot = transform.localRotation;
	}
	private void Update()
	{
		if (!photonView.IsMine) return;

		UpdateSway();
	}


	#endregion

	#region private methods

	private void UpdateSway()
	{
		//controls
		float t_x_mouse = Input.GetAxis("Mouse X");
		float t_y_mouse = Input.GetAxis("Mouse Y");

		if (!isMine)
		{
			t_x_mouse = 0f;
			t_y_mouse = 0f;
		}

		//calculate target rot.
		Quaternion t_x_adj = Quaternion.AngleAxis(-intensity * t_x_mouse, Vector3.up);
		Quaternion t_y_adj = Quaternion.AngleAxis(intensity * t_y_mouse, Vector3.right);
		Quaternion Target_rot = origin_rot * t_x_adj * t_y_adj;

		//rotate towards target rot.
		transform.localRotation = Quaternion.Lerp(transform.localRotation, Target_rot, Time.deltaTime * smooth);

	}

	#endregion
}
