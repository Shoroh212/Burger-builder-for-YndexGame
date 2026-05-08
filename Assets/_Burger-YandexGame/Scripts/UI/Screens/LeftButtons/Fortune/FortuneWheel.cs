using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FortuneWheel : MonoBehaviour
{
    [SerializeField] private GameObject _wheel;
    [SerializeField] private RectTransform _fortuneWheelButton;
    [SerializeField] private FortuneWheelDetector _fortuneWheelDetector;
    [SerializeField] private Button _spinButton;
    [SerializeField] private TMP_Text _fortuneText;
    [SerializeField] private TextMeshProUGUI tmpText;

    [SerializeField] private float _pulseScale;
    [SerializeField] private float _pulseTime;

    [SerializeField] private float _speed;
    private float _currentSpeed;

    private bool _spinStarted;
    private Tween _pulseTween;

    private void Update()
    {
        SpinFortuneWheel();

        
        if (PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) > 0)
        {
            StartPulse(_fortuneWheelButton);
        }
        else
        {
            StopPulse(_fortuneWheelButton);
        }
    }

    public void _SpinFortuneWheel()
    {
        int currentSpins = PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey);
        int sceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (sceneIndex > 2 && currentSpins > 0)
        {
            currentSpins--;
            PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, currentSpins);
            PlayerPrefs.Save();

            StartSpin();
        }
        else
        {
            int endLevel = 0;

            bool is5Lvl = false;
            bool is10Lvl = false;

            int Level5 = PlayerPrefs.GetInt(SaveData.FortuneWheelLevel5Key, 0) + 1;

            if (Level5 < 5)
                is5Lvl = true;
            else
                is10Lvl = true;

            if (is5Lvl)
                endLevel = 5;
            else if (is10Lvl)
                endLevel = 10;

            _fortuneText.text = $"0/{endLevel}";
            _spinButton.interactable = false;
            _spinButton.GetComponentInChildren<TMP_Text>().color = new Color(1, 1, 1, 0.5f);
        }
    }

    private void SpinFortuneWheel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex >= 2) // Óğîâíè 2 è âûøå
        {
            if (_currentSpeed > 0 && _spinStarted)
            {
                _wheel.transform.Rotate(_wheel.transform.forward * -_currentSpeed);
                _currentSpeed -= Time.deltaTime;
            }
            else if (_spinStarted)
            {
                _fortuneWheelDetector.IsStoped = true;
                _spinStarted = false;

                // ÄÎÁÀÂËÅÍÀ ÏĞÎÂÅĞÊÀ ÑÏÈÍÎÂ ÏÎÑËÅ ÎÑÒÀÍÎÂÊÈ
                int remainingSpins = PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey);
                if (remainingSpins > 0)
                {
                    // ÅÑÒÜ ÑÏÈÍÛ - ÄÅËÀÅÌ ÊÍÎÏÊÓ ÀÊÒÈÂÍÎÉ
                    SetSpinButtonState(true, new Color(1, 0.09056918f, 0, 1), 1f);
                }
                else
                {
                    // ÍÅÒ ÑÏÈÍÎÂ - ÎÑÒÀÂËßÅÌ ÍÅÀÊÒÈÂÍÎÉ
                    PlayerPrefs.SetInt("CurrentFortuneLevel", 0);

                    int endLevel = 0;

                    bool is5Lvl = false;
                    bool is10Lvl = false;

                    int Level5 = PlayerPrefs.GetInt(SaveData.FortuneWheelLevel5Key, 0) + 1;

                    if (Level5 < 5)
                        is5Lvl = true;
                    else
                        is10Lvl = true;

                    if (is5Lvl)
                        endLevel = 5;
                    else if (is10Lvl)
                        endLevel = 10;

                    _fortuneText.text = $"0/{endLevel}";
                    _spinButton.interactable = false;

                    SetSpinButtonState(false, new Color(0.603f, 0.09056918f, 0, 1), 0.5f);
                }
            }
        }
        else
        {
            SetSpinButtonState(false, new Color(0.603f, 0.09056918f, 0, 1), 0.5f);
        }
    }

    private void StartSpin()
    {
        _spinStarted = true;
        _currentSpeed = _speed;
        _spinButton.interactable = false;
        _spinButton.GetComponentInChildren<TMP_Text>().color = new Color(1, 1, 1, 0.5f);

        Material matInstance = tmpText.fontMaterial;

        matInstance.SetColor("_OutlineColor", new Color(0.603f, 0.09056918f, 0, 1)); 

        tmpText.fontMaterial = matInstance;
    }

    //private void SpinFortuneWheel()
    //{
    //    int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

    //    if(currentSceneIndex >= 2) // Óğîâíè 2 è âûøå
    //    {
    //        if(_currentSpeed > 0 && _spinStarted)
    //        {
    //            _wheel.transform.Rotate(_wheel.transform.forward * -_currentSpeed);
    //            _currentSpeed -= Time.deltaTime;

    //            if(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) == 0)
    //            {
    //                SetSpinButtonState(false, new Color(0.603f, 0.09056918f, 0, 1), 0.5f);
    //            }
    //        }
    //        else if(_spinStarted)
    //        {
    //            _fortuneWheelDetector.IsStoped = true;
    //            _spinStarted = false;

    //            if(currentSceneIndex == 2) // buildIndex == 2 (òğåòèé óğîâåíü)
    //            {
    //                SetSpinButtonState(false, new Color(0.603f, 0.09056918f, 0, 1), 0.5f);
    //            }
    //            else if(PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey) == 0)
    //            {
    //                SetSpinButtonState(false, new Color(0.603f, 0.09056918f, 0, 1), 0.5f);
    //            }
    //            else
    //            {
    //                SetSpinButtonState(true, new Color(1, 0.09056918f, 0, 1), 1f);
    //            }
    //        }
    //    }
    //    else
    //    {
    //        SetSpinButtonState(false, new Color(0.603f, 0.09056918f, 0, 1), 0.5f);
    //    }
    //}

    private void SetSpinButtonState(bool interactable, Color outlineColor, float textAlpha)
    {
        _spinButton.interactable = interactable;
        TMP_Text text = _spinButton.GetComponentInChildren<TMP_Text>();
        text.color = new Color(1, 1, 1, textAlpha);

        Material matInstance = tmpText.fontMaterial;
        matInstance.SetColor("_OutlineColor", outlineColor);
        tmpText.fontMaterial = matInstance;
    }

    //public void _SpinFortuneWheel()
    //{
    //    int currentSpins = PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey);

    //    if (SceneManager.GetActiveScene().buildIndex + 1 > 2)
    //    {
    //        if (currentSpins > 0)
    //        {
    //            currentSpins--;
    //            PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, currentSpins);

    //            StartSpin();

    //            // Îáíîâëÿåì ñîñòîÿíèå êíîïêè ïîñëå èñïîëüçîâàíèÿ ñïèíà
    //            if (currentSpins == 0)
    //            {
    //                _spinButton.interactable = false;
    //                _spinButton.GetComponentInChildren<TMP_Text>().color = new Color(1, 1, 1, 0.5f);
    //            }
    //        }
    //        else
    //        {
    //            _spinButton.interactable = false;
    //            _spinButton.GetComponentInChildren<TMP_Text>().color = new Color(1, 1, 1, 0.5f);
    //        }
    //    }
    //}
    private void StartPulse(RectTransform rt)
    {
        if(_pulseTween != null && _pulseTween.IsActive())
            return;

        _pulseTween = rt.DOScale(_pulseScale, _pulseTime).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo);
    }

    private void StopPulse(RectTransform rt)
    {
        _pulseTween?.Kill();
        _pulseTween = null;
        rt.localScale = Vector3.one;
    }

    [SerializeField] private GameObject _fortunePanel;
    [SerializeField] private CanvasGroup _canvasGroup;
    public void TogglePanel(bool show)
    {
        if (_fortunePanel == null || _canvasGroup == null)
            return;

        if (show)
        {
            _fortunePanel.SetActive(true);
            _canvasGroup.gameObject.SetActive(true);

            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.Linear);

            _fortunePanel.transform.localScale = Vector3.zero;
            _fortunePanel.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack);
        }
        else
        {
            _canvasGroup.DOFade(0f, 0.25f)
                .SetEase(Ease.Linear);

            _fortunePanel.transform.DOScale(Vector3.zero, 0.25f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    _fortunePanel.SetActive(false);
                    _canvasGroup.gameObject.SetActive(false);
                });
        }
    }

}
