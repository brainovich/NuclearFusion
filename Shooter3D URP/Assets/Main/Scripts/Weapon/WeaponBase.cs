using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class  WeaponBase : MonoBehaviour
{
    [Header("Base")]
    public int WeaponType;
    [SerializeField] private int _damage;
    [SerializeField] private float _impactForce;
    [SerializeField] private float _shootingRange;
    [SerializeField] private float _shootingDelay;
    private bool _canShoot  = true;

    [Header("Ammo")]

    [SerializeField] private int _ammoQtyTotalMax;
    [SerializeField] private int _ammoQtyClipMax;
    [SerializeField] private float _reloadTime;
    public int _ammoQtyClipCurrent { get; private set; }
    public int _ammoQtyTotalCurrent { get; private set; }

    [Header("Recoil")]
    [SerializeField] private float _recoilForce;
    [SerializeField] private float _returnSpeed;
    private Vector3 _targetPosition;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Quaternion _targetRotation;
    private float _camTargetPosition;
    private float _camTargetRotation;
    private Camera _fpsCam;

    [Header("Effects")]
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private AudioClip _soundShot;
    [SerializeField] private AudioClip _soundReload;


    [Header("Links")]
    private AudioSource _sound;
    private PlayerController2 _playerController;
    private bool _telekinesisEnabled;

    [Header("Keybinds")]
    private KeyCode reloadKey = KeyCode.R;

    private void Start()
    {
        _sound = GetComponent<AudioSource>();
        _playerController = GetComponentInParent<PlayerController2>();
        _fpsCam = GetComponentInParent<Camera>();
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        gameObject.layer = LayerMask.NameToLayer("Weapon");
        GetComponentInChildren<MeshRenderer>().gameObject.layer = LayerMask.NameToLayer("Weapon");

        _ammoQtyClipCurrent = _ammoQtyClipMax;
        _ammoQtyTotalCurrent = _ammoQtyClipMax * 2;

        ResetPosition();
    }

    private void ResetPosition()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
    }

    private void Update()
    {
        _telekinesisEnabled = GameManager.instance.playerController.TelekinesisEnabled;

        if (Input.GetKeyDown(reloadKey))
        {
            StartCoroutine(Reload());
        }
        if (_telekinesisEnabled)
        {
            _canShoot = false;
            StartCoroutine(ResetShoot());
        }
        if (Input.GetButton("Fire1") && _canShoot)
        {
            if(_ammoQtyClipCurrent > 0)
            {
                _canShoot = false;
                Shoot();
                if(_ammoQtyClipCurrent == 0)
                {
                    StartCoroutine(Reload());
                    return;
                }
                StartCoroutine(ResetShoot());
            }
        }
    }

    private void Shoot()
    {
        RaycastHit hit;

        _ammoQtyClipCurrent--;

        if (Physics.Raycast(_fpsCam.transform.position, _fpsCam.transform.forward, out hit, _shootingRange))
        {
            IDamagable currentDamagable = hit.transform.GetComponent<IDamagable>();
            if(currentDamagable == null)
            {
                currentDamagable = hit.transform.GetComponentInParent<IDamagable>();
            }

            if (currentDamagable != null)
            {
                Vector3 forceDirection = (hit.point - _fpsCam.transform.position).normalized;
                forceDirection.y = 0;

                currentDamagable.TakeDamage(_damage, forceDirection*_impactForce, hit.point);
            }
            else if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * _impactForce);
            }

            GameManager.instance.weaponController.CreateImpactEffect(hit);
            GameManager.instance.weaponController.UpdateAmmoUI(_ammoQtyClipCurrent, _ammoQtyTotalCurrent);
        }

        //SHOT EFFECTS
        _muzzleFlash.Play();
        _sound.clip = _soundShot;
        _sound.Play();

        ExecuteRecoil();
    }

    private IEnumerator Reload()
    {
        if(_ammoQtyClipCurrent == 0)
        {
            yield return new WaitForSeconds(_shootingDelay);
        }
        if(_ammoQtyClipCurrent != _ammoQtyClipMax && _ammoQtyTotalCurrent > 0)
        {
            _canShoot = false;

            _sound.clip = _soundReload;
            _sound.Play();

            _ammoQtyTotalCurrent += _ammoQtyClipCurrent;
            int ammoCheck = _ammoQtyTotalCurrent - _ammoQtyClipMax;
            if (ammoCheck < 0)
            {
                _ammoQtyClipCurrent = _ammoQtyTotalCurrent;
                _ammoQtyTotalCurrent = 0;
            }
            else
            {
                _ammoQtyTotalCurrent = ammoCheck;
                _ammoQtyClipCurrent = _ammoQtyClipMax;
            }

            StartCoroutine(ReloadAnimation());
        } 
    }

    IEnumerator ReloadAnimation()
    {
        float degrees = 720;

        float deg = 0;
        while (deg < 360)
        {
            transform.Rotate(Vector3.right, degrees * Time.deltaTime);
            deg += degrees * Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _startPosition;
        transform.localRotation = _startRotation;

        _canShoot = true;
    }

    private IEnumerator ResetShoot()
    {
        if (_telekinesisEnabled)
        {
            while (GameManager.instance.playerController.TelekinesisEnabled)
            {
                yield return null;
            }
            yield return new WaitForSeconds(_shootingDelay * 3);
        }
        else
        {
            yield return new WaitForSeconds(_shootingDelay);
        }

        _canShoot = true;
    }

    private void PositionRecoil()
    {
        //сделать чтобы автомат отходил назад при выстреле
        _targetPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, Mathf.Clamp(transform.localPosition.z, transform.localPosition.z, -_recoilForce));
        _targetRotation = Quaternion.Euler(-7, 0, 0);

        StartCoroutine(PositionRecoilCooldown());
    }

    private void ExecuteRecoil()
    {
        PositionRecoil();

        _camTargetPosition -= Random.Range(-1f, 2f);
        _camTargetRotation -= Random.Range(-1f, 2f);

        CameraRecoil();
    }

    private IEnumerator PositionRecoilCooldown()
    {
        float t = 0;
        float animationDuration = .3f;

        while(t<1)
        {
            transform.localPosition = Vector3.Lerp(_targetPosition, _startPosition, t);
            transform.localRotation = Quaternion.Lerp(_targetRotation, _startRotation, t * t);
            t += Time.deltaTime / animationDuration;
            yield return null;
        }

        transform.localPosition = _startPosition;
        transform.localRotation = _startRotation;
    }

    private void CameraRecoil()
    {
        _playerController.RotationX += _camTargetPosition;
        _playerController.transform.rotation *= Quaternion.Euler(0, _playerController.RotationY + _camTargetRotation, 0);

        _camTargetRotation =0;
        _camTargetPosition = 0;
    }

    public void CanShoot(bool canShoot)
    {
        _canShoot = canShoot;
    }

    public void ReplenishAmmo(int ammoQty)
    {
        _ammoQtyTotalCurrent += ammoQty;
    }
}
