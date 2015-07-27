﻿using UnityEngine;
using System.Collections;

public class ElevatorScript : MonoBehaviour {

	public float currentTorque;

	// Use this for initialization
	void Start () {
        rigidbody.useGravity = false;
	}
	
	// Update is called once per frame
	void Update () {
		//TODO this is all placeholder stuff
		Vector3 forceDirection = Vector3.up;//transform.localToWorldMatrix*Vector3.up;
		Vector3 force = forceDirection * currentTorque;
		rigidbody.AddForce (force*10+Physics.gravity, ForceMode.Acceleration);
	}


}
