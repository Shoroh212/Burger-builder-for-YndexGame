using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpPointer : MonoBehaviour
{
    [Header("Help Pointer Settings")]
    [SerializeField] private RectTransform _helpPointer;
    [SerializeField] private RectTransform _pointerPos1;
    [SerializeField] private RectTransform _pointerPos2;
    [SerializeField] private float _pointerSpeed;

    [SerializeField] private GameObject[] _helpPanel;

    private Tween _moveTween;

    private void Start()
    {
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            foreach (var item in _helpPanel)
            {
                item.SetActive(true);
            }

            Initialize();
        }
    }

    private void Initialize()
    {
        if(_helpPointer == null || _pointerPos1 == null || _pointerPos2 == null)
        {
            return;
        }

        _helpPointer.anchoredPosition = _pointerPos1.anchoredPosition;

        _moveTween = _helpPointer.DOAnchorPos(_pointerPos2.anchoredPosition, _pointerSpeed)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopAnimation()
    {
        if(_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Kill();
        }
    }
}
