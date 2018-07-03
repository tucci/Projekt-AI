using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingRocks : MonoBehaviour {

    Vector3 middleVector = new Vector3(70f, -22f, -34f);
    float speed;
    // Use this for initialization
    void Start() {
        speed = Random.value * 65 + 10;
    }

    // Update is called once per frame
    void Update() {

        transform.RotateAround(middleVector, new Vector3(0f,Random.value,0f),  speed * Time.deltaTime);
    }
}
