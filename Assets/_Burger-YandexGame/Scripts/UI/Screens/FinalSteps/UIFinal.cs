using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class UIFinal : MonoBehaviour, IPointerClickHandler
{
    [Header("Main panels")]
    [SerializeField] private CanvasGroup _alphaFade;
    [SerializeField] private RectTransform _topPanel;

    [Header("Text & typing")]
    [SerializeField] private TMP_Text _foodText;
    [SerializeField, Range(0.01f, .2f)] private float _typingCharDelay = 0.04f;

    [Header("Ingredients")]
    [SerializeField] private Transform _ingredientPanel;
    [SerializeField] private GameObject _ingredientItemPrefab;

    [Header("Final Panels")]
    [SerializeField] private GameObject _finalPanel;
    [SerializeField] private GameObject _firstPanel;
    [SerializeField] private GameObject _secondPanel;
    [SerializeField] private GameObject _thirdPanel;
    [SerializeField] private GameObject _failPanel;

    [Header("Skin fill")]
    [SerializeField] private Image _skinFillImage;
    [SerializeField] private Image _skinFillBackgroundImage;
    [SerializeField] private TMP_Text _skinFillText;

    [SerializeField] private TMP_Text _multiplierText;
    [SerializeField] private TMP_Text _earnedMoneyText;

    [SerializeField] private Button _skipButton;
    [SerializeField] private Transform _fillContainer;

    [SerializeField] private GameObject[] _fillButton;
    [SerializeField] private GameObject _boostButton;

    private Sequence _sequence;
    private bool _skipRequested;
    private Vector2 _topPanelStartPos;

    private Sequence _pulseSequence;
    private Tween _boostPulseTween;
    private bool _boostUsed = false;

    [SerializeField] private AudioClip _win;
    [SerializeField] private AudioClip _star;
    [SerializeField] private AudioClip _fail;

    private AudioSource _audioSource;

    public void SetSkinFill(float pct)
    {
        pct = Mathf.Clamp01(pct);

        _skinFillImage.fillAmount = 0f;
        _skinFillText.text = "0%";

        _pulseSequence?.Kill();
        _boostPulseTween?.Kill();

        _pulseSequence = null;
        _boostPulseTween = null;

        DOTween.To(() => 0f, x =>
        {
            _skinFillImage.fillAmount = x;
            _skinFillText.text = $"{Mathf.RoundToInt(x * 100)}%";
        }, pct, 0.8f)
        .SetEase(Ease.OutCubic)
        .OnComplete(() =>
        {
            if (_skinFillImage.fillAmount == 1f)
            {
                Vector3 vector3 = new Vector3(0, 130, 0);
                _fillContainer.DOLocalMove(vector3, 0.3f)
                              .OnComplete(() =>
                              {
                                  foreach (var item in _fillButton)
                                      item.SetActive(true);
                              });
            }
            else
            {
                _fillButton[1].SetActive(true);

                if (!_boostUsed)
                {
                    _boostButton.SetActive(true);
                    StartSynchronizedPulse();
                }
            }
        });
    }

    private string GetUpgradeText(int percent)
    {
        switch (GameManager.Instance.GetLanguageFromYandex())
        {
            case Language.Russian:
                return $"Улучшить до {percent}%";
            case Language.Turkish:
                return $"Geliştir: %{percent}";
            case Language.English:
                return $"Upgrade to {percent}%";
            case Language.Spanish:
                return $"Mejorar a {percent}%";
            case Language.German:
                return $"Verbessern auf {percent}%";
            default:
                return $"Upgrade to {percent}%";
        }
    }

    private void StartSynchronizedPulse()
    {
        float original = _skinFillImage.fillAmount;
        float target = Mathf.Min(original + 0.34f, 1f);

        _pulseSequence?.Kill();
        _boostPulseTween?.Kill();

        // Один цикл туда и обратно — 1.2с (0.6 + 0.6)
        float pulseDuration = 0.6f;

        _pulseSequence = DOTween.Sequence();

        // Прямой ход
        _pulseSequence.Append(DOTween.To(() => original, x =>
        {
            _skinFillImage.fillAmount = x;

            int displayPct = Mathf.RoundToInt(x * 100);
            //_skinFillText.text = $"{displayPct}%";

            // Обновление текста в кнопке
            int upgradeTo = Mathf.RoundToInt(Mathf.Min(x + 0.34f, 1f) * 100);
            _boostButton.GetComponentInChildren<TextMeshProUGUI>().text = GetUpgradeText(displayPct);

        }, target, pulseDuration).SetEase(Ease.InOutSine))

        // Обратный ход
        .Append(DOTween.To(() => target, x =>
        {
            _skinFillImage.fillAmount = x;

            int displayPct = Mathf.RoundToInt(x * 100);
            //_skinFillText.text = $"{displayPct}%";

            int upgradeTo = Mathf.RoundToInt(Mathf.Min(x + 0.34f, 1f) * 100);
            _boostButton.GetComponentInChildren<TextMeshProUGUI>().text = GetUpgradeText(displayPct);
        }, original, pulseDuration).SetEase(Ease.InOutSine))

        .SetLoops(-1);

        // Пульсация кнопки (тот же ритм)
        _boostPulseTween = _boostButton.transform
            .DOScale(1.1f, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void Awake()
    {
        _topPanelStartPos = _topPanel.anchoredPosition;
        _audioSource = GetComponent<AudioSource>();
    }

    private IEnumerator DelayedPlayRoutine()
    {
        yield return new WaitForSeconds(2f);

        _skipRequested = false;

        _finalPanel.SetActive(true);
        _firstPanel.SetActive(true);
        _secondPanel.SetActive(false);
        _thirdPanel.SetActive(false);

        PrepareInitialState();
        BuildSequence();

        _sequence.AppendInterval(0.5f);

        _sequence.OnComplete(PlaySecond);
        _sequence.Play();

    }

    public void Play()
    {
        _audioSource.volume = PlayerPrefs.GetFloat(SaveData.SoundKey, 0.3f);
        _audioSource.PlayOneShot(_win);

        _skipRequested = false;

        // ✅ Получаем количество звёзд
        int stars = 1;
        var starSystem = FindObjectOfType<StarSystem>();
        starSystem.StartAnim();
        if (starSystem != null)
        {
            stars = starSystem.GetStarsCount();
        }

        // ✅ Передаем в анимацию
        print("Stars" + stars);
        GameManager.Instance.HeadController.PlayAngryAnimation(stars);

        if (GameManager.Instance.FinalIngredients.Count > 0)
        {
            StartCoroutine(DelayedPlayRoutine());
        }
        else
        {
            _thirdPanel.SetActive(false);
            ShowFailPanel();
            //_failPanel.SetActive(true);
            // _finalPanel.SetActive(true);

            // PrepareInitialState();

            // _sequence = DOTween.Sequence().Pause().SetAutoKill(false);

            // _sequence.Append(_alphaFade.DOFade(1f, 0.35f));
            // _sequence.Append(_topPanel.DOAnchorPos(_topPanelStartPos, 0.5f).SetEase(Ease.OutBack, 1.1f));

            // string title = CalculateBurgerTitle();

            // _sequence.AppendCallback(() =>
            // {
            //     _skipRequested = false;
            //     StartCoroutine(TypeRoutine(title));
            // });

            // _sequence.AppendInterval(0.5f);

            // _sequence.Play();
        }
    }

    public void OnPointerClick(PointerEventData _)
    {
        if (_sequence == null || !_sequence.IsPlaying()) return;

        // Защита от повторных кликов на первой панели
        if (_firstPanel.activeSelf)
        {
            // Пропускаем только если еще не запрашивали пропуск
            if (!_skipRequested)
            {
                _skipRequested = true;
            }
        }
        else if (_secondPanel.activeSelf)
        {
            _sequence.Complete(true);
        }
    }

    private void PrepareInitialState()
    {
        _alphaFade.alpha = 0f;

        _topPanel.anchoredPosition = _topPanelStartPos + Vector2.up * _topPanel.rect.height;

        _foodText.text = string.Empty;
        _foodText.maxVisibleCharacters = 0;

        foreach (Transform c in _ingredientPanel)
            Destroy(c.gameObject);
    }

    private void BuildSequence()
    {
        _sequence = DOTween.Sequence().Pause().SetAutoKill(false);

        _sequence.Append(_alphaFade.DOFade(1f, 0.35f));
        _sequence.Append(_topPanel.DOAnchorPos(_topPanelStartPos, 0.5f).SetEase(Ease.OutBack, 1.1f));

        string title = CalculateBurgerTitle();

        // ✅ FIX: сбрасываем _skipRequested перед запуском текста
        _sequence.AppendCallback(() =>
        {
            _skipRequested = false;
            StartCoroutine(TypeRoutine(title));
        });

        _sequence.AppendInterval(title.Length * _typingCharDelay);

        int ingGroups = SpawnIngredients();
        float ingDelay = Mathf.Max(0, ingGroups - 1) * 0.05f + 0.35f;
        _sequence.AppendInterval(ingDelay);
    }

    public void SetEarnedMoneyText(string text) => _earnedMoneyText.text = "$" + text;

    private void PlaySecond()
    {
        _skipRequested = false;

        _firstPanel.SetActive(false);
        _secondPanel.SetActive(true);

        SetEarnedMoneyText(GameManager.Instance.CalculateMoney().Total.ToString());

        _sequence = DOTween.Sequence().SetAutoKill(false);

        foreach (Transform child in _ingredientPanel)
        {
            if (!child.gameObject.activeSelf)
                continue;

            var cg = child.GetComponent<CanvasGroup>()
                     ?? child.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            _sequence.Join(cg.DOFade(0f, 0.35f));
            _sequence.Join(child.DOScale(0f, 0.35f));
        }

        string levelText;
        string completeText;

        switch (GameManager.Instance.GetLanguageFromYandex())
        {
            case Language.Russian:
                levelText = "УРОВЕНЬ";
                completeText = "ЗАВЕРШЕН";
                break;
            case Language.Turkish:
                levelText = "SEVİYE";
                completeText = "TAMAMLANDI";
                break;
            case Language.Spanish:
                levelText = "NIVEL";
                completeText = "COMPLETADO";
                break;
            case Language.German:
                levelText = "LEVEL"; // или "STUFE"
                completeText = "ABGESCHLOSSEN";
                break;
            case Language.English:
            default:
                levelText = "LEVEL";
                completeText = "COMPLETE";
                break;
        }

        string doneText = $"{levelText} {GameManager.Instance.LevelIndex + 1}\n{completeText}";

        // ✅ FIX
        _sequence.AppendCallback(() =>
        {
            _skipRequested = false;
            StartCoroutine(TypeRoutine(doneText));
        });

        _sequence.AppendInterval(doneText.Length * _typingCharDelay);

        _sequence.AppendInterval(2f);
        _sequence.AppendCallback(() =>
        {
            _skipButton.gameObject.SetActive(true);
        });

        _sequence.Play();
    }

    public void SetMultiplierText(string text) => _multiplierText.text = "$" + text;

    public void PlayThird()
    {
        print("Wtf");
        _secondPanel.SetActive(false);
        _topPanel.gameObject.SetActive(false);
        _thirdPanel.SetActive(true);

        _skinFillImage.sprite = GameManager.Instance.SkinSprites[PlayerPrefs.GetInt(SaveData.SkinFillKey)];
        _skinFillBackgroundImage.sprite = GameManager.Instance.SkinSprites[PlayerPrefs.GetInt(SaveData.SkinFillKey)];
        _skinFillImage.fillAmount = PlayerPrefs.GetFloat(SaveData.SkinFillPercentKey);

        if (_skinFillImage.fillAmount == 1f || _skinFillImage.fillAmount == 0 || GameManager.Instance.PurchasedSkins.Contains(PlayerPrefs.GetInt(SaveData.SkinFillKey)))
            ChooseFillSkin();

        AddSkinFill();

    }

    public void ShowFailPanel()
    {
        _audioSource.volume = PlayerPrefs.GetFloat(SaveData.SoundKey, 0.3f);
        _audioSource.PlayOneShot(_fail);

        _finalPanel.SetActive(true);
        _failPanel.SetActive(true);

        var cg = _failPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = _failPanel.AddComponent<CanvasGroup>();

        var rt = _failPanel.GetComponent<RectTransform>();
        cg.alpha = 0f;
        rt.localScale = Vector3.zero;

        DOTween.Kill(_failPanel);
        Sequence seq = DOTween.Sequence().SetId(_failPanel);

        // Анимация самого _failPanel
        seq.Append(cg.DOFade(1f, 0.3f));
        seq.Join(rt.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack));

        // Анимация дочерних элементов
        Transform title = _failPanel.transform.Find("Title");
        Transform retryBtn = _failPanel.transform.Find("RetryButton");
        Transform menuBtn = _failPanel.transform.Find("MenuButton");

        if (title != null) seq.Append(AnimateElement(title));
        if (retryBtn != null) seq.Append(AnimateElement(retryBtn));
        if (menuBtn != null) seq.Append(AnimateElement(menuBtn));
    }

    private Tween AnimateElement(Transform element)
    {
        var cg = element.GetComponent<CanvasGroup>();
        if (cg == null) cg = element.gameObject.AddComponent<CanvasGroup>();

        element.localScale = Vector3.zero;
        cg.alpha = 0f;

        Sequence s = DOTween.Sequence();
        s.Append(cg.DOFade(1f, 0.2f));
        s.Join(element.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));

        return s;
    }

    public void AddSkinReward()
    {
        YandexGame.RewVideoShow(SaveData.BoostSkin);
    }

    public void AddSkinFill(bool disableButton = true)
    {
        if (!disableButton)
        {
            _boostUsed = true;                // <== флаг устанавливается
            _boostButton.SetActive(false);    // скрываем кнопку
        }
        else
        {
            _boostButton.SetActive(true);
        }

        float current = PlayerPrefs.GetFloat(SaveData.SkinFillPercentKey);
        float target;

        if (!disableButton)
        {
            string buttonText = _boostButton.GetComponentInChildren<TextMeshProUGUI>().text;
            int targetPercent = ExtractPercentFromText(buttonText);
            target = Mathf.Clamp01(targetPercent / 100f);
        }
        else
        {
            float addValue = 0.34f;
            target = Mathf.Clamp01(current + addValue);
        }

        PlayerPrefs.SetFloat(SaveData.SkinFillPercentKey, target);
        SetSkinFill(target);
    }

    private int ExtractPercentFromText(string text)
    {
        // Ищем первое число в строке (87 из "Улучшить до 87%")
        var match = Regex.Match(text, @"\d+");
        if (match.Success && int.TryParse(match.Value, out int result))
            return result;

        return 0; // Fallback: если не нашли — заполняем до конца
    }

    private IEnumerator TypeRoutine(string txt)
    {
        _foodText.text = txt;
        _foodText.maxVisibleCharacters = 0;
        _skipRequested = false; // Сбрасываем флаг при запуске новой анимации

        for (int i = 1; i <= txt.Length; i++)
        {
            // Проверяем флаг каждые 5 символов для оптимизации
            if (i % 5 == 0 && _skipRequested)
            {
                _foodText.maxVisibleCharacters = txt.Length;
                break;
            }

            _foodText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(_typingCharDelay);
        }

        // Гарантированно показываем весь текст при пропуске
        if (_skipRequested) _foodText.maxVisibleCharacters = txt.Length;
    }

    private int SpawnIngredients()
    {
        var groups = GameManager.Instance.FinalIngredients.GroupBy(g => g.Icon).OrderBy(g => g.Key.name);

        //var dgroups = GameManager.Instance.FinalIngredients[0].Icon;

        int index = 0;
        float delay = 0f;

        foreach (var group in groups)
        {
            var firstIngredient = group.First();

            GameObject itemGO = Instantiate(_ingredientItemPrefab, _ingredientPanel);
            print("Create panel");
            itemGO.transform.localScale = Vector3.zero;

            var image = itemGO.GetComponentInChildren<Image>();
            var text = itemGO.GetComponentInChildren<TMP_Text>();

            if (image != null)
                image.sprite = firstIngredient.Icon;
            if (text != null)
                text.text = "x" + group.Count().ToString();

            if (group.Any(ing => ing.IsLuckIngredient))
            {
                var luckIcon = itemGO.transform.Find("Luck");
                if (luckIcon != null)
                    luckIcon.gameObject.SetActive(true);
            }

            itemGO.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetDelay(delay);

            delay += 0.05f;
            index++;
        }

        return index;
    }

    private readonly Dictionary<Language, Dictionary<string, string>> _localizedAdjectives = new()
    {
        [Language.Russian] = new()
    {
        { "Anchovy",       "Анчоусный"     },
        { "Arugula Leaf",  "Рукколовый"    },
        { "Avocado Slice", "Авокадовый"    },
        { "Bacon Slice",   "Беконный"      },
        { "Cheese Slice",  "Сырный"        },
        { "Cucumber",      "Огуречный"     },
        { "Cutlet B",      "Котленый"  },
        { "Egg Boiled",    "Яичный"        },
        { "Fish Slice",    "Рыбный"        },
        { "Onion Slice",   "Луковый"       },
        { "Pepper Red",    "Острый"        },
        { "Salad Slice",   "Салатный"      },
        { "Shrimp",        "Креветочный"   },
        { "Tomato Slice",  "Томатный"      },
        { "Bone",          "Костяной"      },
        { "CrushedCan",    "Мятый"         },
        { "FishBone",      "Костлявый"     },
        { "Phone",         "Звонящий"      },
        { "ToiletPaper",   "Туалетный"     }
    },
        [Language.English] = new()
    {
        { "Anchovy",       "Anchovy"       },
        { "Arugula Leaf",  "Arugula"       },
        { "Avocado Slice", "Avocado"       },
        { "Bacon Slice",   "Bacon"         },
        { "Cheese Slice",  "Cheesy"        },
        { "Cucumber",      "Cucumber"      },
        { "Cutlet B",      "Classic"       },
        { "Egg Boiled",    "Eggy"          },
        { "Fish Slice",    "Fishy"         },
        { "Onion Slice",   "Oniony"        },
        { "Pepper Red",    "Spicy"         },
        { "Salad Slice",   "Leafy"         },
        { "Shrimp",        "Shrimpy"       },
        { "Tomato Slice",  "Tomatoey"      },
        { "Bone",          "Bony"          },
        { "CrushedCan",    "Crushed"       },
        { "FishBone",      "Fishboned"     },
        { "Phone",         "Ringing"       },
        { "ToiletPaper",   "Toilet"        }
    },
        [Language.Turkish] = new()
    {
        { "Anchovy",       "Hamsili"       },
        { "Arugula Leaf",  "Rokalı"        },
        { "Avocado Slice", "Avokadolu"     },
        { "Bacon Slice",   "Pastırmalı"    },
        { "Cheese Slice",  "Peynirli"      },
        { "Cucumber",      "Salatalıklı"   },
        { "Cutlet B",      "Klasik"        },
        { "Egg Boiled",    "Yumurtalı"     },
        { "Fish Slice",    "Balıklı"       },
        { "Onion Slice",   "Soğanlı"       },
        { "Pepper Red",    "Acılı"         },
        { "Salad Slice",   "Yeşillikli"    },
        { "Shrimp",        "Karidesli"     },
        { "Tomato Slice",  "Domatesli"     },
        { "Bone",          "Kemikli"       },
        { "CrushedCan",    "Ezik"          },
        { "FishBone",      "Kılçıklı"      },
        { "Phone",         "Çalan"         },
        { "ToiletPaper",   "Tuvaletli"     }
    },
        [Language.Spanish] = new()
    {
        { "Anchovy",       "Anchoado"      },
        { "Arugula Leaf",  "De rúcula"     },
        { "Avocado Slice", "Aguacatado"    },
        { "Bacon Slice",   "Con tocino"    },
        { "Cheese Slice",  "Quesudo"       },
        { "Cucumber",      "Pepinilloso"   },
        { "Cutlet B",      "Clásico"       },
        { "Egg Boiled",    "Con huevo"     },
        { "Fish Slice",    "Pescado"       },
        { "Onion Slice",   "Cebolloso"     },
        { "Pepper Red",    "Picante"       },
        { "Salad Slice",   "Ensaladoso"    },
        { "Shrimp",        "Camaronesco"   },
        { "Tomato Slice",  "Tomatoso"      },
        { "Bone",          "Óseo"          },
        { "CrushedCan",    "Aplastado"     },
        { "FishBone",      "Espinoso"      },
        { "Phone",         "Sonante"       },
        { "ToiletPaper",   "Higiénico"     }
    },
        [Language.German] = new()
    {
        { "Anchovy",       "Sardellenhaft" },
        { "Arugula Leaf",  "Rucolaartig"   },
        { "Avocado Slice", "Avocadisch"    },
        { "Bacon Slice",   "Speckig"       },
        { "Cheese Slice",  "Käsig"         },
        { "Cucumber",      "Gurkig"        },
        { "Cutlet B",      "Klassisch"     },
        { "Egg Boiled",    "Eierig"        },
        { "Fish Slice",    "Fischig"       },
        { "Onion Slice",   "Zwieblich"     },
        { "Pepper Red",    "Scharf"        },
        { "Salad Slice",   "Salatig"       },
        { "Shrimp",        "Garnelig"      },
        { "Tomato Slice",  "Tomatig"       },
        { "Bone",          "Knochig"       },
        { "CrushedCan",    "Zerdrückt"     },
        { "FishBone",      "Grätig"        },
        { "Phone",         "Klingelnd"     },
        { "ToiletPaper",   "Toilettenhaft" }
    }
    };

    private string CalculateBurgerTitle()
    {
        var ingredients = GameManager.Instance.FinalIngredients;
        int count = ingredients.Count;

        Language lang = GameManager.Instance.GetLanguageFromYandex();

        string noIngredients;
        string sizeSmall;
        string sizeMedium;
        string sizeLarge;
        string defaultAdjective;

        switch (lang)
        {
            case Language.English:
                noIngredients = "NO INGREDIENTS";
                sizeSmall = "BURGER";
                sizeMedium = "MEGA BURGER";
                sizeLarge = "SUPER MEGA BURGER";
                defaultAdjective = "CLASSIC";
                break;
            case Language.Turkish:
                noIngredients = "MALZEME YOK";
                sizeSmall = "BURGER";
                sizeMedium = "MEGA BURGER";
                sizeLarge = "SÜPER MEGA BURGER";
                defaultAdjective = "KLASİK";
                break;
            case Language.Spanish:
                noIngredients = "SIN INGREDIENTES";
                sizeSmall = "HAMBURGUESA";
                sizeMedium = "MEGA HAMBURGUESA";
                sizeLarge = "SÚPER MEGA HAMBURGUESA";
                defaultAdjective = "CLÁSICA";
                break;
            case Language.German:
                noIngredients = "KEINE ZUTATEN";
                sizeSmall = "BURGER";
                sizeMedium = "MEGA BURGER";
                sizeLarge = "SUPER MEGA BURGER";
                defaultAdjective = "KLASSISCH";
                break;
            case Language.Russian:
            default:
                noIngredients = "НЕТ ИНГРЕДИЕНТОВ";
                sizeSmall = "БУРГЕР";
                sizeMedium = "МЕГА БУРГЕР";
                sizeLarge = "СУПЕР МЕГА БУРГЕР";
                defaultAdjective = "КЛАССИЧЕСКИЙ";
                break;
        }

        if (count == 0)
            return noIngredients;

        string size = count switch
        {
            <= 3 => sizeSmall,
            <= 15 => sizeMedium,
            _ => sizeLarge
        };

        var adjectives = new HashSet<string>();

        // Выбор словаря прилагательных по языку
        var dict = _localizedAdjectives.TryGetValue(lang, out var found) ? found : _localizedAdjectives[Language.English];

        foreach (var item in ingredients)
        {
            string iconName = item.Icon?.name;
            if (iconName != null && dict.TryGetValue(iconName, out var adj))
            {
                adjectives.Add(adj.ToUpper());
            }
        }

        if (adjectives.Count == 0)
            adjectives.Add(defaultAdjective.ToUpper());

        return $"{string.Join(" ", adjectives)} {size}";
    }

    private void ChooseFillSkin()
    {
        int[] allow = new int[] { 1, 2, 12, 5, 4, 3, 11, 10 };

        // Проверяем, есть ли уже выбранный скин, который не был забран
        int savedSkinID = PlayerPrefs.GetInt(SaveData.SkinFillKey, -1);
        float savedFill = PlayerPrefs.GetFloat(SaveData.SkinFillPercentKey, 0);

        // Если есть сохранённый скин и он ещё не куплен, продолжаем показывать его
        if (savedSkinID != -1 && !GameManager.Instance.PurchasedSkins.Contains(savedSkinID))
        {
            var existingSkin = GameManager.Instance.Skins.FirstOrDefault(s => s != null && s.SkinID == savedSkinID);
            if (existingSkin != null && existingSkin.Icon != null)
            {
                _skinFillImage.sprite = existingSkin.Icon;
                _skinFillBackgroundImage.sprite = existingSkin.Icon;
                _skinFillImage.fillAmount = savedFill;
                _skinFillText.text = $"{Mathf.RoundToInt(savedFill * 100)}%";
                return; // ✅ Выходим, чтобы не сбрасывать прогресс
            }
        }

        // --- Если скин ещё не выбран, ищем новый ---
        int startIndex = PlayerPrefs.GetInt(SaveData.LastIndexKey, 0);
        int index = startIndex;
        SkinData value = null;

        for (int tries = 0; tries < allow.Length; tries++)
        {
            int targetSkinID = allow[index];
            var skin = GameManager.Instance.Skins.FirstOrDefault(s => s != null && s.SkinID == targetSkinID);

            if (skin != null && skin.Icon != null && !GameManager.Instance.PurchasedSkins.Contains(skin.SkinID))
            {
                value = skin;
                break;
            }

            index = (index + 1) % allow.Length;
        }

        // Сохраняем индекс следующего скина
        PlayerPrefs.SetInt(SaveData.LastIndexKey, (startIndex + 1) % allow.Length);

        if (value != null)
        {
            _skinFillImage.sprite = value.Icon;
            _skinFillBackgroundImage.sprite = value.Icon;

            PlayerPrefs.SetFloat(SaveData.SkinFillPercentKey, 0);
            PlayerPrefs.SetInt(SaveData.SkinFillKey, value.SkinID);
            _skinFillImage.fillAmount = 0;
            _skinFillText.text = "0%";
        }
    }

    public void GetSkin()
    {
        //_fillButton[0].GetComponent<Button>().interactable = false;
        _fillButton[0].SetActive(false);
        _boostButton.SetActive(false);
        YandexGame.RewVideoShow(SaveData.SkinFillReward);
    }

    public void LoadNextLevel()
    {
        DOTween.KillAll();

        if (PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey, 0) == 0)
        {
            PlayerPrefs.SetInt("CurrentFortuneLevel", PlayerPrefs.GetInt("CurrentFortuneLevel", 1) + 1);

            int Level3 = PlayerPrefs.GetInt(SaveData.FortuneWheelLevel3Key, 0);
            int Level5 = PlayerPrefs.GetInt(SaveData.FortuneWheelLevel5Key, 0);

            if (PlayerPrefs.GetInt("CurrentFortuneLevel", 1) <= 3 && Level3 < 3)
            {
                PlayerPrefs.SetInt(SaveData.FortuneWheelLevel3Key, Level3 + 1);
            }
            else if (PlayerPrefs.GetInt("CurrentFortuneLevel", 1) <= 3 && Level5 < 5)
            {
                PlayerPrefs.SetInt(SaveData.FortuneWheelLevel5Key, Level5 + 1);
            }
            else
            {
                PlayerPrefs.SetInt(SaveData.FortuneWheelLevel10Key, PlayerPrefs.GetInt(SaveData.FortuneWheelLevel10Key, 1) + 1);
            }
        }
        else
        {
            //PlayerPrefs.SetInt("CurrentFortuneLevel", 0);
        }

        int i = GameManager.Instance.CalculateMoney().Total;

        GameManager.Instance.UpdateMoney(PlayerPrefs.GetInt(SaveData.MoneyKey) + i);
        GameManager.Instance.SaveProgress(SceneManager.GetActiveScene().buildIndex + 1);

        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        int lastBuildIndex = SceneManager.sceneCountInBuildSettings - 1;
        if (currentBuildIndex == lastBuildIndex)
        {
            int levelCount = PlayerPrefs.GetInt(SaveData.LevelCount, currentBuildIndex) + 1;
            PlayerPrefs.SetInt(SaveData.LevelCount, levelCount);

            SceneManager.LoadScene(currentBuildIndex);
        }
        else
        {
            SceneManager.LoadScene(currentBuildIndex + 1);
        }
    }
}
