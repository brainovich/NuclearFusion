using UnityEngine;
public interface IPoolable
{
    public void ReturnToPool(Transform originalParent);
    public void Restore();
}
