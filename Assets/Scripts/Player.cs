using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class Player : MonoBehaviour {

    // constatnts
    const float kPushSpeed =0.8f;
    const float kMinSpeed = 8.0f;
    const float kMaxSpeed = 40.0f;
    const float kOlliePower = 22.0f;
    const float kTimeBetweenPushes = 1.25f;

    // public 
    public SkeletonAnimation m_SkeletonAnim;
    public WheelJoint2D m_BackWheel;
    public WheelJoint2D m_FrontWheel;
    public AudioSource m_OllieAudio;
    public BoxCollider2D m_BailCollider;
    public AudioClip[] m_RandomOllieClips;
    public AudioClip[] m_RandomLandClips;
    public AudioClip m_RollAudio;
    public AudioClip[] m_RandomBailClips;

    // private
    Rigidbody2D m_RigidBody;
    Spine.Unity.Modules.SkeletonRagdoll2D ragdoll;
    bool m_ragdollEnabled = false;
    int m_NumWheelsOnGround = 0;
    bool m_IsCrouching = false;
    float m_TimeSinceLastPushed = 0.0f;

    public bool AnyWheelsOnGround()
    {
        return m_NumWheelsOnGround > 0;
    }

    public bool IsCrouching()
    {
        return m_IsCrouching;
    }

    public bool IsPlayingLandAnim()
    {
        TrackEntry trackEntry = m_SkeletonAnim.AnimationState.GetCurrent(0);

        if (trackEntry.Animation.Name == "land" && !trackEntry.IsComplete)
        {
            return true;
        }

        return false;
    }

    // Use this for initialization
    void Start ()
    {
        // m_Bone = m_SkeletonAnim.skeleton.FindBone("board");
        m_SkeletonAnim = GetComponent<SkeletonAnimation>();
        ragdoll = GetComponent<Spine.Unity.Modules.SkeletonRagdoll2D>();
        m_RigidBody = GetComponent<Rigidbody2D>();
    }

    public void PushSkateboard()
    {
        if (m_ragdollEnabled)
        {
            return;
        }

        if (m_NumWheelsOnGround == 0)
        {
            return;
        }

        PlayAnimation("push", false);

        m_TimeSinceLastPushed = Time.time;
    }

    private void Accelerate()
    {
        if (m_ragdollEnabled)
        {
            return;
        }

        if (m_NumWheelsOnGround == 0)
        {
            return;
        }

        if (m_RigidBody)
        {
            Vector2 newVel = m_RigidBody.velocity + new Vector2 (m_RigidBody.transform.right.x * kPushSpeed, 0.0f);

            if (newVel.x > kMaxSpeed)
            {
                newVel.x = kMaxSpeed;
            }

            m_RigidBody.velocity = newVel;
        }
    }

    public bool IsRagdollEnabled()
    {
        return m_ragdollEnabled;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // minimum velocity
        if (m_RigidBody.transform.up.y > 0.9f && !IsRagdollEnabled() )
        {
            if (m_RigidBody.velocity.sqrMagnitude < kMinSpeed * kMinSpeed)
            {
                Vector2 newVel = m_RigidBody.transform.right * kMinSpeed;
                m_RigidBody.velocity = newVel;
            }
        }

        if (m_NumWheelsOnGround == 0)
        {
            // can't be crouching if you're in mid air
            SetIsCrouching(false);
        }

        CheckWheelsAreOnGround();
        UpdateAnimations();
        UpdateCenterOfMass();
        UpdateBoundingBox();

        if (m_NumWheelsOnGround > 0 && !m_OllieAudio.isPlaying && m_RigidBody.velocity.x > 0.0f && !m_ragdollEnabled)
        {
            m_OllieAudio.Stop();
            m_OllieAudio.clip = m_RollAudio;
            m_OllieAudio.loop = true;
            m_OllieAudio.Play();
        
        }
        else if (m_NumWheelsOnGround > 0 && m_OllieAudio.isPlaying && !m_ragdollEnabled)
        {
            // set the speed based on the velocity of the player
            float percent = m_RigidBody.velocity.x / kMaxSpeed;

            if (percent < 0.5f)
            {
                percent = 0.5f;
            }

            m_OllieAudio.pitch = percent;
        }
        else if (m_NumWheelsOnGround == 0 || m_RigidBody.velocity.x <= 0.0f || m_ragdollEnabled)
        {
            if (m_OllieAudio.clip.name == "rolling")
            {
                m_OllieAudio.Stop();
            }
        }

        if (m_NumWheelsOnGround < 1 && !m_ragdollEnabled)
        {
            // gently stabilize upwards when falling
            RotateTowardsUp();
        }

        if (m_IsCrouching)
        {
            float timeSincePush = Time.time - m_TimeSinceLastPushed;

            if (timeSincePush > kTimeBetweenPushes)
            {
                PushSkateboard();
            }

            // always accelerate if crouching
            Accelerate();
        }
    }

    public void Bail()
    {
        if (m_ragdollEnabled)
        {
            return;
        }

        ragdoll.Apply();
        m_ragdollEnabled = true;
        ragdoll.RootRigidbody.velocity = new Vector2(m_RigidBody.velocity.x * 1.5f, 10);

        int randIndex = Random.Range(0, m_RandomBailClips.Length - 1);
        m_OllieAudio.clip = m_RandomBailClips[randIndex];
        m_OllieAudio.pitch = 1.0f;
        m_OllieAudio.loop = false;
        m_OllieAudio.Play();
    }

    public void Ollie()
    {
        if (m_ragdollEnabled)
        {
            return;
        }

        if (m_NumWheelsOnGround == 0)
        {
            return;
        }

        if (m_RigidBody)
        {
            Vector2 up = m_RigidBody.transform.up;
            Vector2 newVel = m_RigidBody.velocity;
            newVel.y += up.y * kOlliePower;
            m_RigidBody.velocity = newVel;

            // random ollie clip
            int randIndex = Random.Range(0, m_RandomOllieClips.Length - 1);
            m_OllieAudio.clip = m_RandomOllieClips[randIndex];
            m_OllieAudio.loop = false;
            m_OllieAudio.Play();

            int randTrick = Random.Range(0, 9);

            if (randTrick == 0)
            {
                PlayAnimation("impossible", false);
            }
            else if (randTrick == 1)
            {
                PlayAnimation("impossible_front", false);
            }
            else if (randTrick == 2)
            {
                PlayAnimation("impossible_kickflip", false);
            }
            else if (randTrick == 3)
            {
                PlayAnimation("impossible_front_kickflip", false);
            }
            else if (randTrick == 4)
            {
                PlayAnimation("kickflip", false);
            }
            else
            {
                PlayAnimation("ollie", false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Collectable" || collision.tag == "Skater")
        {
            return;
        }

        if (m_ragdollEnabled)
        {
            return;
        }

        Bail();
    }

    void PlayAnimation(string animationName, bool loop)
    {
        if (m_ragdollEnabled)
        {
            return;
        }

        if (!m_SkeletonAnim)
        {
            return;
        }

        m_SkeletonAnim.AnimationState.SetAnimation(0, animationName, loop);
    }

    public void Reset()
    {
        if (m_ragdollEnabled)
        {
            ragdoll.Remove();
        }
        
        Destroy(gameObject, 0.0f);
    }

    private void UpdateCenterOfMass()
    {
        // TODO: update this based on the animation
        if (!m_RigidBody)
        {
            return;
        }

        m_RigidBody.centerOfMass = new Vector2(0.09f, -0.4f);
    }

    private void UpdateAnimations()
    {
        if (m_ragdollEnabled)
        {
            return;
        }

        if (!m_SkeletonAnim)
        {
            return;
        }

        TrackEntry trackEntry = m_SkeletonAnim.AnimationState.GetCurrent(0);
        if (trackEntry.Animation.Name == "ollie" || 
            trackEntry.Animation.Name == "impossible"
            || trackEntry.Animation.Name == "impossible_front"
            || trackEntry.Animation.Name == "impossible_kickflip"
            || trackEntry.Animation.Name == "impossible_front_kickflip"
            || trackEntry.Animation.Name == "kickflip")
        {
            if (trackEntry.IsComplete)
            {
                if (m_NumWheelsOnGround > 0)
                {
                    PlayAnimation("idle1", true);
                }
                else
                {
                    PlayAnimation("drop", false);
                }
            }
        }
        else if (trackEntry.Animation.Name == "drop")
        {
            if (m_NumWheelsOnGround > 0)
            {
                PlayAnimation("land", false);

                int randIndex = Random.Range(0, m_RandomLandClips.Length - 1);
                m_OllieAudio.clip = m_RandomLandClips[randIndex];
                m_OllieAudio.loop = false;
                m_OllieAudio.Play();
            }
        }
        else if (trackEntry.Animation.Name == "land")
        {
            if (trackEntry.IsComplete)
            {
                if (m_NumWheelsOnGround > 0)
                {
                    PlayAnimation("idle1", true);
                }
            }
        }
        else if (trackEntry.Animation.Name == "push")
        {
            if (trackEntry.IsComplete)
            {
                if (m_IsCrouching)
                {
                    if (trackEntry.Animation.Name != "idle2")
                    {
                        PlayAnimation("idle2", true);
                    }
                }
                else
                {
                    if (trackEntry.Animation.Name != "idle1")
                    {
                        PlayAnimation("idle1", true);
                    }
                }
            }
        }
    }

    private void UpdateBoundingBox()
    {
        if (m_ragdollEnabled)
        {
            return;
        }

        if (!m_SkeletonAnim)
        {
            return;
        }

        if (m_SkeletonAnim.AnimationName == "ollie" ||
            m_SkeletonAnim.AnimationName == "impossible" ||
            m_SkeletonAnim.AnimationName == "impossble_front")
        {
            m_BailCollider.size = new Vector2(m_BailCollider.size.x, 3.5f);
            m_BailCollider.offset = new Vector2(m_BailCollider.offset.x, 4.0f);
        }
        else
        {
            m_BailCollider.size = new Vector2(m_BailCollider.size.x, 4.9f);
            m_BailCollider.offset = new Vector2(m_BailCollider.offset.x, 3.3f);
        }
    }

    void CheckWheelsAreOnGround()
    {
        m_NumWheelsOnGround = 0;

        Vector3 posOffset = new Vector3(0, - 0.4f, 0.0f);

        RaycastHit2D hit = Physics2D.Raycast(m_BackWheel.transform.position + posOffset, -Vector2.up, 1.4f);
        if (hit.collider != null && hit.collider.tag != "Skater")
        {
            m_NumWheelsOnGround++;
        }

        hit = Physics2D.Raycast(m_FrontWheel.transform.position + posOffset, -Vector2.up, 1.4f);
        if (hit.collider != null && hit.collider.tag != "Skater")
        {
            m_NumWheelsOnGround++;
        }
    }

    public void SetIsCrouching(bool crouching)
    {
        m_IsCrouching = crouching;
    }

    void RotateTowardsUp()
    {
        // Smoothly rotates towards target 
        var targetPoint = transform.position;
        var targetRotation = Quaternion.LookRotation(targetPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3.0f);
    }
}
