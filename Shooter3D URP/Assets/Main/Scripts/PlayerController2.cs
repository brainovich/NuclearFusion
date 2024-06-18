using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController2 : MonoBehaviour
{
    [Header("States")]
    [HideInInspector] public bool _canMove = true;
    private bool _toSprint;
    private bool _toJump;

    [Header("Controlls")]
    private KeyCode _sprintKey = KeyCode.LeftShift;
    private KeyCode _jumpKey = KeyCode.Space;

    [Header("Movement parameters")]
    private float _speedInUse;
    private float _speedWalk = 5f;
    private float _speedRun = 8f;
    private float _jumpForce = 8;
    private Vector3 _moveDirection;
    private Vector2 _currentInput;
    private CharacterController _characterController;

    [Header("Physics")]
    private float _gravity = 20f;
    private float _pushForce = 2f;
    private Rigidbody _rb;
    private Collider _playerCollider;

    [Header("Camera parameters")]
    public Camera PlayerCamera;
    [HideInInspector] public float RotationX = 0;
    [HideInInspector] public float RotationY = 0;
    private float _lookSpeed = 2f;
    private float _lookLimitVertical = 80f;
    private Ray _centralRay;

    [Header("Telekenesis parameters")]
    [SerializeField] private GameObject _telekinesisParent;
    [HideInInspector] public bool TelekinesisEnabled { get; private set; } = false;
    private GameObject _levitateObject = null;
    private float _telekinesisMovingForce = 600;
    private float _telekinesisShootingForce = 300;
    private float _shootingRange = 5;
    private float _distanceToTarget;

    [Header("Interaction parameters")]
    private float _interactionDistance = 10;


    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerCollider = GetComponent<CapsuleCollider>();
        _rb = GetComponent<Rigidbody>();

        _playerCollider.enabled = false;
        _rb.isKinematic = true;

        GameManager.instance.uiController.TurnOffCursor();
    }

    private void Update()
    {
        if (_canMove)
        {
            StatesCheck();

            AcceptMovementInput();
            AcceptMouseInput();
            AcceptJumpInput();

            if (Input.GetButtonDown("Fire2"))
            {
                StartCoroutine(HandleTelekinesis());
            }

            _centralRay = PlayerCamera.ScreenPointToRay(new Vector3(PlayerCamera.pixelWidth / 2 - 1, PlayerCamera.pixelHeight / 2 - 1, 0));

            if (Input.GetKey(KeyCode.E))
            {
                RaycastHit interactionHit;
                Physics.Raycast(_centralRay.origin, _centralRay.direction, out interactionHit, _interactionDistance);


                if (interactionHit.transform.gameObject != null && interactionHit.transform.gameObject.CompareTag("Weapon"))
                {
                    GameManager.instance.weaponController.PickUpWeapon(interactionHit);
                }
                if (interactionHit.transform.gameObject != null && interactionHit.transform.TryGetComponent<IInteractable>(out IInteractable interactableScript))
                {
                    interactableScript.OnInteraction();
                }

            }

            ApplyMovement();
        }
    }

    private void StatesCheck()
    {
        _toSprint = Input.GetKey(_sprintKey) && _characterController.isGrounded;
        _toJump = Input.GetKey(_jumpKey) && _characterController.isGrounded;
    }

    private void AcceptMovementInput()
    {
        if (_toSprint)
        {
            _speedInUse = _speedRun;
        }
        else if(_characterController.isGrounded)
        {
            _speedInUse = _speedWalk;
        }
        _currentInput = new Vector2(Input.GetAxis("Vertical") * _speedInUse, Input.GetAxis("Horizontal") * _speedInUse);
        if (!_characterController.isGrounded)
        {
            _currentInput.x = Mathf.Clamp(_currentInput.x, -_speedInUse/2, _speedInUse);
        }
        float moveDirectionY = _moveDirection.y;
        _moveDirection = (transform.TransformDirection(Vector3.forward) * _currentInput.x) + (transform.TransformDirection(Vector3.right) * _currentInput.y);

        _moveDirection.y = moveDirectionY;
    }

    private void AcceptJumpInput()
    {
        if (_toJump)
        {
            _moveDirection.y = _jumpForce;
        }
    }

    private void AcceptMouseInput()
    {
        RotationX -= Input.GetAxis("Mouse Y") * _lookSpeed;
        RotationX = Mathf.Clamp(RotationX, -_lookLimitVertical, _lookLimitVertical);
        PlayerCamera.transform.localRotation = Quaternion.Euler(RotationX, 0, 0);
        RotationY = Input.GetAxis("Mouse X") * _lookSpeed;
        transform.rotation *= Quaternion.Euler(0, RotationY, 0);
    }

    private void ApplyMovement()
    {
        if (!_characterController.isGrounded)
        {
            _moveDirection.y -= _gravity * Time.deltaTime;
        }

        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    private IEnumerator HandleTelekinesis()
    {
        RaycastHit hit;
        Physics.Raycast(_centralRay.origin, _centralRay.direction, out hit, _shootingRange);
        if (hit.rigidbody && !_levitateObject)
        {
            float initDrag;
            _levitateObject = hit.transform.gameObject;
            _distanceToTarget = Vector3.Distance(gameObject.transform.position, _levitateObject.transform.position);
            TelekinesisEnabled = true;
            Rigidbody telekinesisRigidbody = _levitateObject.GetComponent<Rigidbody>();
            initDrag = telekinesisRigidbody.drag;
            telekinesisRigidbody.drag = 8;

            StartCoroutine(ExecuteTelekinesis(telekinesisRigidbody));

            yield return new WaitUntil(() => TelekinesisEnabled == false);

            StopCoroutine(ExecuteTelekinesis(telekinesisRigidbody));
            telekinesisRigidbody.drag = initDrag;
            TelekinesisEnabled = false;
            _levitateObject = null;
        }
    }

    private IEnumerator ExecuteTelekinesis(Rigidbody rb)
    {
        while (TelekinesisEnabled)
        {
            _telekinesisParent.transform.localPosition = new Vector3(0, 0, _distanceToTarget);
            rb.AddForce((_telekinesisParent.transform.position -
                        _levitateObject.transform.position) * (_telekinesisMovingForce / rb.mass));
            if (Vector3.Distance(_levitateObject.transform.position, _telekinesisParent.transform.position) > 2 || Input.GetButtonUp("Fire2"))
            {
                TelekinesisEnabled = false;
            }

            if (Input.GetButtonDown("Fire1"))
            {
                TelekinesisEnabled = false;
                rb.AddForce(PlayerCamera.transform.forward * _telekinesisShootingForce, ForceMode.Impulse);
            }

            yield return null;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (_canMove)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            if (body == null || body.isKinematic)
                return;

            if (hit.moveDirection.y < -0.3f)
                return;

            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            body.velocity = pushDir * _pushForce;
        }
    }

    public void PlayDeathAnimation()
    {
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        yield return new WaitForSeconds(0.2f);
        GameManager.instance.weaponController.TurnOffAllWeapons();
    }

    public void PlayImpactEffect(Vector3 direction, bool isDead)
    {
        StartCoroutine(ImpactEffect(direction, isDead));
    }

    private IEnumerator ImpactEffect (Vector3 direction, bool isDead)
    {
        _characterController.enabled = false;
        _rb.isKinematic = false;
        _playerCollider.enabled = true;
        if (isDead)
        {
            _rb.freezeRotation = false;
        }
        _rb.AddForce(direction, ForceMode.Impulse);
        yield return new WaitForSeconds(1);
        if (!isDead)
        {
            _characterController.enabled = true;
            _playerCollider.enabled = false;
            _rb.isKinematic = true;
        }
    }
}
