using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.
        [SerializeField] private float m_AirAccel;
        [SerializeField] private AnimationCurve m_SlideSpeedBonusCurve;
        [SerializeField] private float m_SlideDuration;
        [SerializeField] private float m_StandUpDuration;
        [SerializeField] private float m_CanInterruptPercent;
        [SerializeField] private float m_MeteorVel;
        [SerializeField] private float m_MaxSlideSpeedBonusAmt;
        [SerializeField] private float m_WallrunDuration;

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;
        private bool[] m_GroundedHistory;
        private int m_JumpCounter;
        private bool m_Crouched;
        private bool m_Sliding;
        private bool m_MidStandUp;
        private float m_SlideSpeedBonus;
        private float m_StandUpPercent;
        private bool m_CanInterruptSlide;
        private bool m_IsSlideLocked;
        private bool m_Spiking;
        private bool m_IsWallrunning;

        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);
            m_GroundedHistory = new bool[] { true, true, true };
            m_JumpCounter = 1;
            m_Crouched = false;
            m_Sliding = false;
            m_MidStandUp = false;
            m_SlideSpeedBonus = 0f;
            m_StandUpPercent = (m_SlideDuration - m_StandUpDuration) / m_SlideDuration;
            m_CanInterruptSlide = false;
            m_IsWallrunning = false;
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();

            // update three-frame grounding history
            m_GroundedHistory[2] = m_GroundedHistory[1];
            m_GroundedHistory[1] = m_GroundedHistory[0];
            m_GroundedHistory[0] = m_CharacterController.isGrounded;

            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump && (IsFirmlyGrounded() || m_Jumping))
            {
                if(m_Sliding)
                {
                    if(m_CanInterruptSlide)
                    {
                        m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
                    }
                }
                else if (!m_Spiking)
                {
                    m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
                }
                
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            if (m_CharacterController.isGrounded)
            {
                m_JumpCounter = 1;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;

            if (CrossPlatformInputManager.GetButtonDown("Slide"))
            {
                if(m_CharacterController.isGrounded)
                {
                    if (!m_Sliding && !m_Jump)
                    {
                        StartCoroutine("Slide");
                    }
                }
                else
                {
                    if(!m_Spiking && !m_Jump)
                    {
                        StartCoroutine("Spike");
                    }
                }
                
            }
        }

        private IEnumerator Spike()
        {
            m_Spiking = true;
            m_IsSlideLocked = true;

            transform.localScale = new Vector3(1.0f, 0.5f, 1.0f);

            while(!m_CharacterController.isGrounded)
            {
                yield return null;
            }

            StartCoroutine("Slide");
            m_Spiking = false;
        }

        private IEnumerator Slide()
        {
            m_Sliding = true;
            m_IsSlideLocked = true;
            m_CanInterruptSlide = false;
            bool standUpHasRun = false;

            transform.localScale = new Vector3(1.0f, 0.5f, 1.0f);
            
            for (float t = 0; t <= 1; t += Time.fixedDeltaTime / m_SlideDuration)
            {
                m_SlideSpeedBonus = m_SlideSpeedBonusCurve.Evaluate(t) * m_MaxSlideSpeedBonusAmt;

                if (t > m_CanInterruptPercent)
                {
                    m_CanInterruptSlide = true;
                }

                if (!standUpHasRun && (t > m_StandUpPercent || (m_CanInterruptSlide && m_Jump)) && !m_MidStandUp)
                {
                    standUpHasRun = true;
                    m_IsSlideLocked = false;
                    StartCoroutine("StandUp");
                }
                yield return null;
            }
            m_Sliding = false;
            m_CanInterruptSlide = false;
        }

        private IEnumerator StandUp()
        {
            m_MidStandUp = true;
            Vector3 standDeltaVec = new Vector3(1.0f, 0.5f, 1.0f);

            for (float t = 0; t <= 1; t += Time.fixedDeltaTime / m_StandUpDuration)
            {
                standDeltaVec.y = Mathf.Lerp(0.5f, 1.0f, t);
                transform.localScale = standDeltaVec;
                yield return null;
            }

            transform.localScale = Vector3.one;
            m_MidStandUp = false;
        }

        private bool IsFirmlyGrounded()
        {
            return m_GroundedHistory[0] || m_GroundedHistory[1] || m_GroundedHistory[2];
        }

        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);

            speed += m_SlideSpeedBonus;

            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
            desiredMove.x *= speed;
            desiredMove.z *= speed;


            if (m_CharacterController.isGrounded)
            {
                if (!m_IsSlideLocked)
                {
                    m_MoveDir.x = desiredMove.x;
                    m_MoveDir.z = desiredMove.z;
                }
                else
                {
                    m_MoveDir.x = desiredMove.x / 2.0f;
                }
            }
            else
            {
                Vector3 rawMove = new Vector3();
                rawMove.x = m_MoveDir.x + m_AirAccel * (desiredMove.x - m_MoveDir.x);
                rawMove.z = m_MoveDir.z + m_AirAccel * (desiredMove.z - m_MoveDir.z);
                if (desiredMove.x > m_MoveDir.x)
                {
                    m_MoveDir.x = Mathf.Min(rawMove.x, desiredMove.x);
                }
                else
                {
                    m_MoveDir.x = Mathf.Max(rawMove.x, desiredMove.x);
                }

                if (desiredMove.z > m_MoveDir.z)
                {
                    m_MoveDir.z = Mathf.Min(rawMove.z, desiredMove.z);
                }
                else
                {
                    m_MoveDir.z = Mathf.Max(rawMove.z, desiredMove.z);
                }
            }

            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;
            }
            else if (m_Spiking)
            {
                m_MoveDir.y = -m_MeteorVel;
            }
            else
            {
                m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
            }

            if (m_Jump)
            {
                m_Jump = false;
                if (m_Jumping)
                {
                    switch (m_JumpCounter)
                    {
                        case 2:
                            m_MoveDir.y = m_JumpSpeed;
                            m_MoveDir.x = desiredMove.x;
                            m_MoveDir.z = desiredMove.z;
                            m_JumpCounter++;
                            PlayJumpSound();
                            break;
                        case 3:
                            m_MoveDir.y = m_JumpSpeed / 3.0f;
                            m_MoveDir.x = desiredMove.x;
                            m_MoveDir.z = desiredMove.z;
                            m_JumpCounter++;
                            PlayJumpSound();
                            break;
                    }
                }
                else
                {
                    m_MoveDir.y = m_JumpSpeed;
                    m_JumpCounter++;
                    PlayJumpSound();
                    m_Jumping = true;
                }
            }

            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            m_MouseLook.UpdateCursorLock();
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            m_MouseLook.LookRotation (transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (m_CollisionFlags == CollisionFlags.Sides)
            {
                GameObject wall = hit.gameObject;
                
                if(CanStartWallrun(hit))
                {
                    StartCoroutine("Wallrun");
                }
            }

            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }

            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }

        private bool CanStartWallrun(ControllerColliderHit hit)
        {
            if (m_IsWallrunning)
            {
                return false;
            }

            if (hit.gameObject.GetComponent<Wallrunnable>() == null)
            {
                return false;
            }

            return true;


        }

        private IEnumerator Wallrun()
        {
            for (float t = 0; t < m_WallrunDuration; t += Time.fixedDeltaTime)
            {
                yield return null;
            }
        }
    }
}
