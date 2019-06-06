using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPlayerTracker : MonoBehaviour {
    public float AwayRatio, HeightRatio;
    public float smoothFactor;

    public float raiseSpeed;
    public float raiseHeight;

    Transform cam;

    void Awake () {
        cam = GetComponentInChildren<Camera> ().transform;
    }

    void FixedUpdate () {
        var cell = PlayerControl.cell;
        if (cell != null && Game.stage == 0) {
            var lookat = cam.forward.normalized;
            var radius = cell.transform.localScale.y;
            var tmp = cell.transform.position; // center
            tmp += new Vector3 (0, radius * HeightRatio, 0); // go up
            tmp -= lookat * (radius * AwayRatio);
            transform.position = Vector3.Lerp (transform.position, tmp, smoothFactor);
        } else if (transform.position.y < raiseHeight) {
            transform.position += Vector3.up * (raiseSpeed * Time.deltaTime);
        }
    }
}