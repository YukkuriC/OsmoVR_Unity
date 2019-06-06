using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCamera : MonoBehaviour {
	public Vector2 sensitivity=Vector2.one;
	static float cumx,cumy;
	public static void Reset(){
		cumx=0;
	}
	void Update () {
		cumx+=Input.GetAxis("Mouse X")*sensitivity.x;
		cumy+=Input.GetAxis("Mouse Y")*sensitivity.y;
		cumy=Mathf.Clamp (cumy, -90, 90);
		transform.rotation = Quaternion.Euler(-cumy, cumx, 0);
	}
}
