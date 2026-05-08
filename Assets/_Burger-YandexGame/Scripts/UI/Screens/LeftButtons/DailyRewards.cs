using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class DailyRewards : MonoBehaviour
{
    [SerializeField] private bool _isTestMode = false;

    [SerializeField] private TMP_Text _dailyGiftText;
    [SerializeField] private TMP_Text _claimButtonText;
    [SerializeField] private Button _claimButton;
    [SerializeField] private Transform _giftContainer;
    [SerializeField] private GameObject _dailyGiftPanel;
    [SerializeField] private CanvasGroup _canvasGroup;

    //[SerializeField] private AudioClip _buySound;

    private DateTime _lastLoginDateTime;
    private int _maxStreak;
    private int _currentStreak;
    private double RewardIntervalHours => _isTestMode ? (10.0 / 3600.0) : 24.0; // 1 минута или 24 часа

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex >= 3)
        {
            _maxStreak = _giftContainer.childCount;
            LoadData();
            CheckDailyLogin();

            UpdateClaimButton();
            InvokeRepeating(nameof(UpdateClaimButton), 0f, 1f);
        }

        MarkClaimed();
    }

    public void OnClaimButtonClick()
    {
        if (IsRewardAvailable())
        {
            _currentStreak = PlayerPrefs.GetInt(SaveData.LastSavedStreakKey, 0);
            _currentStreak = (_currentStreak % _maxStreak) + 1;

            GiveReward(_currentStreak);
            SaveDate(DateTime.Now, _currentStreak);

            MarkClaimed();
            UpdateClaimButton();
            //GameManager.Instance.UIController.PlaySound(_buySound);
        }
        else
        {
            Debug.Log("Подарок еще недоступен");
        }
    }

    private void UpdateClaimButton()
    {
        if (_claimButtonText == null || _claimButton == null)
            return;

        TimeSpan timeLeft = GetTimeUntilNextGift();
        ColorBlock colors = _claimButton.colors;

        if (IsRewardAvailable())
        {
            _claimButtonText.text = GetLocalizedClaimText(); // ← локализованный текст
            _claimButton.interactable = true;
            _claimButton.GetComponentInChildren<TMP_Text>().color = new Color(1f, 1f, 1f, 1f); // полностью видимая
        }
        else
        {
            string time = timeLeft.ToString(@"hh\:mm\:ss");
            _claimButtonText.text = time;
            _claimButton.interactable = false;
            _dailyGiftText.text = time;
            _claimButton.GetComponentInChildren<TMP_Text>().color = new Color(1f, 1f, 1f, 0.5f); // прозрачность 0.5
        }

        _claimButton.colors = colors;
    }

    private string GetLocalizedClaimText()
    {
        switch (GameManager.Instance.GetLanguageFromYandex())
        {
            case Language.Russian:
                return "ВЗЯТЬ";
            case Language.Turkish:
                return "AL";
            case Language.English:
                return "CLAIM";
            case Language.Spanish:
                return "RECLAMAR";
            case Language.German:
                return "ERHALTEN";
            default:
                return "CLAIM";
        }
    }

    private bool IsRewardAvailable()
    {
        string lastLoginStr = PlayerPrefs.GetString(SaveData.LastSavedDateKey);
        if (!DateTime.TryParse(lastLoginStr, out DateTime lastLogin))
            return true;

        // Используем RewardIntervalHours вместо хардкода 24
        return (DateTime.Now - lastLogin).TotalHours >= RewardIntervalHours;
    }

    private void Update()
    {
        _dailyGiftText.text = GetTimeUntilNextGift().ToString(@"hh\:mm\:ss");
    }

    private void CheckDailyLogin()
    {
        string lastLoginStr = PlayerPrefs.GetString(SaveData.LastSavedDateKey);

        if (!DateTime.TryParse(lastLoginStr, out _lastLoginDateTime))
        {
            _currentStreak = 1;
        }
        else
        {
            TimeSpan difference = DateTime.Now - _lastLoginDateTime;
            // Используем RewardIntervalHours вместо 24
            if (difference.TotalHours >= RewardIntervalHours)
            {
                int previousStreak = PlayerPrefs.GetInt(SaveData.LastSavedStreakKey, 0);
                _currentStreak = (previousStreak % _maxStreak) + 1;
            }
            else
            {
                _currentStreak = PlayerPrefs.GetInt(SaveData.LastSavedStreakKey, 1);
            }
        }
    }

    private void GiveReward(int day)
    {
        DailyGiftCard[] rewards = _giftContainer.GetComponentsInChildren<DailyGiftCard>();
        int index = day - 1;
        if (index >= 0 && index < rewards.Length)
        {
            rewards[index].ClaimReward();
        }
    }

    public TimeSpan GetTimeUntilNextGift()
    {
        string lastLoginStr = PlayerPrefs.GetString(SaveData.LastSavedDateKey);
        if (!DateTime.TryParse(lastLoginStr, out DateTime lastLogin))
        {
            return TimeSpan.Zero;
        }

        // Используем RewardIntervalHours вместо 24
        DateTime nextGiftTime = lastLogin.AddHours(RewardIntervalHours);
        TimeSpan timeLeft = nextGiftTime - DateTime.Now;
        return (timeLeft.TotalSeconds > 0) ? timeLeft : TimeSpan.Zero;
    }

    private void SaveDate(DateTime dateTime, int streak)
    {
        string dateString = dateTime.ToString("O");
        PlayerPrefs.SetString(SaveData.LastSavedDateKey, dateString);
        PlayerPrefs.SetInt(SaveData.LastSavedStreakKey, streak);
        YandexGame.savesData.LastSavedDate = dateString;
        YandexGame.savesData.LastSavedStreak = streak;
        YandexGame.SaveProgress();
    }

    private void LoadData()
    {
        if(!string.IsNullOrEmpty(YandexGame.savesData.LastSavedDate))
        {
            PlayerPrefs.SetString(SaveData.LastSavedDateKey, YandexGame.savesData.LastSavedDate);
        }
        if(YandexGame.savesData.LastSavedStreak > 0)
        {
            PlayerPrefs.SetInt(SaveData.LastSavedStreakKey, YandexGame.savesData.LastSavedStreak);
        }
    }

    private void MarkClaimed()
    {
        DailyGiftCard[] rewards = _giftContainer.GetComponentsInChildren<DailyGiftCard>();
        int claimedCount = PlayerPrefs.GetInt(SaveData.LastSavedStreakKey, 0); // Только реально полученные

        for (int i = 0; i < rewards.Length; i++)
        {
            if (i < claimedCount)
            {
                rewards[i].MarkupClaimed();
            }
            else if (i == claimedCount)
            {
                rewards[i].MarkupReady(); // ← именно это получит игрок при следующем клике
            }
            else
            {
                rewards[i].MarkupDefault();
            }
        }
    }

    public void TogglePanel(bool show)
    {
        if (_dailyGiftPanel == null || _canvasGroup == null)
            return;

        if (show)
        {
            _dailyGiftPanel.SetActive(true);
            _canvasGroup.gameObject.SetActive(true);

            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.Linear);

            _dailyGiftPanel.transform.localScale = Vector3.zero;
            _dailyGiftPanel.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack);
        }
        else
        {
            _canvasGroup.DOFade(0f, 0.25f)
                .SetEase(Ease.Linear);

            _dailyGiftPanel.transform.DOScale(Vector3.zero, 0.25f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    _dailyGiftPanel.SetActive(false);
                    _canvasGroup.gameObject.SetActive(false);
                });
        }
    }
}