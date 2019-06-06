using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFaceCamera : MonoBehaviour {
	public float distance;
	public float smoothFactor;
	Transform cam;
	void Awake () {
		cam = Camera.main.transform;
	}

	void Update () {
		// Debug.Log(cam.forward);
		Vector3 target = cam.position+ cam.forward * distance;
		transform.position = Vector3.Lerp (transform.position, target, smoothFactor);
		transform.LookAt (cam.position, cam.up);
	}
}