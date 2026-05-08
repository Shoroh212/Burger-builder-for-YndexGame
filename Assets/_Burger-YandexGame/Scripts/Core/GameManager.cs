using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // -------------------- Price --------------------
    [field: Header("Price")]
    [field: SerializeField] public int GoodIngredientPrice { get; private set; }
    [field: SerializeField] public int BadIngredientPrice { get; private set; }
    [field: SerializeField] public int RecipeIngredientPrice { get; private set; }
    [field: SerializeField] public int IncomeModePrice { get; private set; }
    [field: SerializeField] public int LuckModePrice { get; private set; }

    // -------------------- Multiply --------------------
    [field: Header("Multiply")]
    [field: SerializeField] public float IncomeMultiply { get; private set; }
    [field: SerializeField] public float LuckMultiply { get; private set; }
    [field: SerializeField] public float BonusLevelMultiply { get; private set; }

    [field: SerializeField] public float LuckModeChance { get; private set; }
    [field: SerializeField] public float LuckModeBonus { get; private set; } = 0.25f;

    // -------------------- Prefabs --------------------
    [Header("Prefabs")]
    [SerializeField] private GameObject _luckTextPrefab;
    [SerializeField] private GameObject _luckParticlePrefab;
    [SerializeField] private GameObject _playerPrefab;
    [field: SerializeField] public GameObject[] IngredientPrefabs { get; set; }

    // -------------------- Materials & Colors --------------------
    [Header("Materials")]
    [SerializeField] private Material _roadMaterialOne;
    [SerializeField] private Material _roadMaterialTwo;

    [Space(10)]
    [SerializeField] private Color _roadColorDefaultOne;
    [SerializeField] private Color _roadColorDefaultTwo;

    [Space(10)]
    [SerializeField] private Color _roadColorBonusLevelOne;
    [SerializeField] private Color _roadColorBonusLevelTwo;

    // -------------------- UI --------------------
    [Header("UI")]
    [HideInInspector] public UIController UIController;

    // -------------------- Skins --------------------
    [field: Header("Skins")]
    [field: SerializeField] public SkinData[] Skins { get; private set; }
    [field: SerializeField] public Sprite[] SkinSprites { get; private set; }

    [field: Header("Head Skins")]
    [field: SerializeField] public HeadSkinData[] HeadSkins { get; private set; }
    [field: SerializeField] public Sprite[] HeadSkinSprites { get; private set; }

    [field: Header("Block Skins")]
    [field: SerializeField] public BlockSkinData[] BlockSkins { get; private set; }
    [field: SerializeField] public Sprite[] BlockSkinSprites { get; private set; }

    public int CurrentSkinID { get; set; }
    [field: SerializeField] public List<int> PurchasedSkins { get; set; } = new List<int>();

    public int CurrentHeadSkinID { get; set; }
    [field: SerializeField] public List<int> PurchasedHeadSkins { get; set; } = new List<int>();

    public int CurrentBlockSkinID { get; set; }
    [field: SerializeField] public List<int> PurchasedBlockSkins { get; set; } = new List<int>();

    // -------------------- Player and Game State --------------------
    public Player Player { get; private set; }
    public bool GameLaunch { get; private set; }
    public int TotalIngredientsCount { get; set; }
    public int LevelIndex { get; private set; }

    public bool IncomeModeEnabled { get; private set; }
    public bool LuckModeEnabled { get; private set; }
    public bool BonusLevelEnabled { get; private set; }

    public int IncomeLevel { get; private set; } = 1;
    public int LuckLevel { get; private set; } = 1;

    public bool LuckRewardShowed { get; set; }
    public bool IncomeRewardShowed { get; set; }

    // -------------------- Recipe and Ingredients --------------------
    //public List<RecipeIngredient> Recipe { get; private set; } = new List<RecipeIngredient>();
    public List<Ingredient> Recipe { get; private set; } = new List<Ingredient>();
    public List<Ingredient> FinalIngredients { get; set; } = new List<Ingredient>();

    private RecipeData _recipeData;
    public HeadController HeadController { get; set; }

    private AudioSource _musicSource;

    public bool IsInitialized { get; private set; } = false;
    [SerializeField] private Canvas _portraitCanvas;

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        IsInitialized = true;

        GameManager.Instance.UIController = FindAnyObjectByType<UIController>();
#if UNITY_EDITOR


        Destroy(GameManager.Instance.UIController.gameObject);

        GameManager.Instance.UIController = Instantiate(_portraitCanvas).GetComponent<UIController>();

#else
        
    
        switch (YandexGame.EnvironmentData.deviceType)
        {
            case "desktop":
                break;

            case "mobile":
                if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
                {
                    Destroy(GameManager.Instance.UIController.gameObject);

                    GameManager.Instance.UIController = Instantiate(_portraitCanvas).GetComponent<UIController>();
                }
                break;
        }
#endif

        int buildIndex = scene.buildIndex;
        int lastBuildIndex = SceneManager.sceneCountInBuildSettings - 1;

        if (buildIndex == lastBuildIndex)
        {
            if (buildIndex > PlayerPrefs.GetInt(SaveData.LevelCount, buildIndex))
            {
                LevelIndex = buildIndex;
            }
            else
            {
                LevelIndex = PlayerPrefs.GetInt(SaveData.LevelCount, buildIndex);
            }
        }
        else
        {
            LevelIndex = buildIndex;
        }

        IncomeModeEnabled = false;
        LuckModeEnabled = false;
        BonusLevelEnabled = false;

        _roadMaterialOne.color = _roadColorDefaultOne;
        _roadMaterialTwo.color = _roadColorDefaultTwo;

        if (LevelIndex == 0)
        {
            _recipeData = Resources.Load<RecipeData>("Recipe/Level");
            //PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, 2);
            //int spins = PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey, 0);
            //UIController.UpdateFortuneWheelText(spins.ToString());
        }
        else
        {
            _recipeData = Resources.Load<RecipeData>($"Recipe/Level {LevelIndex}");
        }

        if (_recipeData != null)
            Recipe = _recipeData.RecipeIngredients;

        Player = FindAnyObjectByType<Player>();

        if (!HeadController)
            HeadController = FindAnyObjectByType<HeadController>();

        TotalIngredientsCount = FindObjectsByType<Ingredient>(FindObjectsSortMode.None).Length;

        UIController.UpdateSceneText((scene.buildIndex + 1).ToString());
        Player.CanMove = false;
        RefreshLuckModeChance(LuckLevel);

        LuckRewardShowed = false;
        IncomeRewardShowed = false;
        if (FirstLevelLoaderLogic.instance != null)
            FirstLevelLoaderLogic.instance.DeletePanel();

    }

    private void Awake()
    {
        _musicSource = GetComponent<AudioSource>();
        _musicSource.volume = PlayerPrefs.GetFloat(SaveData.MusicKey, 0.3f);
        _musicSource.Play();

        PurchasedSkins.Add(0);
        PurchasedHeadSkins.Add(0);
        PurchasedBlockSkins.Add(0);

        DOTween.Init();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Запускаем корутину ожидания SDK
            //StartCoroutine(WaitForSDKAndInit());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (YandexGame.SDKEnabled)
        {
            GetLoad();
            SceneManager.LoadScene(YandexGame.savesData.levelIndex);

            SceneManager.sceneLoaded += OnSceneLoad;
        }
    }

    //public void ApplyLoadedData()
    //{

    //    //ChangeSkin(CurrentSkinID);
    //    //ChangeHeadSkin(CurrentHeadSkinID);
    //    //ChangeBlockSkin(CurrentBlockSkinID);
    //}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            UpdateMoney(10000);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            UpdateMoney(200);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetSave();
        }
    }

    private void ResetSave()
    {
        PlayerPrefs.DeleteAll();

        YandexGame.savesData.money = 0;

        // Сохраняем скины и прогресс
        YandexGame.savesData.currentSkinID = 0;
        YandexGame.savesData.purchasedSkins.Clear();

        YandexGame.savesData.currentHeadSkinID = 0;
        YandexGame.savesData.purchasedHeadSkins.Clear();

        YandexGame.savesData.currentBlockSkinID = 0;
        YandexGame.savesData.purchasedBlockSkins.Clear();

        YandexGame.savesData.levelIndex = 0;
        YandexGame.savesData.incomeLevel = 1;
        YandexGame.savesData.luckLevel = 1;

        YandexGame.savesData.fortuneSpin = 0;

        YandexGame.savesData.LastSavedDate = "";
        YandexGame.savesData.LastSavedStreak = 0;

        IncomeLevel = 1;
        LuckLevel = 1;

        PurchasedSkins.Clear();
        PurchasedHeadSkins.Clear();
        PurchasedBlockSkins.Clear();
        PurchasedSkins.Add(0);
        PurchasedHeadSkins.Add(0);
        PurchasedBlockSkins.Add(0);
        SceneManager.LoadScene(0);

        ChangeSkin(0);
        ChangeHeadSkin(0);
        ChangeBlockSkin(0);

        Debug.Log("RESET SAVE");

        YandexGame.SaveProgress();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    public void LaunchGame()
    {
        GameLaunch = true;
        UIController.HideHelpPanel();
        UIController.SetRecipe();
    }

    public void StopGame()
    {
        Player.CanMove = false;

        if (Player.Ingredients.Count > 0)
            HeadController.PlayEatAnimation();
        else
            HeadController.PlayEatAnimation();
        //LoseGame();
    }

    public void FinalGame()
    {
        //HeadController.gameObject.SetActive(false);

        UIController.PlayFinalPanel();
    }

    public void LoseGame()
    {
        Player.CanMove = false;
        Player.Vertical = 0;
        Camera.main.transform.SetParent(null);
        Player.transform.root.gameObject.SetActive(false);
        UIController.UIFinal.ShowFailPanel();
    }

    public void UpdateMoney(int newValue)
    {
        if (newValue >= 0)
        {
            PlayerPrefs.SetInt(SaveData.MoneyKey, newValue);
        }
        else
        {
            PlayerPrefs.SetInt(SaveData.MoneyKey, 0);
        }


        UIController.UpdateMoneyText(PlayerPrefs.GetInt(SaveData.MoneyKey, 0));

        UIController.UpdateIncomeAndLuckText();

        SaveProgress(SceneManager.GetActiveScene().buildIndex);
    }

    public bool EnableLuckMode(bool reward = false, bool isGift = false)
    {
        if (LuckRewardShowed)
            return false;

        int money = PlayerPrefs.GetInt(SaveData.MoneyKey);
        bool enableLuckMode = false;

        if (isGift)
        {
            enableLuckMode = true;
        }
        else if (money >= LuckModePrice || reward)
        {
            if (money >= LuckModePrice)
                UpdateMoney(money - LuckModePrice);

            enableLuckMode = true;
        }
        else
        {
            YandexGame.RewVideoShow(SaveData.LuckReward);
            LuckRewardShowed = true;
            return false;
        }

        if (enableLuckMode)
        {
            RefreshLuckModeChance(LuckLevel);
            LuckLevel++;
            LuckModePrice = 200 * LuckLevel;
            UIController.UpdateIncomeAndLuckText();

            SaveProgress(SceneManager.GetActiveScene().buildIndex);

            foreach (var item in FindObjectsByType<Ingredient>(FindObjectsSortMode.None))
            {
                if (UnityEngine.Random.value < LuckModeChance && !item.IsLuckIngredient)
                {
                    item.SetLuckIngredient();
                }
            }
        }

        LuckModeEnabled = true;
        return true;
    }

    public bool EnableIncomeMode(bool reward = false, bool isGift = false)
    {
        if (IncomeRewardShowed)
            return false;

        int money = PlayerPrefs.GetInt(SaveData.MoneyKey);
        bool enableIncomeMode = false;

        if (isGift)
        {
            enableIncomeMode = true;
        }
        else if (money >= IncomeModePrice || reward)
        {
            if (money >= IncomeModePrice)
                UpdateMoney(money - IncomeModePrice);

            enableIncomeMode = true;
        }
        else
        {
            YandexGame.RewVideoShow(SaveData.IncomeReward);
            IncomeRewardShowed = true;

            return false;
        }

        if (enableIncomeMode)
        {
            IncomeMultiply += 0.05f;

            IncomeLevel++;
            IncomeModePrice += 200;
            UIController.UpdateIncomeAndLuckText();
            SaveProgress(SceneManager.GetActiveScene().buildIndex);
            IncomeModeEnabled = true;
            return true;
        }

        return false;
    }

    public void EnableBonusLevel()
    {
        UIController.HideBonusPanel();
        BonusLevelEnabled = true;
        LaunchGame();
    }

    public void SkipBonusLevelLevel()
    {
        UIController.HideBonusPanel();
        ChangeBlockSkin(CurrentBlockSkinID);

        BonusLevelEnabled = false;

        //_roadMaterialOne.color = _roadColorDefaultOne;
        //_roadMaterialTwo.color = _roadColorDefaultTwo;

        foreach (var item in FindObjectsByType<Ingredient>(FindObjectsSortMode.None))
        {
            if (item.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.material.color = new Color(108f / 255f, 108f / 255f, 108f / 255f);

            }
        }

        foreach (var item in FindObjectsByType<Trap>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            item.gameObject.SetActive(true);
        }

        UIController.SetStartPanelAfterBonusGame();
        //LaunchGame();
    }

    public void ChangeSkin(int skinID)
    {
        foreach (var item in Skins)
        {
            if (item.SkinID == skinID)
            {
                Player.BurgerDown.GetComponent<MeshFilter>().mesh = item.BurgerDownMesh;
                Player.BurgerDown.GetComponent<MeshRenderer>().materials = item.BurgerDownMaterial;

                Player.BurgerTop.GetComponent<MeshFilter>().mesh = item.BurgerTopMesh;
                Player.BurgerTop.GetComponent<MeshRenderer>().materials = item.BurgerTopMaterial;

                CurrentSkinID = skinID;
                //SaveProgress();

                print("Change Skin");
            }
        }
    }

    public void ChangeHeadSkin(int skinID)
    {
        var headControllers = FindObjectsOfType<HeadController>(true);

        for (int i = 0; i < headControllers.Length; i++)
        {
            headControllers[i].gameObject.SetActive(false);

            if (headControllers[i].HeadSkinData.SkinID == skinID)
            {
                headControllers[i].gameObject.SetActive(true);
                CurrentHeadSkinID = skinID;
                HeadController = headControllers[i];

                //SaveProgress();
            }
        }
    }

    public void ChangeBlockSkin(int skinID)
    {
        foreach (var item in BlockSkins)
        {
            if (item.SkinID == skinID)
            {
                _roadMaterialOne.color = item.One;
                _roadMaterialTwo.color = item.Two;

                CurrentBlockSkinID = skinID;
                //SaveProgress();
                break;
            }
        }
    }

    public void UpdateMusic(float newVolume)
    {
        _musicSource.volume = newVolume;
    }

    private void Rewarded(int id)
    {
        if (id == SaveData.LuckReward)
        {
            EnableLuckMode(reward: true);
            LuckLevel++;
            LuckModePrice += 200;
            UIController.UpdateIncomeAndLuckText();

        }

        else if (id == SaveData.IncomeReward)
        {
            EnableIncomeMode(reward: true);
            IncomeLevel++;
            IncomeModePrice += 200;
            UIController.UpdateIncomeAndLuckText();
        }

        else if (id == SaveData.BonusLevelReward)
            EnableBonusLevel();

        else if (id == SaveData.FortuneWheelReward)
        {
            PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) + 1);
        }

        else if (id == SaveData.MultiplierWheelReward)
        {
            UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) + PlayerPrefs.GetInt(SaveData.MultiplierMoneyKey));
            UIController.MultiplierUsed = true;
            UIController.UIFinal.SetEarnedMoneyText(PlayerPrefs.GetInt(SaveData.MultiplierMoneyKey).ToString());
            UIController.UIFinal.PlayThird();
        }

        else if (id == SaveData.SkinFillReward)
        {
            PurchasedSkins.Add(PlayerPrefs.GetInt(SaveData.SkinFillKey));
            UIController.UIFinal.LoadNextLevel();
        }

        else if (id == SaveData.SkipLevel)
        {
            UIController.UIFinal.LoadNextLevel();
        }
        else if (id == SaveData.BoostSkin)
        {
            UIController.UIFinal.AddSkinFill(false);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (_musicSource == null) return;

        if (!hasFocus)
        {
            _musicSource.Pause();
        }
        else
        {
            _musicSource.UnPause();
        }
    }

    public void ProposeBonusLevel()
    {
        UIController.ProposeBonusLevel();

        _roadMaterialOne.color = _roadColorBonusLevelOne;
        _roadMaterialTwo.color = _roadColorBonusLevelTwo;

        foreach (var item in FindObjectsByType<Ingredient>(FindObjectsSortMode.None))
        {
            if (item.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.material.color = Color.yellow;
            }
        }

        foreach (var item in FindObjectsByType<Trap>(FindObjectsSortMode.None))
        {
            item.gameObject.SetActive(false);
        }
    }

    public void RefreshLuckModeChance(int luckLevel)
    {
        float increment = 0f;

        if (luckLevel <= 1)
        {
            increment = 0f;
        }
        else if (luckLevel <= 10)
        {
            increment = 0.01f;
        }
        else if (luckLevel <= 20)
        {
            increment = 0.008f;
        }
        else if (luckLevel <= 50)
        {
            increment = 0.005f;
        }
        else
        {
            increment = 0.001f;
        }

        if (LuckModeEnabled)
            increment *= LuckModeBonus;

        LuckModeChance += increment;

        // Ограничиваем до 20%
        LuckModeChance = Mathf.Min(LuckModeChance, 0.2f);

    }

    private void OnEnable()
    {
        //YandexGame.GetDataEvent += GetLoad;
        YandexGame.RewardVideoEvent += Rewarded;
    }

    private void OnDisable()
    {
        //YandexGame.GetDataEvent -= GetLoad;
        YandexGame.RewardVideoEvent -= Rewarded;
    }

    public void GetLoad()
    {
        // Загружаем деньги
        PlayerPrefs.SetInt(SaveData.MoneyKey, YandexGame.savesData.money);

        // Загружаем скины и прогресс
        CurrentSkinID = YandexGame.savesData.currentSkinID;
        PurchasedSkins = YandexGame.savesData.purchasedSkins;

        CurrentHeadSkinID = YandexGame.savesData.currentHeadSkinID;
        PurchasedHeadSkins = YandexGame.savesData.purchasedHeadSkins;

        CurrentBlockSkinID = YandexGame.savesData.currentBlockSkinID;
        PurchasedBlockSkins = YandexGame.savesData.purchasedBlockSkins;

        PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, YandexGame.savesData.fortuneSpin);

        //LevelIndex = YandexGame.savesData.levelIndex;
        IncomeLevel = YandexGame.savesData.incomeLevel;
        LuckLevel = YandexGame.savesData.luckLevel;
        LuckModePrice *= LuckModePrice;
        IncomeModePrice *= IncomeLevel;
        if (!string.IsNullOrEmpty(YandexGame.savesData.LastSavedDate))
            PlayerPrefs.SetString(SaveData.LastSavedDateKey, YandexGame.savesData.LastSavedDate);

        PlayerPrefs.SetInt(SaveData.LastSavedStreakKey, YandexGame.savesData.LastSavedStreak);

        Debug.Log("Loading progress...");

        //UIController.UpdateMoneyText(PlayerPrefs.GetInt(SaveData.MoneyKey, 0));
        //UIController.UpdateIncomeAndLuckText();
    }

    public void SaveProgress(int levelIndex)
    {
        // Сохраняем деньги
        YandexGame.savesData.money = PlayerPrefs.GetInt(SaveData.MoneyKey, 0);

        // Сохраняем скины и прогресс
        YandexGame.savesData.currentSkinID = CurrentSkinID;
        YandexGame.savesData.purchasedSkins = PurchasedSkins;

        YandexGame.savesData.currentHeadSkinID = CurrentHeadSkinID;
        YandexGame.savesData.purchasedHeadSkins = PurchasedHeadSkins;

        YandexGame.savesData.currentBlockSkinID = CurrentBlockSkinID;
        YandexGame.savesData.purchasedBlockSkins = PurchasedBlockSkins;

        YandexGame.savesData.levelIndex = levelIndex;
        YandexGame.savesData.incomeLevel = IncomeLevel;
        YandexGame.savesData.luckLevel = LuckLevel;

        YandexGame.savesData.fortuneSpin = PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey);

        YandexGame.savesData.LastSavedDate = PlayerPrefs.GetString(SaveData.LastSavedDateKey, "");
        YandexGame.savesData.LastSavedStreak = PlayerPrefs.GetInt(SaveData.LastSavedStreakKey, 0);

        Debug.Log("Saving progress...");

        YandexGame.SaveProgress();
    }

    public MoneyBreakdown CalculateMoney()
    {
        int correctRecipeBase = 0;
        int recipeExtra = 0;
        int wrongIngredientPenalty = 0;
        int badIngredientPenalty = 0;
        int luckIngredientBonus = 0;

        var finalIngredients = GameManager.Instance.FinalIngredients;
        var grouped = finalIngredients.GroupBy(i => Regex.Replace(i.name, @"\s*\(\d+\)$", ""));

        HashSet<Sprite> usedIcons = new HashSet<Sprite>();
        List<Sprite> recipeIcons = GameManager.Instance.Recipe.Select(r => r.Icon).ToList();

        foreach (var group in grouped)
        {
            var ing = group.First();
            int count = group.Count();
            bool inRecipe = recipeIcons.Contains(ing.Icon);

            if (inRecipe)
            {
                if (!usedIcons.Contains(ing.Icon))
                {
                    correctRecipeBase += 10; // Первый раз этот ингредиент — +10
                    usedIcons.Add(ing.Icon);
                    if (count > 1)
                        recipeExtra += (count - 1); // Повторы — по +1
                }
                else
                {
                    recipeExtra += count; // если повторный group, все идут как +1
                }
            }
            else
            {
                wrongIngredientPenalty -= count; // –1 за каждый не тот продукт
                if (ing.IsBadIngredient)
                    badIngredientPenalty -= count; // –2 за каждый плохой
            }

            // ✅ Добавляем бонус за удачные ингредиенты
            if (ing.IsLuckIngredient)
            {
                luckIngredientBonus += 20 * count;
            }
        }

        bool isBurgerFullyCompleted = recipeIcons.All(icon => usedIcons.Contains(icon));
        int fullBurgerBonus = isBurgerFullyCompleted ? 10 : 0;

        int baseMoney = correctRecipeBase + recipeExtra + fullBurgerBonus +
                        wrongIngredientPenalty + (badIngredientPenalty * 2) +
                        luckIngredientBonus;  // ✅ прибавляем бонус

        baseMoney = Math.Max(baseMoney, 15); // Минимум 15

        int totalMoneyToAdd = baseMoney;
        int incomeBonus = 0, luckBonus = 0, bonusLevel = 0;

        if (GameManager.Instance.IncomeModeEnabled)
        {
            int newTotal = (int)Math.Round(totalMoneyToAdd * GameManager.Instance.IncomeMultiply);
            incomeBonus = newTotal - totalMoneyToAdd;
            totalMoneyToAdd = newTotal;
        }

        if (GameManager.Instance.LuckModeEnabled)
        {
            luckBonus = (int)Math.Round(totalMoneyToAdd * GameManager.Instance.LuckMultiply);
            totalMoneyToAdd += luckBonus;
        }

        if (GameManager.Instance.BonusLevelEnabled)
        {
            bonusLevel = (int)Math.Round(totalMoneyToAdd * GameManager.Instance.BonusLevelMultiply);
            totalMoneyToAdd += bonusLevel;
        }

        return new MoneyBreakdown(correctRecipeBase + recipeExtra,
                                  badIngredientPenalty * 2,
                                  fullBurgerBonus,
                                  baseMoney,
                                  incomeBonus,
                                  luckBonus,
                                  bonusLevel,
                                  totalMoneyToAdd);
    }

    public Language GetLanguageFromYandex()
    {
        switch (YandexGame.lang)
        {
            case "ru":
                return Language.Russian;
            case "tr":
                return Language.Turkish;
            case "en":
                return Language.English;
            case "es":
                return Language.Spanish;
            case "de":
                return Language.German;
            default:
                return Language.Russian;
        }
    }
}

public readonly struct MoneyBreakdown
{
    public int AddedGood { get; }
    public int AddedBad { get; }
    public int AddedRecipe { get; }
    public int BaseMoney { get; }
    public int IncomeBonus { get; }
    public int LuckBonus { get; }
    public int BonusLevel { get; }
    public int Total { get; }

    public MoneyBreakdown(int addedGood, int addedBad, int addedRecipe, int baseMoney, int incomeBonus, int luckBonus, int bonusLevel, int total)
    {
        AddedGood = addedGood;
        AddedBad = addedBad;
        AddedRecipe = addedRecipe;
        BaseMoney = baseMoney;
        IncomeBonus = incomeBonus;
        LuckBonus = luckBonus;
        BonusLevel = bonusLevel;
        Total = total;
    }
}

public static class SaveData //PlayerPrefs
{
    public static string MoneyKey { get; private set; } = "Money";
    public static string LastLevelKey { get; private set; } = "LastLevel";

    public static string MusicKey { get; private set; } = "Music";
    public static string SoundKey { get; private set; } = "Sound";
    public static string MouseSensitivityKey { get; private set; } = "MouseSensitivity";
    public static string KeyboardSensitivityKey { get; private set; } = "KeyboardSensitivity";

    public static string LastSavedDateKey { get; private set; } = "LastSavedDate";
    public static string LastSavedStreakKey { get; private set; } = "LastSavedStreak";

    public static string SkinIdKey { get; private set; } = "SkinID";

    public static string LuckLevelKey { get; private set; } = "LuckLevel";

    public static string LevelCount { get; private set; } = "LevelCount";

    public static string MultiplierMoneyKey { get; private set; } = "MultiplierMoney";

    public static string SkinFillPercentKey { get; private set; } = "SkinFillPercent";
    public static string SkinFillKey { get; private set; } = "SkinFillReady";

    public static string LastIndexKey { get; private set; } = "LastSkinFillIndex";

    public static string FortuneWheelSpineKey { get; private set; } = "FortuneWheelSpine";
    public static string FortuneWheelLevel3Key { get; private set; } = "FortuneWheelLevel3";
    public static string FortuneWheelLevel5Key { get; private set; } = "FortuneWheelLevel5";
    public static string FortuneWheelLevel10Key { get; private set; } = "FortuneWheelLevel10";


    public static int LuckReward { get; private set; } = 100;
    public static int IncomeReward { get; private set; } = 101;
    public static int BonusLevelReward { get; private set; } = 102;
    public static int FortuneWheelReward { get; private set; } = 103;
    public static int MultiplierWheelReward { get; private set; } = 104;
    public static int SkinFillReward { get; private set; } = 105;
    public static int SkipLevel { get; private set; } = 106;
    public static int BoostSkin { get; private set; } = 107;
}

public enum Language
{
    Russian,
    Turkish,
    English,
    Spanish,
    German
}
