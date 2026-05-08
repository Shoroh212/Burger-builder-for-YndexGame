using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class BlockShopCard : MonoBehaviour
{
    [field: SerializeField] public Button CardButton { get; private set; }
    [field: SerializeField] public TMP_Text CardButtonText { get; private set; }
    [field: SerializeField] public int SkinID { get; private set; }

    [SerializeField] private Image _shopIcon;
    [SerializeField] private int _price = 100;
    [SerializeField] private BlockSkinData[] _blockSkin;

    [SerializeField] private TMP_FontAsset _greenPrice;
    [SerializeField] private TMP_FontAsset _orangePrice;

    [SerializeField] private AudioClip _buttonSound;

    private void OnValidate()
    {
        if(_shopIcon == null || _blockSkin == null)
            return;
        foreach (var skin in _blockSkin)
        {
            if (SkinID == skin.SkinID)
            {
                gameObject.name = skin.SkinName;

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

        if (_shopIcon == null || _blockSkin == null)
            return;
        foreach (var skin in _blockSkin)
        {
            if (SkinID == skin.SkinID)
            {
                gameObject.name = skin.SkinName;

                _shopIcon.sprite = skin.Icon;
                break;
            }
        }

        CardButtonText.text = _price.ToString();

        CardButtonText.text = _price.ToString();
        CardButton.onClick.AddListener(ChangeOrBuySkin);

        var gm = GameManager.Instance;
        if (gm == null)
            return;

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

        if (SkinID == gm.CurrentBlockSkinID)
        {
            CardButtonText.text = usedText;
            CardButton.interactable = false;
            CardButtonText.font = _greenPrice;
        }
        else if (gm.PurchasedBlockSkins.Contains(SkinID))
        {
            CardButtonText.text = useText;
            CardButton.interactable = true;
            CardButtonText.font = _orangePrice;
        }
    }

    private void ChangeOrBuySkin()
    {
        var gm = GameManager.Instance;
        if (gm == null)
            return;

        // Ïåðåâîä äëÿ òåêñòà "USED"
        string usedText;
        switch (gm.GetLanguageFromYandex())
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

        if (gm.PurchasedBlockSkins.Contains(SkinID))
        {
            gm.ChangeBlockSkin(SkinID);
            gm.UIController.ResetAllBlockButtons();
            CardButtonText.text = usedText;
            CardButton.interactable = false;

            gm.UIController.ShopCamera.SetCamera();
            CardButtonText.font = _greenPrice;
            gm.SaveProgress(SceneManager.GetActiveScene().buildIndex);

        }
        else
        {
            int money = PlayerPrefs.GetInt(SaveData.MoneyKey, 0);
            if (money >= _price)
            {
                gm.UpdateMoney(money - _price);
                gm.PurchasedBlockSkins.Add(SkinID);

                gm.ChangeBlockSkin(SkinID);
                gm.UIController.ResetAllBlockButtons();
                CardButtonText.text = usedText;
                CardButton.interactable = false;

                gm.UIController.ShopCamera.SetCamera();
                CardButtonText.font = _greenPrice;
                gm.SaveProgress(SceneManager.GetActiveScene().buildIndex);

                GameManager.Instance.UIController.PlaySound(_buttonSound);
            }
        }
    }
}
