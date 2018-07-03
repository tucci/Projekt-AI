using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {
    Vector3 middleVector = new Vector3(-17.12f, 0.0f, -17.24f);

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

        transform.RotateAround(middleVector, Vector3.up, 15 * Time.deltaTime);
	}
}
