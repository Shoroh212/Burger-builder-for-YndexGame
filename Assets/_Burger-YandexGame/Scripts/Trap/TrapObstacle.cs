using DG.Tweening;
using UnityEngine;

public class TrapObstacle : Trap
{
    void Start()
    {
        transform.DOLocalRotate(Vector3.up * 360f, rotateSpeed, RotateMode.FastBeyond360)
                 .SetRelative()
                 .SetLoops(-1, LoopType.Incremental)
                 .SetEase(Ease.Linear);
    }
}
