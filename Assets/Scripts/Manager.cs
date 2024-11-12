using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Manager : MonoBehaviour
{
    public string player_prefab;
	public Transform[] Spawn_Points;

	private void Start()
	{
		Spawn();
	}

	public void Spawn()
	{
		Transform t_spawnc = Spawn_Points[Random.Range(0, Spawn_Points.Length)];
		PhotonNetwork.Instantiate(player_prefab, t_spawnc.position, t_spawnc.rotation);
	}
}
