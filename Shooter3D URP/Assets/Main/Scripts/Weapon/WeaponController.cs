using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon info")]
    private GameObject[] _weapons;
    private int _ammoQtyTotal;
    private int _ammoQtyClip;
    private bool _canUseWeapon = true;
    public GameObject CurrentWeapon { get; private set; }
    public int ActiveWeaponType { get; private set; }

    [Header("Bullet holes pool")]
    [SerializeField] private GameObject _impactSteelHoleObj;
    [SerializeField] private GameObject _impactBloodHoleObj;
    private List<GameObject> _impactSteelHolesList;
    private List<GameObject> _impactBloodHolesList;
    private int _impactSteelHolesQty = 20;
    private int _impactBloodHolesQty = 10;


    private void Start()
    {
        _weapons = new GameObject[2];
        _impactSteelHolesList = new List<GameObject>();
        _impactBloodHolesList = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < _impactSteelHolesQty; i++)
        {
            tmp = Instantiate(_impactSteelHoleObj);
            tmp.SetActive(false);
            _impactSteelHolesList.Add(tmp);
        }
        for (int i = 0; i < _impactBloodHolesQty; i++)
        {
            tmp = Instantiate(_impactBloodHoleObj);
            tmp.SetActive(false);
            _impactBloodHolesList.Add(tmp);
        }
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Alpha1) || ActiveWeaponType == 0) && _canUseWeapon)
        {
            ActiveWeaponType = 0;
            ChangeWeapon(ActiveWeaponType);
        }        
        if ((Input.GetKeyDown(KeyCode.Alpha2) || ActiveWeaponType == 1) && _canUseWeapon)
        {
            ActiveWeaponType = 1;
            ChangeWeapon(ActiveWeaponType);
        }

        CurrentWeapon = _weapons[ActiveWeaponType];

        if (!CurrentWeapon)
        {
            return;
        }

        _ammoQtyTotal = CurrentWeapon.GetComponent<WeaponBase>()._ammoQtyTotalCurrent;
        _ammoQtyClip = CurrentWeapon.GetComponent<WeaponBase>()._ammoQtyClipCurrent;
    }

    private GameObject GetBulletHoleSteel()
    {
        for (int i = 0; i < _impactSteelHolesQty; i++)
        {
            if (!_impactSteelHolesList[i].activeInHierarchy)
            {
                return _impactSteelHolesList[i];
            }
        }
        return null;
    }
    private GameObject GetBulletHoleBlood()
    {
        for (int i = 0; i < _impactBloodHolesQty; i++)
        {
            if (!_impactBloodHolesList[i].activeInHierarchy)
            {
                return _impactBloodHolesList[i];
            }
        }
        return null;
    }

    private void ChangeWeapon(int activeWeaponType)
    {
        for(int i = 0; i < _weapons.Length; i++)
        {
            if(i != activeWeaponType && _weapons[i] != null)
            {
                _weapons[i].SetActive(false);
            }
            else if (_weapons[i] != null)
            {
                _weapons[i].SetActive(true);
            }
        }

        if (CurrentWeapon)
        {
            UpdateAmmoUI(_ammoQtyClip, _ammoQtyTotal);
        }
    }

    public void TurnOffAllWeapons()
    {
        _canUseWeapon = false;
        ChangeWeapon(5);
        CurrentWeapon = null;
    }

    public void CanShoot(bool canShoot)
    {
        if(CurrentWeapon != null)
        {
            CurrentWeapon.GetComponent<WeaponBase>().CanShoot(canShoot);
        }
    }

    public void UpdateAmmoUI(int clipQty, int totalQty)
    {
        GameManager.instance.uiController.UpdateAmmoUI(_ammoQtyClip, _ammoQtyTotal, ActiveWeaponType);
    }

    public void CreateImpactEffect(RaycastHit hit)
    {
        GameObject bulletHole;

        if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            bulletHole = GetBulletHoleBlood();
        }
        else
        {
            bulletHole = GetBulletHoleSteel();
        }
        
        if (bulletHole != null)
        {
            bulletHole.transform.position = hit.point;
            bulletHole.transform.rotation = Quaternion.LookRotation(hit.normal);
            bulletHole.transform.SetParent(hit.transform);
            bulletHole.SetActive(true);
        }
    }

    public void PickUpWeapon(RaycastHit hit)
    {
        WeaponBase weaponBase;
        hit.transform.TryGetComponent<WeaponBase>(out weaponBase);
        if (weaponBase != null)
        {
            int weaponType = weaponBase.WeaponType;
            if (_weapons[weaponType] == null)
            {
                hit.transform.SetParent(transform);
                _weapons[weaponType] = hit.transform.gameObject;
                ActiveWeaponType = weaponType;
                weaponBase.enabled = true;
                if (_weapons[weaponType].TryGetComponent<Collider>(out Collider weaponCollider))
                {
                    weaponCollider.enabled = false;
                }
                else if(_weapons[weaponType].GetComponentInChildren<Collider>())
                {
                    _weapons[weaponType].GetComponentInChildren<Collider>().enabled = false;
                }
            }
        }
    }
}
