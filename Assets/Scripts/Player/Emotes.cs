using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Emotes : NetworkBehaviour {

    static string clear = "clearEmote";

    static string hiphop = "doHipHop";
    static string hiphop2 = "doHipHop2";

    static float epsilon = 0.0001f;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (isLocalPlayer) {
            Vector3 vel = GetComponent<Rigidbody>().velocity;
            //use an epsilon for floating point inconsistensies
            if ((vel.x >= -epsilon && vel.x <= epsilon) &&
                (vel.y >= -epsilon && vel.y <= epsilon) &&
                (vel.z >= -epsilon && vel.z <= epsilon)) {
#if !UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftControl)) {
#endif
                if (Input.GetKeyDown(KeyCode.Z)) {
                    playEmote(hiphop);
                } else if (Input.GetKeyDown(KeyCode.C)) {
                    playEmote(hiphop2);
                }
#if !UNITY_EDITOR
            }
#endif
            } else {
                clearEmote();
            }
        }
    }

    public void playEmote(string trigger) {
        if (isLocalPlayer) {
            GetComponent<NetworkAnimator>().SetTrigger(trigger);
        }
    }

    public void clearEmote() {
        if (isLocalPlayer) {
            GetComponent<NetworkAnimator>().SetTrigger(clear);
        }
    }

    public void animationEnd() {
        clearEmote();
    }
}
