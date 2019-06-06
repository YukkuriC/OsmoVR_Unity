using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPositionHolder : MonoBehaviour {
    Transform cam;
    void Awake () {
        cam = GetComponentInChildren<Camera> ().transform;
    }

    // Update is called once per frame
    void Update () {
        transform.localPosition = -cam.localPosition;
    }
}