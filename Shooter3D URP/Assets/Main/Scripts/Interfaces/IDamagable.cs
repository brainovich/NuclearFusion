using UnityEngine;
public interface IDamagable 
{
    GameObject gameObject { get;}
    public void TakeDamage(int damageAmount, Vector3 force, Vector3 hitPoint);
    public void OnObjDestroy(int timeDelay);
}