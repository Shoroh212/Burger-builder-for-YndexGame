using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _bonusLevelPanel;
    [SerializeField] private GameObject _deathPanel;
    [SerializeField] private GameObject _gamePanel;
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _fade;
    [SerializeField] private GameObject _settingsPanel;

    [Header("Left Buttons")]
    [SerializeField] private GameObject _fortuneWheelLeftButton;
    [SerializeField] private GameObject _minuteGiftLeftButton;
    [SerializeField] private GameObject _dailyGiftLeftButton;

    [Header("Other References")]
    [SerializeField] private GameObject _shopButton;
    [SerializeField] private GameObject _recipeImage;
    [SerializeField] private GameObject _playerHelp;
    [SerializeField] private Image _progressBarGame;

    [Header("Texts")]
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _fortuneWheelText;
    [SerializeField] private TMP_Text _moneyText;

    [Header("Luck & Income")]
    [SerializeField] private TMP_Text[] _luckPriceText;
    [SerializeField] private TMP_Text[] _luckLevelText;
    [SerializeField] private TMP_Text[] _incomePriceText;
    [SerializeField] private TMP_Text[] _incomeLevelText;

    [Header("Transforms")]
    [SerializeField] private RectTransform _recipeContainer;
    [SerializeField] private RectTransform _recipeContainerSide;

    [field: Header("Buttons")]
    [field: SerializeField] public Button IncomeButton { get; private set; }
    [field: SerializeField] public Button LuckButton { get; private set; }

    [SerializeField] private GameObject[] _luckButtomComp;
    [SerializeField] private GameObject[] _incomeButtomComp;

    private bool setAlreadyRecipe;
    public ShopCamera ShopCamera { get; private set; }
    private Coroutine _scrollRoutine;

    [SerializeField] private List<ShopCard> _shopCards;
    [SerializeField] private List<HeadShopCard> _shopHeadCards;
    [SerializeField] private List<BlockShopCard> _shopBlockCards;
    [SerializeField] private Sprite _roundedBackground;
    [SerializeField] private Sprite _burgerTopSprite;
    [SerializeField] private Sprite _burgerDownSprite;
    [SerializeField] private Scrollbar _shopScrollbar;
    [SerializeField] private Button _fortuneWheelButton;
    [SerializeField] private TMP_FontAsset _orangePrice;

    [Header("Final Panel")]
    [SerializeField] private GameObject _finalPanel;

    [field: Header("Final First")]
    public UIFinal UIFinal { get; private set; }
    public bool MultiplierUsed { get; set; }
    private List<Transform> _recipeObj = new List<Transform>();

    [SerializeField] private AudioClip _whoosh;
    [SerializeField] private AudioClip _star;

    private AudioSource _audioSource;

    private IEnumerator Start()
    {
        while (GameManager.Instance == null || !GameManager.Instance.IsInitialized)
        {
            yield return null;
        }
        _audioSource = GetComponent<AudioSource>();

        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        int lastBuildIndex = SceneManager.sceneCountInBuildSettings - 1;

        int levelIndex = 0;

        if (buildIndex == lastBuildIndex)
        {
            if (buildIndex > PlayerPrefs.GetInt(SaveData.LevelCount, buildIndex))
            {
                levelIndex = buildIndex;
            }
            else
            {
                levelIndex = PlayerPrefs.GetInt(SaveData.LevelCount, buildIndex);
            }
        }
        else
        {
            levelIndex = buildIndex;
        }

        buildIndex = levelIndex;

        _startPanel.SetActive(true);

        if ((buildIndex + 1) % 6 == 0 && buildIndex != 0)
            _startPanel.SetActive(false);
        _finalPanel.SetActive(false);

        _fortuneWheelLeftButton.SetActive(false);
        _dailyGiftLeftButton.SetActive(false);
        _shopButton.SetActive(false);
        _gamePanel.SetActive(false);

        if (buildIndex > 0)
        {
            _playerHelp.SetActive(false);

            _fortuneWheelLeftButton.SetActive(true);
            _shopButton.SetActive(true);

            if (buildIndex == 1)
            {
                FocusObject(_fortuneWheelLeftButton.transform);
                FocusObject(_shopButton.transform);
            }
        }

        if (buildIndex > 2)
        {
            _dailyGiftLeftButton.SetActive(true);

            if (buildIndex == 3)
            {
                FocusObject(_dailyGiftLeftButton.transform);
            }
        }

        _moneyText.text = PlayerPrefs.GetInt(SaveData.MoneyKey).ToString();
        //UpdateFortuneWheelText(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey).ToString());

        UpdateIncomeAndLuckText();

        UIFinal = GetComponent<UIFinal>();
        ShopCamera = FindAnyObjectByType<ShopCamera>();

        _shopScrollbar.value = 0;

        if (_scrollRoutine != null)
            StopCoroutine(_scrollRoutine);
        _scrollRoutine = StartCoroutine(SmoothScrollTo(0));

        ResetAllButtons();
        ChangeShopContainers(0);
    }

    public void HideHelpPanel()
    {
        _startPanel.SetActive(false);

        _fortuneWheelLeftButton.SetActive(false);
        _minuteGiftLeftButton.SetActive(false);
        _dailyGiftLeftButton.SetActive(false);
    }

    public void ShowPanel(GameObject panel) => panel.SetActive(!panel.activeSelf);

    public void EnableIncomeMode(bool isGift = false)
    {
        if (GameManager.Instance.EnableIncomeMode(isGift: isGift))
        {
            return;
        }

        foreach (var item in _incomeButtomComp)
        {
            if (item.TryGetComponent(out TMP_Text text))
            {
                if (item.name == "BackgroundOff")
                    item.SetActive(true);
                else
                    text.color = new Color(121f / 255f, 121f / 255f, 121f / 255f);

            }
            else if (item.TryGetComponent(out Image image))
            {
                image.color = new Color(121f / 255f, 121f / 255f, 121f / 255f);
            }
        }

        for (int i = 0; i < _incomePriceText.Length; i++)
        {
            switch (GameManager.Instance.GetLanguageFromYandex())
            {
                case Language.Russian:
                    _incomePriceText[i].text = "НЕДОСТУПНО";
                    break;
                case Language.Turkish:
                    _incomePriceText[i].text = "KULLANILAMAZ";
                    break;
                case Language.English:
                    _incomePriceText[i].text = "UNAVAILABLE";
                    break;
                case Language.Spanish:
                    _incomePriceText[i].text = "NO DISPONIBLE";
                    break;
                case Language.German:
                    _incomePriceText[i].text = "NICHT VERFÜGBAR";
                    break;
                default:
                    _incomePriceText[i].text = "UNAVAILABLE";
                    break;
            }
        }
    }

    public void EnableLuckMode(bool isGift = false)
    {
        if (GameManager.Instance.EnableLuckMode(isGift: isGift))
        {
            return;
        }

        foreach (var item in _luckButtomComp)
        {
            if (item.TryGetComponent(out TMP_Text text))
            {
                if (item.name == "BackgroundOff")
                    item.SetActive(true);
                else
                    text.color = new Color(85f / 255f, 85f / 255f, 85f / 255f);

            }
            else if (item.TryGetComponent(out Image image))
            {
                image.color = new Color(85f / 255f, 85f / 255f, 85f / 255f);
            }
        }

        for (int i = 0; i < _luckPriceText.Length; i++)
        {
            switch (GameManager.Instance.GetLanguageFromYandex())
            {
                case Language.Russian:
                    _luckPriceText[i].text = "НЕДОСТУПНО";
                    break;
                case Language.Turkish:
                    _luckPriceText[i].text = "KULLANILAMAZ";
                    break;
                case Language.English:
                    _luckPriceText[i].text = "UNAVAILABLE";
                    break;
                case Language.Spanish:
                    _luckPriceText[i].text = "NO DISPONIBLE";
                    break;
                case Language.German:
                    _luckPriceText[i].text = "NICHT VERFÜGBAR";
                    break;
                default:
                    _luckPriceText[i].text = "UNAVAILABLE";
                    break;
            }

        }
    }

    public void UpdateMoneyText(int newMoney)
    {
        int currentMoney = int.Parse(_moneyText.text);

        DOTween.To(() => currentMoney, x =>
        {
            currentMoney = x;
            _moneyText.text = currentMoney.ToString();
        }, newMoney, 0.5f).SetEase(Ease.OutCubic);
    }

    public void LoadNextScene()
    {
        DOTween.KillAll();

        if (PlayerPrefs.GetInt(SaveData.LevelCount) == SceneManager.sceneCountInBuildSettings)
        {
            int nextIndex = PlayerPrefs.GetInt(SaveData.LevelCount) + 1;
            PlayerPrefs.SetInt(SaveData.LevelCount, nextIndex);
            SceneManager.LoadScene(SceneManager.sceneCount);
        }
        else
        {
            int nextIndex = PlayerPrefs.GetInt(SaveData.LevelCount) + 1;
            PlayerPrefs.SetInt(SaveData.LevelCount, nextIndex);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        }
    }

    public void ReloadScene()
    {
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void UpdateSceneText(string newLevel)
    {
        switch (GameManager.Instance.GetLanguageFromYandex())
        {
            case Language.Russian:
                _levelText.text = "УРОВЕНЬ: " + newLevel;
                break;
            case Language.Turkish:
                _levelText.text = "SEVİYE: " + newLevel;
                break;
            case Language.English:
                _levelText.text = "LEVEL: " + newLevel;
                break;
            case Language.Spanish:
                _levelText.text = "NIVEL: " + newLevel;
                break;
            case Language.German:
                _levelText.text = "LEVEL: " + newLevel;
                break;
            default:
                _levelText.text = "LEVEL: " + newLevel;
                break;
        }
    }

    public void SetCheckRecipe(Sprite ingredientSprite)
    {
        foreach (Transform item in _recipeObj)
        {
            if (item.GetChild(1).GetComponent<Image>().sprite == ingredientSprite)
            {
                item.GetChild(2).gameObject.SetActive(true);

            }
        }
    }

    public void SetRecipe()
    {
        _audioSource.volume = PlayerPrefs.GetFloat(SaveData.SoundKey, 0.3f);

        if (setAlreadyRecipe)
            return;

        int counter = 0;
        var spawned = new List<RectTransform>();
        Sequence seq = DOTween.Sequence();

        // ===== Верхняя булка =====
        {
            GameObject imageGO = Instantiate(_recipeImage, _recipeContainer);
            RectTransform rt = imageGO.GetComponent<RectTransform>();
            spawned.Add(rt);

            rt.localScale = Vector3.one * 2f;

            CanvasGroup cg = imageGO.GetComponent<CanvasGroup>();
            if (!cg) cg = imageGO.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            Image bgImage = imageGO.transform.GetChild(0).GetComponent<Image>();
            bgImage.sprite = _roundedBackground;

            foreach (Transform child in imageGO.transform)
            {
                if (child.CompareTag("UIIngredientImage"))
                {
                    var img = child.GetComponent<Image>();
                    var rect = img.rectTransform;

                    img.sprite = _burgerTopSprite;
                    rect.sizeDelta = new Vector2(240f, 120f);
                    img.color = Color.white;
                }
                if (child.CompareTag("RowBackground"))
                {
                    child.GetComponent<Image>().color = Color.white;
                }
            }

            // Звук синхронно с анимацией
            seq.AppendCallback(() =>
            {
                if (_audioSource != null && _whoosh != null)
                {
                    _audioSource.pitch = Random.Range(0.95f, 1.05f);
                    _audioSource.PlayOneShot(_whoosh);
                }
            });

            seq.Join(rt.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
            seq.Join(cg.DOFade(1f, 0.25f));
            seq.AppendInterval(0.05f);

            counter++;
        }

        // ===== Ингредиенты =====
        foreach (var item in GameManager.Instance.Recipe)
        {
            GameObject imageGO = Instantiate(_recipeImage, _recipeContainer);
            _recipeObj.Add(imageGO.transform);

            RectTransform rt = imageGO.GetComponent<RectTransform>();
            spawned.Add(rt);

            rt.localScale = Vector3.one * 2f;

            CanvasGroup cg = imageGO.GetComponent<CanvasGroup>();
            if (!cg) cg = imageGO.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            foreach (Transform child in imageGO.transform)
            {
                if (child.CompareTag("UIIngredientImage"))
                {
                    var img = child.GetComponent<Image>();
                    img.sprite = item.Icon;
                    img.color = Color.white;
                }

                if (child.CompareTag("RowBackground"))
                {
                    if (counter % 2 == 1)
                        child.GetComponent<Image>().color = new Color32(255, 186, 207, 255); // розовый
                    else
                        child.GetComponent<Image>().color = Color.white;
                }
            }

            seq.AppendCallback(() =>
            {
                if (_audioSource != null && _whoosh != null)
                {
                    _audioSource.pitch = Random.Range(0.95f, 1.05f);
                    _audioSource.PlayOneShot(_whoosh);
                }
            });

            seq.Join(rt.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
            seq.Join(cg.DOFade(1f, 0.25f));
            seq.AppendInterval(0.05f);

            counter++;
        }

        // ===== Нижняя булка =====
        {
            GameObject imageGO = Instantiate(_recipeImage, _recipeContainer);
            RectTransform rt = imageGO.GetComponent<RectTransform>();
            spawned.Add(rt);

            rt.localScale = Vector3.one * 2f;

            CanvasGroup cg = imageGO.GetComponent<CanvasGroup>();
            if (!cg) cg = imageGO.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            Image bgImage = imageGO.transform.GetChild(0).GetComponent<Image>();
            bgImage.sprite = _roundedBackground;
            bgImage.rectTransform.localScale = new Vector3(1f, -1f, 1f);

            foreach (Transform child in imageGO.transform)
            {
                if (child.CompareTag("UIIngredientImage"))
                {
                    var img = child.GetComponent<Image>();
                    var rect = img.rectTransform;

                    img.sprite = _burgerDownSprite;
                    rect.sizeDelta = new Vector2(240f, 120f);
                    img.color = Color.white;
                }
                if (child.CompareTag("RowBackground"))
                {
                    bool lastIsPink = (counter - 1) % 2 == 1;
                    child.GetComponent<Image>().color = lastIsPink ? Color.white : new Color32(255, 186, 207, 255);
                }
            }

            seq.AppendCallback(() =>
            {
                if (_audioSource != null && _whoosh != null)
                {
                    _audioSource.pitch = Random.Range(0.95f, 1.05f);
                    _audioSource.PlayOneShot(_whoosh);
                }
            });

            seq.Join(rt.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
            seq.Join(cg.DOFade(1f, 0.25f));
            seq.AppendInterval(0.05f);

            counter++;
        }

        seq.AppendInterval(1f);

        seq.OnComplete(() =>
        {
            _recipeContainer.SetParent(_recipeContainerSide);
            _recipeContainer.DOMove(_recipeContainerSide.position, 0.7f);
            _recipeContainerSide.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.7f);
            _gamePanel.SetActive(true);
            GameManager.Instance.Player.CanMove = true;
        });

        setAlreadyRecipe = true;
    }
    public void SetStartPanelAfterBonusGame()
    {
        _startPanel.SetActive(true);
        _fortuneWheelLeftButton.SetActive(true);
        _minuteGiftLeftButton.SetActive(true);
        _dailyGiftLeftButton.SetActive(true);
    }
    public void ProposeBonusLevel()
    {
        HideHelpPanel();

        _bonusLevelPanel.SetActive(true);
        _startPanel.SetActive(false);

        CanvasGroup canvasGroup = _bonusLevelPanel.GetComponentInChildren<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1f, 0.5f); // Плавное появление за 0.5 секунды
        //_bonusLevelPanel.transform.localScale = Vector3.zero;
        //_bonusLevelPanel.transform.DOScale(Vector3.one, 0.2f);
    }

    public void EnableBonusLevel()
    {
        YandexGame.RewVideoShow(SaveData.BonusLevelReward);
    }

    public void HideBonusPanel() => _bonusLevelPanel.SetActive(false);

    public void SkipBonusLevelLevel() => GameManager.Instance.SkipBonusLevelLevel();

    public void ResetAllButtons() // Rename
    {
        foreach (var item in _shopCards)
        {
            if (GameManager.Instance.PurchasedSkins.Contains(item.SkinID))
            {
                item.CardButton.interactable = true;

                switch (GameManager.Instance.GetLanguageFromYandex())
                {
                    case Language.Russian:
                        item.CardButtonText.text = "ИСПОЛЬЗОВАТЬ";
                        break;
                    case Language.Turkish:
                        item.CardButtonText.text = "KULLAN";
                        break;
                    case Language.English:
                        item.CardButtonText.text = "USE";
                        break;
                    case Language.Spanish:
                        item.CardButtonText.text = "USAR";
                        break;
                    case Language.German:
                        item.CardButtonText.text = "BENUTZEN";
                        break;
                    default:
                        item.CardButtonText.text = "USE";
                        break;
                }

                item.CardButtonText.font = _orangePrice;
            }
        }
    }

    public void ResetAllHeadButtons()
    {
        foreach (var item in _shopHeadCards)
        {
            if (GameManager.Instance.PurchasedHeadSkins.Contains(item.SkinID))
            {
                item.CardButton.interactable = true;

                switch (GameManager.Instance.GetLanguageFromYandex())
                {
                    case Language.Russian:
                        item.CardButtonText.text = "ИСПОЛЬЗОВАТЬ";
                        break;
                    case Language.Turkish:
                        item.CardButtonText.text = "KULLAN";
                        break;
                    case Language.English:
                        item.CardButtonText.text = "USE";
                        break;
                    case Language.Spanish:
                        item.CardButtonText.text = "USAR";
                        break;
                    case Language.German:
                        item.CardButtonText.text = "BENUTZEN";
                        break;
                    default:
                        item.CardButtonText.text = "USE";
                        break;
                }

                item.CardButtonText.font = _orangePrice;
            }
        }
    }

    public void ResetAllBlockButtons()
    {
        foreach (var item in _shopBlockCards)
        {
            if (GameManager.Instance.PurchasedBlockSkins.Contains(item.SkinID))
            {
                item.CardButton.interactable = true;

                switch (GameManager.Instance.GetLanguageFromYandex())
                {
                    case Language.Russian:
                        item.CardButtonText.text = "ИСПОЛЬЗОВАТЬ";
                        break;
                    case Language.Turkish:
                        item.CardButtonText.text = "KULLAN";
                        break;
                    case Language.English:
                        item.CardButtonText.text = "USE";
                        break;
                    case Language.Spanish:
                        item.CardButtonText.text = "USAR";
                        break;
                    case Language.German:
                        item.CardButtonText.text = "BENUTZEN";
                        break;
                    default:
                        item.CardButtonText.text = "USE";
                        break;
                }

                item.CardButtonText.font = _orangePrice;
            }
        }
    }

    public void UpdateFortuneWheelText(string newFortune)
    {
        _fortuneWheelText.text = newFortune;

        if (PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) > 0)
        {
            _fortuneWheelButton.interactable = true;
            _fortuneWheelButton.GetComponentInChildren<TMP_Text>().color = new Color(1, 1, 1, 1f);


        }
    }

    public void UpdateIncomeAndLuckText()
    {
        int money = PlayerPrefs.GetInt(SaveData.MoneyKey, 0);

        bool canBuyIncome = money >= GameManager.Instance.IncomeModePrice;
        bool canBuyLuck = money >= GameManager.Instance.LuckModePrice;

        if (GameManager.Instance.IncomeRewardShowed && canBuyIncome)
            GameManager.Instance.IncomeRewardShowed = false;

        if (GameManager.Instance.LuckRewardShowed && canBuyLuck)
            GameManager.Instance.LuckRewardShowed = false;

        Color gray = new Color(121f / 255f, 121f / 255f, 121f / 255f);
        Color white = Color.white;

        string unavailableText;
        string freeText;
        string levelText;

        switch (GameManager.Instance.GetLanguageFromYandex())
        {
            case Language.Russian:
                unavailableText = "НЕДОСТУПНО";
                freeText = "БЕСПЛАТНО";
                levelText = "УРОВЕНЬ";
                break;
            case Language.Turkish:
                unavailableText = "KULLANILAMAZ";
                freeText = "ÜCRETSİZ";
                levelText = "SEVİYE";
                break;
            case Language.Spanish:
                unavailableText = "NO DISPONIBLE";
                freeText = "GRATIS";
                levelText = "NIVEL";
                break;
            case Language.German:
                unavailableText = "NICHT VERFÜGBAR";
                freeText = "KOSTENLOS";
                levelText = "LEVEL"; // Или "STUFE", если хочешь
                break;
            case Language.English:
            default:
                unavailableText = "UNAVAILABLE";
                freeText = "FREE";
                levelText = "LEVEL";
                break;
        }

        // --- INCOME ---
        bool incomeAvailable = canBuyIncome || !GameManager.Instance.IncomeRewardShowed;

        foreach (var item in _incomeButtomComp)
        {
            if (item.TryGetComponent(out TMP_Text text))
            {
                if (item.name == "BackgroundOff")
                    item.SetActive(!incomeAvailable);
                else
                    text.color = incomeAvailable ? white : gray;
            }
            else if (item.TryGetComponent(out Image image))
            {
                image.color = incomeAvailable ? white : gray;
            }
        }

        for (int i = 0; i < _incomePriceText.Length; i++)
        {
            if (GameManager.Instance.IncomeRewardShowed && !canBuyIncome)
                _incomePriceText[i].text = unavailableText;
            else if (canBuyIncome)
                _incomePriceText[i].text = $"{GameManager.Instance.IncomeModePrice} $";
            else
                _incomePriceText[i].text = freeText;

            _incomeLevelText[i].text = $"{levelText} {GameManager.Instance.IncomeLevel}";
        }

        // --- LUCK ---
        bool luckAvailable = canBuyLuck || !GameManager.Instance.LuckRewardShowed;

        foreach (var item in _luckButtomComp)
        {
            if (item.TryGetComponent(out TMP_Text text))
            {
                if (item.name == "BackgroundOff")
                    item.SetActive(!luckAvailable);
                else
                    text.color = luckAvailable ? white : gray;
            }
            else if (item.TryGetComponent(out Image image))
            {
                image.color = luckAvailable ? white : gray;
            }
        }

        for (int i = 0; i < _luckPriceText.Length; i++)
        {
            if (GameManager.Instance.LuckRewardShowed && !canBuyLuck)
                _luckPriceText[i].text = unavailableText;
            else if (canBuyLuck)
                _luckPriceText[i].text = $"{GameManager.Instance.LuckModePrice} $";
            else
                _luckPriceText[i].text = freeText;

            _luckLevelText[i].text = $"{levelText} {GameManager.Instance.LuckLevel}";
        }
    }

    private void FocusObject(Transform focusObject)
    {
        Sequence sequence = DOTween.Sequence();

        Image image = focusObject.GetComponent<Image>();
        if (image != null)
        {
            Color tempColor = image.color;
            tempColor.a = 0;
            image.color = tempColor;

            sequence.Append(image.DOFade(1, 0.5f));
        }

        focusObject.localScale = Vector3.zero;
        sequence.Join(focusObject.DOScale(1f, 0.5f).SetEase(Ease.OutBack));

        sequence.Append(focusObject.DOScale(1.1f, 0.2f));
        sequence.Append(focusObject.DOScale(1f, 0.2f));
    }

    public void UpdateGameProgressBar(float value)
    {
        _progressBarGame.fillAmount = value;
    }

    public void ShowShopPanel()
    {
        CanvasGroup fadeCg = _fade.GetComponent<CanvasGroup>() ?? _fade.AddComponent<CanvasGroup>();
        fadeCg.alpha = 0;
        _fade.SetActive(true);

        RectTransform shopRT = _shopPanel.GetComponent<RectTransform>();
        Vector2 shopTargetPos = shopRT.anchoredPosition;
        float flyDistance = Screen.height * 1.1f;

        shopRT.anchoredPosition += Vector2.up * flyDistance;
        _shopPanel.SetActive(true);

        RectTransform blueRT = shopRT.Find("SideButton/BlueButton").GetComponent<RectTransform>();
        RectTransform greenRT = shopRT.Find("SideButton/GreenButton").GetComponent<RectTransform>();
        RectTransform orangeRT = shopRT.Find("SideButton/OrangeButton").GetComponent<RectTransform>();

        Vector2 downOffset = Vector2.down * (Screen.height * 0.8f);
        foreach (RectTransform rt in new[] { blueRT, greenRT, orangeRT })
        {
            rt.anchoredPosition += downOffset;
            rt.gameObject.SetActive(false);
        }

        Sequence seq = DOTween.Sequence();

        seq.Insert(0f, fadeCg.DOFade(0.75f, 0.25f));
        seq.Insert(0.05f, shopRT.DOAnchorPos(shopTargetPos, 0.45f).SetEase(Ease.OutBack, overshoot: 1.2f));

        float startAt = 0.4f;
        float step = 0.15f;
        RectTransform[] colored = { blueRT, greenRT, orangeRT };

        foreach (RectTransform rt in colored)
        {
            seq.InsertCallback(startAt, () => rt.gameObject.SetActive(true));
            seq.Insert(startAt, rt.DOAnchorPos(rt.anchoredPosition - downOffset, 0.35f).SetEase(Ease.OutBack));
            startAt += step;
        }

        seq.Play();
    }

    public void HideShopPanel()
    {
        CanvasGroup fadeCg = _fade.GetComponent<CanvasGroup>() ?? _fade.AddComponent<CanvasGroup>();

        RectTransform shopRT = _shopPanel.GetComponent<RectTransform>();
        Vector2 originalShopPos = shopRT.anchoredPosition;
        float flyDistance = Screen.height * 1.1f;

        RectTransform blueRT = shopRT.Find("SideButton/BlueButton").GetComponent<RectTransform>();
        RectTransform greenRT = shopRT.Find("SideButton/GreenButton").GetComponent<RectTransform>();
        RectTransform orangeRT = shopRT.Find("SideButton/OrangeButton").GetComponent<RectTransform>();

        Vector2 downOffset = Vector2.down * (Screen.height * 0.8f);

        // Сохраняем изначальные позиции кнопок
        Vector2 blueOriginalPos = blueRT.anchoredPosition;
        Vector2 greenOriginalPos = greenRT.anchoredPosition;
        Vector2 orangeOriginalPos = orangeRT.anchoredPosition;

        RectTransform[] colored = { blueRT, greenRT, orangeRT };
        Vector2[] originalPositions = { blueOriginalPos, greenOriginalPos, orangeOriginalPos };

        Sequence seq = DOTween.Sequence();

        float step = 0.1f;
        float buttonAnimDuration = 0.3f;
        float panelAnimDuration = 0.4f;
        float fadeDuration = panelAnimDuration; // чтобы fade длился столько же, сколько анимация панели

        float startAt = 0f;

        // Анимируем кнопки с последовательной задержкой
        for (int i = 0; i < colored.Length; i++)
        {
            RectTransform rt = colored[i];
            Vector2 startPos = originalPositions[i];

            seq.Insert(startAt, rt.DOAnchorPos(startPos + downOffset, buttonAnimDuration).SetEase(Ease.InBack));
            seq.InsertCallback(startAt + buttonAnimDuration, () => rt.gameObject.SetActive(false));

            startAt += step;
        }

        // Анимация панели и затухание фона начинаются после анимации кнопок
        Vector2 offscreenPos = originalShopPos + Vector2.up * flyDistance;
        seq.Insert(startAt, shopRT.DOAnchorPos(offscreenPos, panelAnimDuration).SetEase(Ease.InBack));
        seq.Insert(startAt, fadeCg.DOFade(0f, fadeDuration));

        seq.OnComplete(() =>
        {
            // Возвращаем позиции
            shopRT.anchoredPosition = originalShopPos;
            blueRT.anchoredPosition = blueOriginalPos;
            greenRT.anchoredPosition = greenOriginalPos;
            orangeRT.anchoredPosition = orangeOriginalPos;

            // Обнуляем прозрачность и выключаем объекты
            fadeCg.alpha = 0f;
            _shopPanel.SetActive(false);
            _fade.SetActive(false);
        });

        seq.Play();
    }

    public void ChangeShopContainers(int index)
    {
        float target = 0f;

        switch (index)
        {
            case 0:
                ShopCamera.SetCamera(GameManager.Instance.Player);
                target = 0f;
                break;

            case 1:
                ShopCamera.SetCamera(GameManager.Instance.HeadController);
                target = 0.5f;
                break;

            case 2:
                target = 1f;
                ShopCamera.SetCamera();
                break;
        }

        if (_scrollRoutine != null)
            StopCoroutine(_scrollRoutine);
        _scrollRoutine = StartCoroutine(SmoothScrollTo(target));
    }

    public void PlayFinalPanel()
    {
        _recipeContainer.gameObject.SetActive(false);
        _progressBarGame.transform.parent.gameObject.SetActive(false);

        UIFinal.Play();

    }

    public void SkipLevelReward()
    {
        YandexGame.RewVideoShow(SaveData.SkipLevel);
    }

    private IEnumerator SmoothScrollTo(float target)
    {
        float start = _shopScrollbar.value;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / 0.25f;
            _shopScrollbar.value = Mathf.Lerp(start, target, t);
            yield return null;
        }

        _shopScrollbar.value = target;
    }

    public void TogglePanel(bool show)
    {
        CanvasGroup canvasGroup = _fade.GetComponent<CanvasGroup>();

        if (_settingsPanel == null || canvasGroup == null)
            return;

        if (show)
        {
            _settingsPanel.SetActive(true);
            canvasGroup.gameObject.SetActive(true);

            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.Linear);

            _settingsPanel.transform.localScale = Vector3.zero;
            _settingsPanel.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack);
        }
        else
        {
            canvasGroup.DOFade(0f, 0.25f)
                .SetEase(Ease.Linear);

            _settingsPanel.transform.DOScale(Vector3.zero, 0.25f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    _settingsPanel.SetActive(false);
                    canvasGroup.gameObject.SetActive(false);
                });
        }
    }

    public void PlaySound(AudioClip audioClip)
    {
        _audioSource.volume = PlayerPrefs.GetFloat(SaveData.SoundKey, 0.3f);
        _audioSource.PlayOneShot(audioClip);
    }
}