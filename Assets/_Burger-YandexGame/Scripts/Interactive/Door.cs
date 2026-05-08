using UnityEngine;
using DG.Tweening;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform _door;
    [SerializeField] private Transform _button;
    [SerializeField] private float _doorMoveDistance = 3f;
    [SerializeField] private float _doorMoveDuration = 1f;

    private bool _isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isActivated)
            return;

        if (other.TryGetComponent(out Player player)) 
        {
            _isActivated = true;
            MoveDoorDown();
        }
    }

    private void MoveDoorDown()
    {
        if (_door != null)
        {
            _door.DOMoveY(_door.position.y - _doorMoveDistance, _doorMoveDuration);

        }
    }
}
