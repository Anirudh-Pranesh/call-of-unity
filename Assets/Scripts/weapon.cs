using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class weapon : MonoBehaviourPunCallbacks
{

	#region Variables
	public Gun[] loadout;
	private int currentIndex;
    public Transform weaponParent;
    private GameObject currentWeapon;
	public GameObject bulletHolePrefab;
	public LayerMask canBeShot;
	private float currentCoolDown;

	#endregion

	#region MonoBehavior Callbacks

    void Update()
    {

		if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1))
		{
			photonView.RPC("equip", RpcTarget.All, 0);
		}
		if(currentWeapon != null)
		{
			if (photonView.IsMine)
			{
				Aim(Input.GetMouseButton(1));

				if (Input.GetMouseButtonDown(0) && currentCoolDown <= 0)
				{
					photonView.RPC("shoot", RpcTarget.All);
				}

				if (currentCoolDown > 0) currentCoolDown -= Time.deltaTime;
			}
			//weapon elasticity

			currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 5f);

			
		}
        
    }
	#endregion

	#region private methods

	[PunRPC]
	void equip(int p_ind)
	{
        if (currentWeapon != null) Destroy(currentWeapon);

		currentIndex = p_ind;

        GameObject t_newWeapon = Instantiate (loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        t_newWeapon.transform.localPosition = Vector3.zero;
        t_newWeapon.transform.localEulerAngles = Vector3.zero;

		t_newWeapon.GetComponent<Weapon_Sway>().isMine = photonView.IsMine;

        currentWeapon = t_newWeapon;
		Debug.Log(currentWeapon.name);

    }

	void Aim(bool p_isAiming)
	{
		Transform t_anchor = currentWeapon.transform.Find("Anchor");
		Transform t_state_ads = currentWeapon.transform.Find("states/ADS");
		Transform t_state_hip = currentWeapon.transform.Find("states/Hip");

		if (p_isAiming)
		{
			//aim
			t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_ads.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
		}
		else
		{
			//hip
			t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_hip.position, Time.deltaTime * loadout[currentIndex].aimSpeed);

		}
	}

	[PunRPC]
	void shoot()
	{
		Transform t_spawn = transform.Find("Cameras/normal Camera");
		//bloom
		Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
		t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.up;
		t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.right;
		t_bloom -= t_spawn.position;
		t_bloom.Normalize();

		//cooldown

		currentCoolDown = loadout[currentIndex].firerate;
		

		//raycast
		RaycastHit t_hit = new RaycastHit();
		if (Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, canBeShot))
		{
			GameObject t_newHole = Instantiate(bulletHolePrefab, t_hit.point + t_hit.normal * 0.0001f, Quaternion.identity) as GameObject;
			t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
			Destroy(t_newHole, 5f);

			if (photonView.IsMine)
			{
				//shooting other players on network
				if(t_hit.collider.gameObject.layer == 8)
				{
					//RPC call to damage player

					t_hit.collider.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[currentIndex].damage);
				}
			}
		}

		//gun vfx

		currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
		currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;

		
	}

	[PunRPC]
	private void TakeDamage(int p_damage)
	{
		GetComponent<Player>().TakeDamage(p_damage);
	}

	#endregion
}
