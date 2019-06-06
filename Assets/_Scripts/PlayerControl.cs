using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {
    public static PlayerControl instance;
    public static Cell cell;
    public static Vector3 deadPosition;
    public static float lastShoot;
    Transform cam;

    void Awake () {
        instance = this;
        GetComponent<MeshRenderer> ().material.color = Color.blue;
        cam = Camera.main.transform;
        lastShoot = Time.time;
    }

    void Update () {
        if (Time.time - lastShoot >= Game.instance.consts.shootTime) {
            if (Game.use_VR) {
                if (VRControl.instance.PlayerControlShoot ()) {
                    lastShoot = Time.time;
                }
            } else if (Input.GetMouseButton (0)) {
                Game.instance.Eject (cell, cam.forward);
                lastShoot = Time.time;
            }
        }
    }

    void OnDestroy () {
        deadPosition = transform.position;
        Game.GameOver ();
    }

    static float _size;
    public static float size {
        get {
            if (cell != null) _size = cell.transform.localScale.x;
            return _size;
        }
    }
}