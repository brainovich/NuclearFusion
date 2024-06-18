using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class EnemyVisualFX : MonoBehaviour
{
    [Header("Stand Up Properties")]
    private RigAdjusterForAnimation _rigAdjusterForBackStandingUpAnimation;
    private RigAdjusterForAnimation _rigAdjusterForFrontStandingUpAnimation;
    private Action _standingUpCallback;
    private const string BackStandClipName = "StandUpBack";
    private const string FrontStandClipName = "StandUpFront";

    //Animation names
    [HideInInspector] public string speedFloatKey { get; private set; } = "speed";
    [HideInInspector] public string detectedTriggerKey { get; private set; } = "detected";
    [HideInInspector] public string getHitTriggerKey { get; private set; } = "getHit";
    [HideInInspector] public string startLookingTriggerKey { get; private set; } = "startLooking";
    [HideInInspector] public string stopLookingTriggerKey { get; private set; } = "stopLooking";

    [Header("Links")]
    [SerializeField] private ParticleSystem _deathParticleEfect;
    [HideInInspector] public Transform HipsBone { get; private set; }
    [HideInInspector] public Transform HeadBone { get; private set; }
    private EnemyMover _mover;
    private EnemyRagdoll _ragdoll;
    private EnemyEvents _events;
    private Animator _animator;
    private SkinnedMeshRenderer _mesh;
    private Collider _collider;


    // START
    public void Initialize()
    {
        if(_animator == null)
        {
            _mover = GetComponent<EnemyMover>();
            _ragdoll = GetComponent<EnemyRagdoll>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider>();
            _mesh = GetComponentInChildren<SkinnedMeshRenderer>();
            _events = GetComponentInParent<EnemyEvents>();

            _events.EnemyDied += Fall;
            _events.DamageTaken += Damaged;

            HipsBone = _animator.GetBoneTransform(HumanBodyBones.Hips);
            HeadBone = _animator.GetBoneTransform(HumanBodyBones.Head);

            AnimationClip[] currentClips = _animator.runtimeAnimatorController.animationClips;
            Transform[] bones = HipsBone.GetComponentsInChildren<Transform>();

            _rigAdjusterForBackStandingUpAnimation = new RigAdjusterForAnimation(currentClips.First(clip => clip.name == BackStandClipName), bones, this);
            _rigAdjusterForFrontStandingUpAnimation = new RigAdjusterForAnimation(currentClips.First(clip => clip.name == FrontStandClipName), bones, this);
        }

        _animator.enabled = true;
        _mesh.enabled = true;
    }

    // EVERY FRAME CHECK

    private void Update()
    {
        _animator.SetFloat(speedFloatKey, _mover.GetSpeed());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody.velocity.magnitude > 5 && collision.gameObject.CompareTag("LevitateObj"))
        {
            Vector3 hitForce = collision.relativeVelocity;
            hitForce.y = 0;
            ContactPoint contact = collision.GetContact(0);
            Vector3 hitPoint = contact.point;

            _events.OnEnemyFall();
            //_mover.Disable();
            Fall(hitForce * 10, hitPoint);

            Invoke("StandUp", 5);
        }
    }

    //FUNCTIONALITY
    private void EnableAnimator()
    {
        _animator.enabled = true;
        _animator.Rebind();
    }

    public void SetTrigger(string name)
    {
        _animator.SetTrigger(name);
    }

    public void DisableAnimator() => _animator.enabled = false;
    public void EnableCollider() => _collider.enabled = true;
    public void DisableCollider() => _collider.enabled = false;
    public void Disappear() => _mesh.enabled = false;

    //ANIMATIONS

    public void OnStateChaseContinued()
    {
        _events.OnStateChaseContinued();
    }

    public void OnAttackInstantiated()
    {
        _events.OnAttackInstantiated();
    }

    public void Damaged()
    {
        _animator.SetTrigger(getHitTriggerKey);
    }

    public void DeathParticleEffect()
    {
        if(_deathParticleEfect != null)
        {
            _deathParticleEfect.Play();
        }
    }

    //RAGDOLL ANIMATIONS

    public void Fall(Vector3 force, Vector3 hitPoint)
    {
        _animator.enabled = false;
        _collider.enabled = false;
        _ragdoll.Enable();
        _ragdoll.Hit(force, hitPoint);

    }

    public void StandUp()
    {
        _ragdoll.Disable();

        PlayStandingUp(EnableAnimator, EnableCollider);
        //_mover.Enable();
    }

    public void PlayStandingUp(Action adjustAnimationEndedCallback = null, Action animationEndedCallback = null)
    {
        _ragdoll.AdjustPositionAndRotation();
        _mover.Warp(transform.position);
        
        _standingUpCallback = animationEndedCallback;

        if (_ragdoll.IsFrontUp == false)
        {
            _animator.Play(FrontStandClipName, -1, 0);
            _rigAdjusterForFrontStandingUpAnimation.Adjust(() => CallbackForAdjustStandingUpAnimation(FrontStandClipName, adjustAnimationEndedCallback));
        }
        else
        {
            _rigAdjusterForBackStandingUpAnimation.Adjust(() => CallbackForAdjustStandingUpAnimation(BackStandClipName, adjustAnimationEndedCallback));
            _animator.Play(BackStandClipName, -1, 0);
        }
    }

    private void CallbackForAdjustStandingUpAnimation(string clipName, Action additionalCallback)
    {
        additionalCallback?.Invoke();
        _animator.Play(clipName, -1, 0f);
    }

    public void OnStandingUpAnimationEnded() 
    { 
        _standingUpCallback?.Invoke();
        _events.OnEnemyStoodUp();
    } 
}
