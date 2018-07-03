using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct DollyMovement
{
    public Transform start;
    public Transform end;

}

public class MatchMakingCameraDolly : MonoBehaviour
{
    private List<DollyMovement> dollys;

    public Camera mCamera;

    private int camDollyIndex;

    private float sceneTimer;
    private float timePerDolly = 30;





    // Use this for initialization
    void Start()
    {
        camDollyIndex = 0;
        sceneTimer = timePerDolly;
        dollys = new List<DollyMovement>();
        for (int i = 0; i < 4; i++)
        {
            DollyMovement dollyMovement = new DollyMovement();

            
            Transform startTransform = GameObject.FindGameObjectWithTag("dms" + (i + 1)).transform;
            Transform endTransform = GameObject.FindGameObjectWithTag("dme" + (i + 1)).transform;

            dollyMovement.start = startTransform;
            dollyMovement.end = endTransform;
            dollys.Add(dollyMovement);

        }

    }

    // Update is called once per frame
    void Update()
    {
        sceneTimer -= Time.deltaTime;
        if (sceneTimer < 0)
        {

            // Move to next scene
            camDollyIndex = (camDollyIndex + 1) % dollys.Count;
            sceneTimer = timePerDolly;
        }
        else
        {
            Vector3 movement = Vector3.Lerp(dollys[camDollyIndex].end.position, dollys[camDollyIndex].start.position, sceneTimer / timePerDolly);
            Quaternion rotation = Quaternion.Lerp(dollys[camDollyIndex].end.rotation, dollys[camDollyIndex].start.rotation, sceneTimer / timePerDolly);

            mCamera.transform.position = movement;
            mCamera.transform.rotation = rotation;


        }


    }
}
