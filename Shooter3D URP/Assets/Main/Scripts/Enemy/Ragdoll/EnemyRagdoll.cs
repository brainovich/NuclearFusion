using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyRagdoll : MonoBehaviour
{
    private List<Rigidbody> _rigidbodies;
    [SerializeField] private Transform _hipsBone;
    private Transform _parent;

    public bool IsFrontUp => Vector3.Dot(_hipsBone.up, Vector3.up) > 0;

    public void Initialize()
    {
        _rigidbodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());
        _parent = GetComponentsInParent<Transform>()[0];
        Disable();
    }

    public void Hit(Vector3 force, Vector3 hitPosition)
    {
        Rigidbody injuredRigidbody = _rigidbodies.OrderBy(rigidbody => Vector3.Distance(rigidbody.position, hitPosition)).First();
        injuredRigidbody.AddForceAtPosition(force, hitPosition, ForceMode.Impulse);
    }

    public void Enable()
    {
        foreach (Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = false;
        }
    }
    public void Disable()
    {
        foreach (Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = true;
        }
    }

    //ADJUST POSITION TO STAND UP

    public void AdjustPositionAndRotation()
    {
        AdjustParentPositionToHipsBone();
        AdjustParentRotationToHipsBone();
    }

    private void AdjustParentPositionToHipsBone()
    {
        Vector3 initHipsPosition = _hipsBone.position;
        _parent.position = initHipsPosition;

        AdjustParentPositionRelativeGround();

        _hipsBone.position = initHipsPosition;
    }

    private void AdjustParentPositionRelativeGround()
    {
        if (Physics.Raycast(_parent.position, Vector3.down, out RaycastHit hit, 5, 1 << LayerMask.NameToLayer("Default")))
        {
            _parent.position = new Vector3(_parent.position.x, hit.point.y, _parent.position.z);
        }
    }

    private void AdjustParentRotationToHipsBone()
    {
        Vector3 initHipsPosition = _hipsBone.position;
        Quaternion initHipsRotation = _hipsBone.rotation;

        Vector3 directionForRotate = _hipsBone.up;
        if (IsFrontUp)
        {
            directionForRotate *= -1;
        }
        directionForRotate.y = 0;

        Quaternion correctionRotation = Quaternion.FromToRotation(_parent.forward, directionForRotate.normalized);
        _parent.rotation *= correctionRotation;

        _hipsBone.position = initHipsPosition;
        _hipsBone.rotation = initHipsRotation;
    }
}
