// #define FIELD3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
    public static WorldConsts consts;

    #region properties
    public float mass {
        get {
            return transform.localScale.x * transform.localScale.y * transform.localScale.z;
        }
        set {
            float radius = Mathf.Pow (value, 1f / 3);
            transform.localScale = Vector3.one * radius;
        }
    }
    #endregion

    #region cached objects
    [HideInInspector]
    public Rigidbody body;
    [HideInInspector]
    public Material skin;
    AudioSource audio_pop;
    #endregion

    void Awake () {
        body = GetComponent<Rigidbody> ();
        skin = GetComponent<MeshRenderer> ().material;
        audio_pop = GetComponent<AudioSource> ();
    }

    void FixedUpdate () {
        switch (Game.stage) {
            case 1:
            case 2: // attract to endpoint
                body.AddForce ((PlayerControl.deadPosition - transform.position).normalized * consts.playerDeadForce);
                goto default;
            default:
                float vx = body.velocity.x, vy = body.velocity.y, vz = body.velocity.z;
                float px = transform.position.x, py = transform.position.y, pz = transform.position.z;

                // reflect on boundary
                if (Mathf.Abs (px) > Game.size.x) {
                    px = Game.size.x * (px > 0 ? 1 : -1);
                    if (px * vx > 0)
                        vx *= -1;
                }
                if (Mathf.Abs (pz) > Game.size.z) {
                    pz = Game.size.z * (pz > 0 ? 1 : -1);
                    if (pz * vz > 0)
                        vz *= -1;
                }

#if FIELD3D // bound on y
                if (Mathf.Abs (py) > Game.size.y) {
                    py = Game.size.y * (py > 0 ? 1 : -1);
                    if (py * vy > 0)
                        vy *= -1;
                }
#else // float to y=0
                vy -= transform.position.y * consts.floatAcc / Time.deltaTime;
                vy *= consts.floatFriction;
#endif

                body.velocity = new Vector3 (vx, vy, vz);
                transform.position = new Vector3 (px, py, pz);
                break;
        }
    }

    void Update () {
        // other cells update color
        Cell player = PlayerControl.cell;
        if (player != this) {
            float size_ratio = transform.localScale.x / PlayerControl.size;
            Color clr;
            if (size_ratio <= consts.cellDangerThreshold.x) {
                clr = Color.Lerp (consts.cellSmallest, consts.cellNear, size_ratio / consts.cellDangerThreshold.x);
            } else if (size_ratio <= consts.cellDangerThreshold.y) {
                clr = Color.Lerp (consts.cellNear, consts.cellDanger, (size_ratio - consts.cellDangerThreshold.x) / (consts.cellDangerThreshold.y - consts.cellDangerThreshold.x));
            } else {
                clr = Color.Lerp (consts.cellDanger, consts.cellDeadly, (size_ratio - consts.cellDangerThreshold.y) / (1 - consts.cellDangerThreshold.y));
            }
            skin.color = clr;
        }
    }

    void OnTriggerEnter (Collider c) {
        Cell other = c.GetComponent<Cell> ();
        if (other == null || other.gameObject == null) return;
        float ms = mass, mo = other.mass;
        if (ms > mo) {
            // sync position
            transform.position = (transform.position * ms + other.transform.position * mo) / (ms + mo);
            audio_pop.Play ();
            // sync velocity
            body.velocity = (body.velocity * ms + other.body.velocity * mo) / (ms + mo);
            // sync mass
            mass += mo;
            // destroy other
            Game.cells.Remove (other.gameObject);
            Destroy (other.gameObject);
            Game.UpdateOnAbsorb ();
        }
    }
}