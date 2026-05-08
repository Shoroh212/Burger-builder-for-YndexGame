using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector3 _stepVector;

    private Vector3 _currentStepVector;

    private void Start()
    {
        _currentStepVector = transform.localPosition;
    }

    public void StepBack()
    {
        _currentStepVector += _stepVector;

        transform.DOLocalMove(_currentStepVector, 0.1f);
    }

    public void StepForward()
    {
        _currentStepVector -= _stepVector;

        transform.DOLocalMove(_currentStepVector, 0.1f);
    }
}