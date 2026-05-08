using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FortuneWheelAward : MonoBehaviour
{
    [SerializeField] private AwardType _awardType;

    private enum AwardType
    {
        Money10,
        Money50,
        Money100,
        Money150,
        Skin,
        Respin,
        IncomeLevel,
        LuckLevel,
    }

    public void GetAward()
    {
        switch(_awardType)
        {
            case AwardType.Money10:
                GetMoney(10);
                break;

            case AwardType.Money50:
                GetMoney(50);
                break;

            case AwardType.Money100:
                GetMoney(100);
                break;

            case AwardType.Money150:
                GetMoney(150);
                break;

            case AwardType.Skin:
                GetSkin();
                break;

            case AwardType.Respin:
                GetRespin();
                break;

            case AwardType.IncomeLevel:
                GetIncomeLevel();
                break;

            case AwardType.LuckLevel:
                GetLuckLevel();
                break;
        }
    }

    private void GetMoney(int count)
    {
        GameManager.Instance.UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) + count);
    }

    private void GetSkin()
    {
        int[] allow = new int[] { 1, 2, 3, 4, 5, 12, 11, 10 };

        // Фильтруем только те SkinID, которых ещё нет у игрока
        var notPurchased = allow
            .Where(id => !GameManager.Instance.PurchasedSkins.Contains(id))
            .ToList();

        if (notPurchased.Count == 0)
        {
            Debug.Log("Все скины уже куплены.");
            return;
        }

        // Выбираем случайный
        int randomIndex = Random.Range(0, notPurchased.Count);
        int selectedSkinID = notPurchased[randomIndex];

        // Добавляем в список купленных
        GameManager.Instance.PurchasedSkins.Add(selectedSkinID);
        Debug.Log($"Добавлен скин с ID: {selectedSkinID}");
    }

    private void GetRespin()
    {
        PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) + 1);

        //GameManager.Instance.UIController.UpdateFortuneWheelText(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey).ToString());
    }

    private void GetIncomeLevel()
    {
        GameManager.Instance.UIController.EnableIncomeMode(true);
    }

    private void GetLuckLevel()
    {
        GameManager.Instance.UIController.EnableLuckMode(true);
    }
}
