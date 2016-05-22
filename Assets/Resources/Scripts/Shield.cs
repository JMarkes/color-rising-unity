﻿using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour {

	private Transform player;
	private Vector3 offset;

	// Use this for initialization
	void Start () {
		player = GameObject.FindWithTag("Player").GetComponent<Transform>();
		offset = new Vector3(0,2,0);
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = player.position - offset;
		transform.Rotate(Vector3.back * 2);
	}
}
