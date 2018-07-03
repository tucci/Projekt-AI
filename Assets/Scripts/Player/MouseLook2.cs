using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Networking;


public class MouseLook2 : NetworkBehaviour {

	private Camera m_Camera;

    private Camera m_playerFPSCamera;
    private Camera spectatorCamera;
    private bool inRespawn;

    



	void Awake () {
		
		m_Camera = GetComponentInChildren<Camera>();
		Init(transform , m_Camera.transform);
        m_playerFPSCamera = m_Camera;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
		if (!isLocalPlayer)
		{
			return;
		}
		LookRotation (transform, m_Camera.transform);

	    
    }

	void FixedUpdate() {
		if (!isLocalPlayer)
		{
			return;
		}
		UpdateCursorLock();
	}


	public float XSensitivity = 2f;
	public float YSensitivity = 2f;
	public bool clampVerticalRotation = true;
	public float MinimumX = -90F;
	public float MaximumX = 90F;
	public bool smooth;
	public float smoothTime = 5f;
	public bool lockCursor = true;


	private Quaternion m_CharacterTargetRot;
	private Quaternion m_CameraTargetRot;
	private bool m_cursorIsLocked = true;

	public void Init(Transform character, Transform camera) {
		m_CharacterTargetRot = character.localRotation;
		m_CameraTargetRot = camera.localRotation;
		if (!isLocalPlayer)
		{
			return;
		}
	}

    public void SetRespawnCamera(Camera camera)
    {
        spectatorCamera = camera;
    }

    public void SetRotation(Vector3 rotation)
    {

        m_CharacterTargetRot = Quaternion.Euler(0, rotation.y, 0);
        m_CameraTargetRot = Quaternion.Euler(rotation.x, 0, 0);
    }

    public void LookRotation(Transform character, Transform pcamera )
    {
        if (inRespawn) return;
		float yRot = CrossPlatformInputManager.GetAxisRaw("Mouse X") * XSensitivity;
		float xRot = CrossPlatformInputManager.GetAxisRaw("Mouse Y") * YSensitivity;
        
        // Note: This is the camera wobble
        //float zRot = (0.01f *  Mathf.Sin(Time.time));

        m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
		m_CameraTargetRot *= Quaternion.Euler (-xRot, 0f, 0f);

		if(clampVerticalRotation) {
			m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);
		}

        setAnimatorValues(m_CameraTargetRot.x);

		if(smooth) {
			character.localRotation = Quaternion.Slerp (character.localRotation, m_CharacterTargetRot,
				smoothTime * Time.deltaTime);
			pcamera.localRotation = Quaternion.Slerp (pcamera.localRotation, m_CameraTargetRot,
				smoothTime * Time.deltaTime);
		} else {
			character.localRotation = m_CharacterTargetRot;
			pcamera.localRotation = m_CameraTargetRot;
		}

		UpdateCursorLock();
	}

    int RandomSign()
    {
        return Random.value < .5 ? 1 : -1;
    }

    public void SetCursorLock(bool value) {
		lockCursor = value;
		if(!lockCursor) {//we force unlock the cursor if the user disable the cursor locking helper
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	public void UpdateCursorLock() {
		//if the user set "lockCursor" we check & properly lock the cursos
		if (lockCursor)
			InternalLockUpdate();
	}

	private void InternalLockUpdate() {
		if(Input.GetKeyUp(KeyCode.K)) {
			m_cursorIsLocked = false;
		}
		else if(Input.GetMouseButtonUp(0)) {
			m_cursorIsLocked = true;
		}

		if (m_cursorIsLocked) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else if (!m_cursorIsLocked) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	Quaternion ClampRotationAroundXAxis(Quaternion q) {
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;

		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

		angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

		q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

		return q;
	}

    public void ToggleRespawnCamera(bool isInRespawnView)
    {
        if (isInRespawnView)
        {
            m_Camera = spectatorCamera;
            m_playerFPSCamera.enabled = false;
            inRespawn = true;
        }
        else
        {
            m_playerFPSCamera.enabled = true;
            m_Camera = m_playerFPSCamera;
            inRespawn = false;
        }
    }

    void setAnimatorValues(float cameraRotX) {
        //head animation for looking up/down
        GetComponent<Animator>().SetFloat("yLook", cameraRotX);

        //arms animation for looking up/down
        //make it so value goes all the way up to 1 (since angle will make it -0.9 to 0.9 or wtv)
        float maxDown = Mathf.Abs(MaximumX/100f);
        float maxUp = Mathf.Abs(MinimumX /100f);

        if(cameraRotX < 0) {
            //looking up
            GetComponent<Animator>().SetFloat("yLookArms", cameraRotX / maxUp);
        } else {
            //looking down
            GetComponent<Animator>().SetFloat("yLookArms", cameraRotX / maxDown);
        }
    }
}
