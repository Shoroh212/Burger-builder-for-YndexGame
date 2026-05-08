using UnityEngine;
using DG.Tweening;

public class Trampoline : MonoBehaviour
{
    [SerializeField] private AnimationCurve _jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float _jumpHeight = 5f;
    [SerializeField] private float _duration = 1f;

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null || player.HasTriggered) return;

        player.HasTriggered = true;
        player.CanMove = false;

        Vector3 startPos = player.transform.position;
        Vector3 endPos = startPos + Vector3.up * _jumpHeight;

        // DOTween Sequence с кастомной кривой
        Sequence jumpSequence = DOTween.Sequence();
        jumpSequence.Append(DOTween.To(
            () => 0f,
            t =>
            {
                float yOffset = _jumpCurve.Evaluate(t) * _jumpHeight;
                Vector3 pos = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
                player.transform.position = pos;
            },
            1f, // t goes from 0 to 1
            _duration
        ));

        jumpSequence.OnComplete(() =>
        {
            player.CanMove = true;
            player.HasTriggered = false;
        });
    }
}
