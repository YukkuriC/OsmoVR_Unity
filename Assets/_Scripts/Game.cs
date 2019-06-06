// #define FIELD3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    #region settings
    public WorldConsts consts;
    public GameObject cell, playerCell;
    public PlayerSpawnParams playerSpawn;
    public CellSpawnGroup[] cellSpawns;
    #endregion

    #region static vars
    public static bool use_VR;
    public static Vector3 size;
    public static int stage;
    public static bool endgame;
    static float _time_start;
    public static float running_time {
        get {
            return Time.time - _time_start;
        }
    }
    #endregion

    #region cached objects
    public static Game instance;
    public static GameObject UI;
    public static Transform board;
    public static HashSet<GameObject> cells;
    #endregion

    #region methods
    public GameObject CreateCell (Vector3 pos, Vector3? vel, float radius) {
        GameObject obj = Instantiate (
            cell,
            pos,
            Random.rotation,
            transform
        );
        if (vel != null) {
            obj.GetComponent<Rigidbody> ().velocity = (Vector3) vel;
        }
        obj.transform.localScale = Vector3.one * radius;
        cells.Add (obj);
        return obj;
    }

    public GameObject CreatePlayer () {
        GameObject obj = Instantiate (
            playerCell,
            playerSpawn.pos,
            Random.rotation,
            transform
        );
        obj.transform.localScale = Vector3.one * playerSpawn.radius;
        Cell player = obj.GetComponent<Cell> ();
        PlayerControl.cell = player;
        cells.Add (obj);
        return obj;
    }

    public void Eject (Cell player, Vector3 direction) {
        if (player == null) return;
#if FIELD3D
        Vector3 dpos = (-direction).normalized;
#else
        Vector3 dpos = new Vector3 (-direction.x, 0, -direction.z).normalized;
#endif

        // velocity change
        Vector3 dv = dpos * consts.ejectSpeed;
        Vector3 new_v = player.body.velocity + (1 - consts.ejectRatio) * dv;
        player.body.velocity -= consts.ejectRatio * dv;

        // radius change
        float dm = player.mass * consts.ejectRatio;
        player.mass -= dm;
        float new_r = Mathf.Pow (dm, 1f / 3);

        // spawn on center
        CreateCell (player.transform.position + dpos * (consts.ejectSpeed * Time.deltaTime + (new_r + player.transform.localScale.x) * 0.5f), new_v, new_r);
    }

    public static void GameOver (bool win = false) {
        if (endgame) return;
        endgame = true;
        if (instance != null)
            instance.StartCoroutine (instance.GameOverCo (win));
    }

    public IEnumerator GameOverCo (bool win) {
        GameObject text1 = UI.transform.GetChild (0).gameObject;
        GameObject text2 = UI.transform.GetChild (1).gameObject;
        if (use_VR)
            text2.GetComponent<TextMesh> ().text = "按下触摸板重启";
        if (win)
            text1.GetComponent<TextMesh> ().text = "你赢了";
        text1.SetActive (true);
        GetComponent<AudioSource> ().Play ();
        yield return new WaitForSeconds (1);
        stage = 2;
        text2.SetActive (true);
    }

    public static void UpdateOnAbsorb () {
        if (cells.Count == 1) {
            Game.GameOver (PlayerControl.cell != null);
        }
    }
    #endregion

    void Awake () {
        instance = this;
        UI = GameObject.Find ("/UI Layer");
        Cell.consts = consts;
        cells = new HashSet<GameObject> ();
        board = transform.GetChild (0);
    }

    void Start () {
        stage = 0;
        endgame = false;
        size = consts.sizeInit;
        _time_start = Time.time;
        board.localScale = size * 2;

#if FIELD3D
        foreach (var spawn in cellSpawns) {
            spawn.count *= 2;
            spawn.massFrom *= 5;
            spawn.massTo *= 5;
        }
        playerSpawn.radius *= 5;
#endif

        // put player
        CreatePlayer ();

        // put cells
        foreach (var spawn in cellSpawns) {
            for (int i = 0; i < spawn.count; i++) {
                float r = Random.Range (spawn.massFrom, spawn.massTo);
                Vector3 pos = Vector3.zero;
                while (Vector3.Distance (pos, playerSpawn.pos) < playerSpawn.radius + playerSpawn.safeDistance + r) {
#if FIELD3D
                    pos = new Vector3 (Random.Range (-size.x, size.x), Random.Range (-size.y, size.y), Random.Range (-size.z, size.z));
#else
                    pos = new Vector3 (Random.Range (-size.x, size.x), -r * 5, Random.Range (-size.z, size.z));
#endif
                }
                Vector2 tmp = Random.insideUnitCircle * spawn.maxSpeed;
                CreateCell (pos, new Vector3 (tmp.x, 0, tmp.y), r);
            }
        }
    }

    void Update () {
        switch (stage) {
            case 1: // died
            case 2:
                if ((use_VR && VRControl.instance.GetRestart ()) || Input.GetKey (KeyCode.R)) {
                    SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
                }
                break;
        }

        // exit game
        if ((use_VR && VRControl.instance.GetExit ()) || Input.GetKey (KeyCode.Escape)) {
            Application.Quit ();
        }
    }

    void FixedUpdate () {
        // shrink game board
        float t = running_time;
        if (t > consts.timeShrink.x && t < consts.timeShrink.y) {
            float sep = (t - consts.timeShrink.x) / (consts.timeShrink.y - consts.timeShrink.x);
            size = Vector3.Lerp (consts.sizeInit, consts.sizeEnd, sep);
            board.localScale = size * 2;
        }
    }
}