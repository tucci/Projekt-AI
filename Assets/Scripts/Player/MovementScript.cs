using UnityEngine;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityEngine.Networking;

namespace UnityStandardAssets.Characters.FirstPerson
{
	[RequireComponent (typeof (Rigidbody))]
	[RequireComponent (typeof (CapsuleCollider))]

	public class MovementScript : NetworkBehaviour {

        [SerializeField] private bool isNewPlayerModel;

        [SerializeField] private float m_Speed = 10.0f;
        [SerializeField] private float m_AirSpeed = 45.0f;
		[SerializeField] private float m_AirDrag = 2.0f;
		[SerializeField] private float m_Gravity = 30.0f;
		[SerializeField] private float m_MaxVelocityChange = 10.0f;
		[SerializeField] private float m_JumpHeight = 3f;
		[SerializeField] private float m_DoubleJumpHeight = 3f; 
		[SerializeField] private float m_DashJumpSpeed = 5.0f;
		[SerializeField] private float m_DashSidwaysSpeed = 30.0f;
		[SerializeField] private float m_DashDeadzone = 0.5f;
		[SerializeField] private float m_DashCoolDown = 1.0f;


		public float shellOffset; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
		public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )

        [SerializeField]
        private AudioSource m_JumpSound1;
        [SerializeField]
        private AudioSource m_JumpSound2;
        [SerializeField]
        private AudioSource m_LandingSound;

        [SerializeField]
        private ParticleSystem[] m_inAirParticleSystems;

        private Vector2 m_Input;
		private bool m_Grounded;
		private bool m_PreviouslyGrounded;
		private bool m_Jump;
		private bool m_Jumping;
		private bool m_CanDoubleJump;
		private bool m_Dash;
		private bool m_Dashing;
		private float m_DashingTimer;

	    

		private CapsuleCollider m_Capsule;
		private Vector3 m_GroundContactNormal;

	

		private Camera m_Camera;
		private Rigidbody m_Rigidbody;
        private Animator m_Animator;

        void Awake () {
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Rigidbody.freezeRotation = true;
			m_Rigidbody.useGravity = false;
			m_Jumping = false;
			m_Camera = GetComponentInChildren<Camera>();
			m_Capsule = GetComponent<CapsuleCollider>();
            m_Animator = GetComponent<Animator>();
        }

		void Update() {

			if (!isLocalPlayer)
			{
				m_Camera.enabled = false;
				return;
			}

			// the jump state needs to read here to make sure it is not missed
			if (!m_Jump) {
				m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
			}

			if (!m_Dash){
				m_Dash = CrossPlatformInputManager.GetButtonDown("Dash");
			}
			if (m_Dashing) {
				m_DashingTimer -= Time.deltaTime;
				if (m_DashingTimer < 0) {
					m_Dashing = false;
				}

			}

			if (!m_PreviouslyGrounded && m_Grounded) {
				m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 0, m_Rigidbody.velocity.z);
				m_Jumping = false;

			}
			if (!m_Grounded && !m_Jumping && m_PreviouslyGrounded) {
				m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 0, m_Rigidbody.velocity.z);
            }

			m_PreviouslyGrounded = m_Grounded;
		}

		void FixedUpdate () {

            if (!isLocalPlayer){
                return;
            }

            GroundCheck();
			GetInput();
            setAnimatorValues();

            if (m_Grounded) {
				m_Rigidbody.drag = 0;
				// Ground control
				// Calculate how fast we should be moving
				Vector3 targetVelocity = new Vector3(m_Input.x, 0, m_Input.y);

                
			    if (Mathf.Abs(m_Input.x) > 0.5)
			    {
                    //Debug.Log(m_Input.x);
                    //m_Camera.transform.rotation *= Quaternion.Euler()
                }
			    else
			    {
                    
                }

			    targetVelocity = transform.TransformDirection(targetVelocity);
				targetVelocity *= m_Speed;

				// Apply a force that attempts to reach our target velocity
				Vector3 velocity = m_Rigidbody.velocity;
				Vector3 velocityChange = (targetVelocity - velocity);	
				velocityChange.x = Mathf.Clamp(velocityChange.x, -m_MaxVelocityChange, m_MaxVelocityChange);
				velocityChange.z = Mathf.Clamp(velocityChange.z, -m_MaxVelocityChange, m_MaxVelocityChange);
				velocityChange.y = 0;
				m_Rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
			
				// Jump
				if (m_Jump) {
					Jump(m_JumpHeight);
					m_Jumping = true;
					m_CanDoubleJump = true;
				}

				if (!m_Jump && m_Dash && !m_Dashing) {
					if (Mathf.Abs(m_Input.x) > m_DashDeadzone || Mathf.Abs(m_Input.y) > m_DashDeadzone){
						Vector3 dashMove = Vector3.zero;
						m_Dashing = true;
						m_Jumping = true;
						m_Rigidbody.velocity = 
							  transform.forward * m_Input.y * m_DashSidwaysSpeed 
							+ transform.up * m_DashJumpSpeed 
							+ transform.right * m_Input.x * m_DashSidwaysSpeed;
						m_DashingTimer = m_DashCoolDown;
                        PlayDashSound();
					}
				}
			    

			} else {
				// Air control
				// Calculate how fast we should be moving
				Vector3 targetVelocity = new Vector3(m_Input.x, 0, m_Input.y);
				targetVelocity = transform.TransformDirection(targetVelocity);
				targetVelocity *= m_AirSpeed;
				m_Rigidbody.drag = m_AirDrag;
				m_Rigidbody.AddForce(targetVelocity, ForceMode.Force);

				// Air jumping
				if (m_Jump) {
					// Double jump
					if (m_CanDoubleJump) {
						m_CanDoubleJump = false;
						Jump(m_DoubleJumpHeight);

					} else if (!m_Jumping) {
						// Allow jumping in air, if the player just fell off a platform
						Jump(m_JumpHeight);
						m_Jumping = true;
						m_CanDoubleJump = true;
					}
				}
			}
				
			// We apply gravity manually for more tuning control
			m_Rigidbody.AddForce(new Vector3 (0, -m_Gravity * m_Rigidbody.mass, 0));

			//m_Grounded = false;
			m_Jump = false;
			m_Dash = false;
		}

	    void OnCollisionEnter(Collision collision)
	    {
            
	        if (collision.gameObject.name == "Player")
	        {
                Debug.Log("Collided with other player");
            }


	    }


	    private void GetInput() {
			m_Input = new Vector2(CrossPlatformInputManager.GetAxis("Horizontal"), CrossPlatformInputManager.GetAxis("Vertical"));
			// normalize input if it exceeds 1 in combined length:
			if (m_Input.sqrMagnitude > 1) {
				m_Input.Normalize();
			}
		}
        
        private void setAnimatorValues() {
            m_Animator.SetFloat("xVelocity", m_Input.x);
            m_Animator.SetFloat("yVelocity", m_Input.y);

            //change to in-air-movement/jump animation
            bool justGotOffGround = m_PreviouslyGrounded && !m_Grounded;
            bool justGotBackOnGround = !m_PreviouslyGrounded && m_Grounded;
            if (justGotOffGround || justGotBackOnGround) {
                GetComponent<Animator>().SetBool("isGrounded", m_Grounded);
            }
            //set air direction
            float airX = m_Input.x;
            airX = (airX + 1f) / 2f;
            GetComponent<Animator>().SetFloat("direction", airX);

            CmdBurners(justGotOffGround, justGotBackOnGround);
        }

        void Burners(bool start, bool stop) {
            bool justGotOffGround = m_PreviouslyGrounded && !m_Grounded;
            bool justGotBackOnGround = !m_PreviouslyGrounded && m_Grounded;
            //start/stop in-air particle system
            if (start) {
                for(int i = 0; i < m_inAirParticleSystems.Length; i++) {
                    m_inAirParticleSystems[i].Play();
                }
            } else if (stop) {
                for (int i = 0; i < m_inAirParticleSystems.Length; i++) {
                    m_inAirParticleSystems[i].Pause();
                    m_inAirParticleSystems[i].Clear();
                }
            }
        }

        [Command]
        void CmdBurners(bool start, bool stop) {
            RpcBurners(start, stop);
        }

        [ClientRpc]
        void RpcBurners(bool start, bool stop) {
            Burners(start, stop);
        }

        private void Jump(float jheight) {
            
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, Mathf.Sqrt(2 * jheight * m_Gravity), m_Rigidbody.velocity.z);	
			PlayJumpSound();
		}
			
		private void PlayJumpSound() {
		    if (m_Jumping)
		    {
                CmdPlayJumpSound(1);
            }
		    else
		    {
                CmdPlayJumpSound(2);
            }
		    
			
		}

	    [Command]
	    void CmdPlayJumpSound(int n)
	    {
	        RpcPlayJumpSound(n);

        }

	    [ClientRpc]
	    void RpcPlayJumpSound(int n)
	    {
            if (n == 1)
            {
                m_JumpSound1.Play();
            }
            else
            {
                m_JumpSound2.Play();
            }
        }

	    private void PlayDashSound()
	    {
            CmdPlayDashSound();
        }

	    [Command]
	    void CmdPlayDashSound()
	    {
            RpcPlayDashSound();
        }

	    [ClientRpc]
	    void RpcPlayDashSound()
	    {
            m_JumpSound1.Play();
        }
        

	    private void PlayLandSound()
	    {
            CmdPlayLandSound();
        }

	    [Command]
	    void CmdPlayLandSound()
	    {
            RpcPlayLandSound();
        }

	    [ClientRpc]
	    void RpcPlayLandSound()
	    {
            m_LandingSound.Play();
        }

	    /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
		private void GroundCheck() {
			m_PreviouslyGrounded = m_Grounded;
            m_Grounded = false;
            RaycastHit hitInfo;
            Vector3 playerCenter = transform.position;
            if (isNewPlayerModel) {
                playerCenter.y += (m_Capsule.height / 2f);
            }
            if (Physics.SphereCast(playerCenter, m_Capsule.radius * (1.0f - shellOffset), Vector3.down, out hitInfo,
            	((m_Capsule.height/2f) - m_Capsule.radius) + groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
				m_Grounded = true;
				m_GroundContactNormal = hitInfo.normal;
                
            } else {
				m_Grounded = false;
				m_GroundContactNormal = Vector3.up;
			}
			if (!m_PreviouslyGrounded && m_Grounded) {
			    if (m_Jumping)
			    {
                    PlayLandSound();
                    m_Jumping = false;
                }
			}
        }

        public Rigidbody getRigidBody()
        {
            return m_Rigidbody;
        }

	    


        
	}


}
