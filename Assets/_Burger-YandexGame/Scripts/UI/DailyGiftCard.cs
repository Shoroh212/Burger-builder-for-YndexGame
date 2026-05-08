using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class DailyGiftCard : MonoBehaviour
{
    [SerializeField] private AwardType _awardType;

    [Header("Reference")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _iconAward;

    [Header("Background")]
    [SerializeField] private Sprite _readyBackground;
    [SerializeField] private Sprite _defaultBackground;

    [Header("Icon")]
    [SerializeField] private Sprite _iconCoin;
    [SerializeField] private Sprite _iconFortune;
    [SerializeField] private Sprite _iconSkin;
    [SerializeField] private Sprite _iconIncome;
    [SerializeField] private Sprite _iconLuck;

    public bool IsClaimed { get; private set; } = false;

    private enum AwardType
    {
        Money100,
        Money200,
        Skin,
        Respin,
        IncomeLevel,
        LuckLevel,

        Money300,
        Respin3,
        Skin3,
    }

    private void OnValidate()
    {
        if (_background != null)
            _background.sprite = _defaultBackground;

        switch (_awardType)
        {
            case AwardType.Money100:
                _iconAward.sprite = _iconCoin;
                break;
            case AwardType.Money200:
                _iconAward.sprite = _iconCoin;
                break;
            case AwardType.Money300:
                _iconAward.sprite = _iconCoin;
                break;

            case AwardType.Skin:
                _iconAward.sprite = _iconSkin;
                break;
            case AwardType.Skin3:
                _iconAward.sprite = _iconSkin;
                break;

            case AwardType.Respin:
                _iconAward.sprite = _iconFortune;
                break;
            case AwardType.Respin3:
                _iconAward.sprite = _iconFortune;
                break;

            case AwardType.IncomeLevel:
                _iconAward.sprite = _iconIncome;
                break;
            case AwardType.LuckLevel:
                _iconAward.sprite = _iconLuck;
                break;
        }
    }

    public void ClaimReward()
    {
        if (IsClaimed)
            return;

        switch (_awardType)
        {
            case AwardType.Money100:
                GetMoney(100);
                break;
            case AwardType.Money200:
                GetMoney(200);
                break;
            case AwardType.Money300:
                GetMoney(200);
                break;

            case AwardType.Skin:
                GetSkin();
                break;

            case AwardType.Skin3:
                GetSkin();
                GetSkin();
                GetSkin();
                break;

            case AwardType.Respin:
                GetFortuneSpin();
                break;

            case AwardType.Respin3:
                GetFortuneSpin();
                GetFortuneSpin();
                GetFortuneSpin();
                break;

            case AwardType.IncomeLevel:
                GetIncomeLevel();
                break;
            case AwardType.LuckLevel:
                GetLuckLevel();
                break;
        }

        MarkupClaimed();
    }

    public void MarkupClaimed()
    {
        IsClaimed = true;
        _background.sprite = _defaultBackground;
        _background.color = new Color(0.4f, 0.4f, 0.4f, 1f);

        if (_iconAward != null)
            _iconAward.color = new Color(0.2f, 0.2f, 0.2f, 1f);

    }

    private void GetMoney(int count)
    {
        GameManager.Instance.UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) + count);
    }

    private void GetSkin()
    {
        int[] allow = new int[] { 1, 2, 3, 4, 5, 12, 11, 10 };

        // ��������� ������ �� SkinID, ������� ��� ��� � ������
        var notPurchased = allow
            .Where(id => !GameManager.Instance.PurchasedSkins.Contains(id))
            .ToList();

        if (notPurchased.Count == 0)
        {
            Debug.Log("��� ����� ��� �������.");
            return;
        }

        // �������� ���������
        int randomIndex = Random.Range(0, notPurchased.Count);
        int selectedSkinID = notPurchased[randomIndex];

        // ��������� � ������ ���������
        GameManager.Instance.PurchasedSkins.Add(selectedSkinID);
        Debug.Log($"�������� ���� � ID: {notPurchased[randomIndex]}");
    }

    private void GetFortuneSpin()
    {
        PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) + 1);

        FortuneWheelSpinManager fortuneWheelSpinManager = FindAnyObjectByType<FortuneWheelSpinManager>();

        fortuneWheelSpinManager.SetReadyFortune();
    }

    private void GetIncomeLevel()
    {
        GameManager.Instance.UIController.EnableIncomeMode(true);
    }

    private void GetLuckLevel()
    {
        GameManager.Instance.UIController.EnableLuckMode(true);
    }

    public void MarkupReady()
    {
        if (_background != null)
            _background.sprite = _readyBackground;

        if (_iconAward != null)
            _iconAward.color = Color.white;
    }

    public void MarkupDefault()
    {
        if (_background != null)
            _background.sprite = _defaultBackground;

        if (_iconAward != null)
            _iconAward.color = Color.white;
    }

}