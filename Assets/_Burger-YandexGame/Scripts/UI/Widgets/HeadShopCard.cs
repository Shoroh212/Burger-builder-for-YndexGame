using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class HeadShopCard : MonoBehaviour
{
    [field: SerializeField] public Button CardButton { get; private set; }
    [field: SerializeField] public TMP_Text CardButtonText { get; private set; }
    [field: SerializeField] public int SkinID { get; private set; }

    [SerializeField] private Image _shopIcon;
    [SerializeField] private int _price = 100;
    [SerializeField] private HeadSkinData[] _headSkins;

    [SerializeField] private TMP_FontAsset _greenPrice;
    [SerializeField] private TMP_FontAsset _orangePrice;
    [SerializeField] private RawImage _yanIcon;

    [SerializeField] private AudioClip _buttonSound;

    private void OnValidate()
    {
        if (_shopIcon == null || _headSkins == null)
            return;

        foreach (var skin in _headSkins)
        {
            if (skin.SkinName == gameObject.name)
            {
                SkinID = skin.SkinID;
                _shopIcon.sprite = skin.Icon;
                break;
            }
        }

        CardButtonText.text = _price.ToString();
    }

    private void Start()
    {
        if (CardButton == null || CardButtonText == null)
            return;

        CardButtonText.text = _price.ToString();
        CardButton.onClick.AddListener(ChangeOrBuySkin);

        var gm = GameManager.Instance;
        if (gm == null)
            return;

        // Локализация текстов
        string usedText, useText;
        switch (gm.GetLanguageFromYandex())
        {
            case Language.Russian:
                usedText = "ИСПОЛЬЗУЕТСЯ";
                useText = "ИСПОЛЬЗОВАТЬ";
                break;
            case Language.Turkish:
                usedText = "KULLANILIYOR";
                useText = "KULLAN";
                break;
            case Language.Spanish:
                usedText = "USADO";
                useText = "USAR";
                break;
            case Language.German:
                usedText = "VERWENDET";
                useText = "VERWENDEN";
                break;
            case Language.English:
            default:
                usedText = "USED";
                useText = "USE";
                break;
        }

        // Показываем _yanIcon по умолчанию
        _yanIcon.gameObject.SetActive(true);

        if (SkinID == gm.CurrentHeadSkinID)
        {
            CardButtonText.text = usedText;
            CardButton.interactable = false;
            CardButtonText.font = _greenPrice;
            _yanIcon.gameObject.SetActive(false); // Скрываем значок "нового"
        }
        else if (gm.PurchasedHeadSkins.Contains(SkinID))
        {
            CardButtonText.text = useText;
            CardButton.interactable = true;
            CardButtonText.font = _orangePrice;
            _yanIcon.gameObject.SetActive(false); // Скрываем значок "нового"
        }
    }

    private void ChangeOrBuySkin()
    {

        var gm = GameManager.Instance;
        if (gm == null)
            return;

        string usedText;
        switch (gm.GetLanguageFromYandex())
        {
            case Language.Russian:
                usedText = "ИСПОЛЬЗУЕТСЯ";
                break;
            case Language.Turkish:
                usedText = "KULLANILIYOR";
                break;
            case Language.Spanish:
                usedText = "USADO";
                break;
            case Language.German:
                usedText = "VERWENDET";
                break;
            case Language.English:
            default:
                usedText = "USED";
                break;
        }

        if (gm.PurchasedHeadSkins.Contains(SkinID))
        {
            gm.ChangeHeadSkin(SkinID);
            gm.UIController.ResetAllHeadButtons();
            CardButtonText.text = usedText;
            CardButton.interactable = false;
            CardButtonText.font = _greenPrice;
            _yanIcon.gameObject.SetActive(false);

            gm.UIController.ShopCamera.SetCamera(gm.HeadController);
            gm.SaveProgress(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            int money = PlayerPrefs.GetInt(SaveData.MoneyKey, 0);
            if (money >= _price)
            {
                gm.UpdateMoney(money - _price);
                gm.PurchasedHeadSkins.Add(SkinID);

                gm.ChangeHeadSkin(SkinID);
                gm.UIController.ResetAllHeadButtons();
                CardButtonText.text = usedText;
                CardButton.interactable = false;
                CardButtonText.font = _greenPrice;
                _yanIcon.gameObject.SetActive(false);

                gm.UIController.ShopCamera.SetCamera(gm.HeadController);
                gm.SaveProgress(SceneManager.GetActiveScene().buildIndex);

                GameManager.Instance.UIController.PlaySound(_buttonSound);
            }
        }
    }
}
