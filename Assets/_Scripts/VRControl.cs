#define VR

using System.Collections;
using System.Collections.Generic;
#if VR
using UnityEngine;
using UnityEngine.XR;
#endif

public class VRControl : MonoBehaviour {
    public static VRControl instance;

#if VR
    InputDevice head;
    List<InputDevice> hands;
    public Transform handObj;
    public int handInUse = -1;
    bool to_refresh;
    void AddNode (XRNodeState obj) {
        updateInputDevices ();
    }

    void updateInputDevices () {
        Debug.Log ("Update");
        hands = new List<InputDevice> ();
        var tmp = new List<InputDevice> ();
        InputDevices.GetDevicesAtXRNode (XRNode.Head, tmp);
        Game.use_VR = tmp.Count >= 1;
        if (Game.use_VR) {
            head = tmp[0];
            Debug.Log ("Use VR");
            InputDevices.GetDevicesAtXRNode (XRNode.GameController, tmp);
            hands.AddRange (tmp);
            InputDevices.GetDevicesAtXRNode (XRNode.LeftHand, tmp);
            hands.AddRange (tmp);
            InputDevices.GetDevicesAtXRNode (XRNode.RightHand, tmp);
            hands.AddRange (tmp);
        }
    }

    void Awake () {
        instance = this;
        InputTracking.nodeAdded += AddNode;
        updateInputDevices ();
    }

    public void Update () {
        if (head != null) {
            if (to_refresh) {
                updateInputDevices ();
                to_refresh = false;
            }
        }
    }

    public void FixedUpdate () {
        if (head != null) {
            UpdateObjects ();
        }
    }

    void UpdateObjects () {
        // Vector3 pos, headPos;
        Quaternion rot, headRot;
        // head.TryGetFeatureValue (CommonUsages.devicePosition, out headPos);
        // head.TryGetFeatureValue (CommonUsages.deviceRotation, out headRot);
        if (handInUse >= 0) {
            var hand = hands[handInUse];
            if (hand.isValid) {
                // hand.TryGetFeatureValue (CommonUsages.devicePosition, out pos);
                hand.TryGetFeatureValue (CommonUsages.deviceRotation, out rot);
                // handObj.localPosition = (pos - headPos) * 100;
                handObj.rotation = rot;
            } else to_refresh = true;
        }
    }

    bool GetButtonDown (InputFeatureUsage<bool> input) {
        bool tmp = false;
        for (int i = 0; i < hands.Count; i++) {
            var hand = hands[i];
            if (hand == null) continue;
            if (!hand.isValid) {
                to_refresh = true;
                continue;
            }
            hand.TryGetFeatureValue (input, out tmp);
            if (tmp) {
                handInUse = i;
                break;
            }
        }
        return tmp;
    }

    public bool PlayerControlShoot () {
        if (GetButtonDown (CommonUsages.triggerButton)) {
            UpdateObjects ();
            Game.instance.Eject (PlayerControl.cell, handObj.forward);
            return true;
        }
        return false;
    }

    public bool GetRestart () {
        return GetButtonDown (CommonUsages.primary2DAxisClick);
    }

    public bool GetExit () {
        // var tmp=GetButtonDown (CommonUsages.gripButton);
        // Debug.Log(tmp);
        return GetButtonDown (CommonUsages.gripButton);
    }

#endif
}