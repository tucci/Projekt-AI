using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {

	public Transform playerview;

	void Update () {
		transform.LookAt(playerview);
	}
}