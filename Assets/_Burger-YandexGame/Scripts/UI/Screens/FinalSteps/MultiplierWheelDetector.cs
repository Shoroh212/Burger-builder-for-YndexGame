using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class MultiplierWheelDetector : MonoBehaviour
{
    [SerializeField] private Transform _arrowPivot;
    [SerializeField] private UIFinal _uiFinal;

    [SerializeField] private float _speed = 1f;
    [SerializeField] private Vector3 _startPoint;
    [SerializeField] private Vector3 _endPoint;

    private Tween _moveTween;
    public int Multiplier { get; set; }

    public void Start()
    {
        _arrowPivot.eulerAngles = _startPoint;

        _moveTween = _arrowPivot.DORotate(_endPoint, _speed).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    public void GetAward(Button button)
    {
        _moveTween.Kill();
        PlayerPrefs.SetInt(SaveData.MultiplierMoneyKey, GameManager.Instance.CalculateMoney().Total * Multiplier);
        button.interactable = false;
        YandexGame.RewVideoShow(SaveData.MultiplierWheelReward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out MultiplierWheelX multiplierWheelX))
        {
            Multiplier = multiplierWheelX.Multiplier;
            string value = (GameManager.Instance.CalculateMoney().Total * Multiplier).ToString();
            _uiFinal.SetMultiplierText(value);
        }
    }
}
