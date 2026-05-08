using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

[ExecuteAlways]
public class ShopCard : MonoBehaviour
{
    [field: SerializeField] public Button CardButton { get; set; }
    [field: SerializeField] public TMP_Text CardButtonText { get; set; }
    [field: SerializeField] public int SkinID { get; set; }

    [SerializeField] private int _price = 100;
    [field: SerializeField] public RareType RareType { get; private set; }
    [SerializeField] private Sprite _icon;
    [SerializeField] private RawImage _yanIcon;
    [SerializeField] private Image _shopIcon;

    [SerializeField] private SkinData[] _skins;
    [SerializeField] private Sprite[] _skinSprites;

    [SerializeField] private TMP_FontAsset _greenPrice;
    [SerializeField] private TMP_FontAsset _orangePrice;

    [SerializeField] private AudioClip _buttonSound;

    private void OnValidate()
    {
        if(_shopIcon != null)
            _shopIcon.sprite = _icon;

        if(_skins.Length > 0 && _skinSprites.Length > 0)
        {
            foreach(var item in _skins)
            {
                if(item.name == gameObject.name)
                {
                    SkinID = item.SkinID;
                }
            }
            foreach(var item in _skinSprites)
            {
                if(item.name == gameObject.name)
                {
                    _icon = item;
                    _shopIcon.sprite = item;
                }
            }
        }

        CardButtonText.text = _price.ToString();
    }

    private void Start()
    {
        CardButtonText.text = _price.ToString();
        CardButton.onClick.AddListener(ChangeOrBuySkin);

        var gm = GameManager.Instance;
        if (gm == null)
            return;

        // Ïîëó÷àåì ïåðåâåä¸ííûå ñòðîêè
        string usedText, useText;
        switch (gm.GetLanguageFromYandex())
        {
            case Language.Russian:
                usedText = "ÈÑÏÎËÜÇÓÅÒÑß";
                useText = "ÈÑÏÎËÜÇÎÂÀÒÜ";
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

        if (SkinID == 0 && gm.CurrentSkinID == 0)
        {
            CardButtonText.text = usedText;
            CardButton.interactable = false;

            CardButtonText.font = _greenPrice;
            _yanIcon.gameObject.SetActive(false);
        }
        else if (gm.CurrentSkinID == SkinID)
        {
            CardButtonText.text = usedText;
            CardButton.interactable = false;

            CardButtonText.font = _greenPrice;
            _yanIcon.gameObject.SetActive(false);
        }
        else if (gm.PurchasedSkins.Contains(SkinID))
        {
            CardButtonText.text = useText;
            CardButton.interactable = true;

            CardButtonText.font = _orangePrice;
            _yanIcon.gameObject.SetActive(false);
        }
    }


    public void ChangeOrBuySkin()
    {

        // Ïîëó÷àåì ÿçûê èãðîêà
        Language currentLanguage = GameManager.Instance.GetLanguageFromYandex();

        // Ïîäñòàâëÿåì íóæíûé ïåðåâîä
        string usedText;
        switch (currentLanguage)
        {
            case Language.Russian:
                usedText = "ÈÑÏÎËÜÇÓÅÒÑß";
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

        if (GameManager.Instance.PurchasedSkins.Contains(SkinID))
        {
            GameManager.Instance.ChangeSkin(SkinID);
            GameManager.Instance.UIController.ResetAllButtons();

            CardButtonText.text = usedText;
            CardButton.interactable = false;

            CardButtonText.font = _greenPrice;
            _yanIcon.gameObject.SetActive(false);
            GameManager.Instance.SaveProgress(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            if (PlayerPrefs.GetInt(SaveData.MoneyKey) >= _price)
            {
                GameManager.Instance.UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) - _price);
                GameManager.Instance.PurchasedSkins.Add(SkinID);

                GameManager.Instance.ChangeSkin(SkinID);
                GameManager.Instance.UIController.ResetAllButtons();

                CardButtonText.text = usedText;
                CardButton.interactable = false;

                CardButtonText.font = _greenPrice;
                _yanIcon.gameObject.SetActive(false);
                GameManager.Instance.SaveProgress(SceneManager.GetActiveScene().buildIndex);

                GameManager.Instance.UIController.PlaySound(_buttonSound);
            }
        }
    }
}
public enum RareType
{
    Common,
    Rare,
    Epic,
}