using UnityEngine;
using System.Collections;

public class RotationTrigger : MonoBehaviour
{
    [SerializeField] private float _speed = 180f;
    [SerializeField] private Vector3 _toRotate = new Vector3(0, 90, 0);
    [SerializeField] private GameObject _newRoadWall;
    [SerializeField] private GameObject _oldRoadWall;
    [SerializeField] private GameObject _otherTrigger;

    private bool _isRotating;
    private float _tempMouse;
    private float _tempKeyboard;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player) && !_isRotating)
        {
            _otherTrigger.SetActive(false);

            _isRotating = true;
            player.Vertical = 1;

            _tempMouse = player.SensitivityMouse;
            _tempKeyboard = player.SensitivityKeyboard;
            player.SensitivityMouse = 0;
            player.SensitivityKeyboard = 0;

            StartCoroutine(RotatePlayer(player));
        }
    }

    private IEnumerator RotatePlayer(Player player)
    {
        Rigidbody _rb = player.GetComponent<Rigidbody>();

        Quaternion target = _rb.rotation * Quaternion.Euler(_toRotate);

        while (Quaternion.Angle(_rb.rotation, target) > 0.5f)
        {
            Quaternion step = Quaternion.RotateTowards(_rb.rotation, target, _speed * Time.fixedDeltaTime);

            _rb.MoveRotation(step);
            yield return new WaitForFixedUpdate();
        }

        _rb.MoveRotation(target);
        FinishRotation(player);
    }

    private void FinishRotation(Player player)
    {
        _newRoadWall.SetActive(false);
        _oldRoadWall.SetActive(true);

        player.SensitivityMouse = _tempMouse;
        player.SensitivityKeyboard = _tempKeyboard;
        _isRotating = false;

        gameObject.SetActive(false);
    }
}
