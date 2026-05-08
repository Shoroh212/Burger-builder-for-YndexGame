using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class MinuteGift : MonoBehaviour
{
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private Button _getButton;
    [SerializeField] private RectTransform _minuteGiftLeftButton;
    [SerializeField] private float _maxTimeRemaining = 600f;

    [SerializeField] private float _pulseScale;
    [SerializeField] private float _pulseTime;

    [SerializeField] private int _giftMoney;

    private bool _timerIsRunning = true;
    private float _timeRemaining;
    private Tween _pulseTween;

    private const string LastGiftTimeKey = "LastGiftTime";

    private void Start()
    {
        LoadTimer();
        _getButton.onClick.AddListener(GetMinuteGift);
    }

    private void Update()
    {
        UpdateTimer(_minuteGiftLeftButton);
    }

    private void UpdateTimer(RectTransform minuteGiftLeftButton)
    {
        if (_timerIsRunning)
        {
            if (_timeRemaining > 1f)
            {
                _timeRemaining -= Time.deltaTime;
                DisplayTime(_timeRemaining);
            }
            else
            {
                _timeRemaining = 0;
                _timerIsRunning = false;
                DisplayTime(_timeRemaining);

                StartPulse(minuteGiftLeftButton);
            }
        }
    }

    private void DisplayTime(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        _timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void StartPulse(RectTransform rt)
    {
        if (_pulseTween != null && _pulseTween.IsActive())
            return;

        _pulseTween = rt.DOScale(_pulseScale, _pulseTime).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo);
    }

    private void StopPulse(RectTransform rt)
    {
        _pulseTween?.Kill();
        _pulseTween = null;
        rt.localScale = Vector3.one;
    }

    public void GetMinuteGift()
    {
        // Если таймер завершился – выдаем награду
        if (!_timerIsRunning && _timeRemaining <= 0f)
        {
            GameManager.Instance.UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) + _giftMoney);

            PlayerPrefs.SetString(LastGiftTimeKey, DateTime.Now.ToBinary().ToString());
            PlayerPrefs.Save();

            ResetTimer(_minuteGiftLeftButton);
        }
        else
        {
            Debug.Log("⏳ Подарок еще не готов!");
        }
    }

    public void ResetTimer(RectTransform minuteGiftLeftButton)
    {
        _timeRemaining = _maxTimeRemaining;
        _timerIsRunning = true;

        StopPulse(minuteGiftLeftButton);
    }

    private void LoadTimer()
    {
        if (PlayerPrefs.HasKey(LastGiftTimeKey))
        {
            long binaryTime = Convert.ToInt64(PlayerPrefs.GetString(LastGiftTimeKey));
            DateTime lastGiftTime = DateTime.FromBinary(binaryTime);

            double elapsedSeconds = (DateTime.Now - lastGiftTime).TotalSeconds;
            _timeRemaining = Mathf.Max(0f, _maxTimeRemaining - (float)elapsedSeconds);

            if (_timeRemaining <= 0)
            {
                _timerIsRunning = false;
                _timeRemaining = 0;
                StartPulse(_minuteGiftLeftButton);
            }
            else
            {
                _timerIsRunning = true;
            }
        }
        else
        {
            PlayerPrefs.SetString(LastGiftTimeKey, DateTime.Now.ToBinary().ToString());
            PlayerPrefs.Save();
            _timeRemaining = _maxTimeRemaining;
            _timerIsRunning = true;
        }

        DisplayTime(_timeRemaining);
    }
}
