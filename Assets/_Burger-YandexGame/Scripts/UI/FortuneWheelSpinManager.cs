using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FortuneWheelSpinManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _fortuneText;
    [SerializeField] private Button _spinButton;

    public void SetFortuneWheel()
    {
        int spins = PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey, 0);
        int startLevel = PlayerPrefs.GetInt("CurrentFortuneLevel", 0);
        int endLevel = 0;

        bool is3Lvl = false;
        bool is5Lvl = false;
        bool is10Lvl = false;

        int Level3 = PlayerPrefs.GetInt(SaveData.FortuneWheelLevel3Key, 0);
        int Level5 = PlayerPrefs.GetInt(SaveData.FortuneWheelLevel5Key, 0);

        if (Level3 < 3)
            is3Lvl = true;
        else if (Level5 < 5)
            is5Lvl = true;
        else
            is10Lvl = true;


        if (is3Lvl)
            endLevel = 3;
        else if (is5Lvl)
            endLevel = 5;
        else if (is10Lvl)
            endLevel = 10;

        if (spins > 0)
        {
            _spinButton.interactable = true;
            _fortuneText.text = $"{startLevel}/{endLevel}";
            _spinButton.GetComponentInChildren<TMP_Text>().color = new Color(1, 1, 1, 1f);

            return;
        }

        if (startLevel == endLevel)
        {
            spins = 1;
            PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, spins);
            _spinButton.interactable = true;
            _spinButton.GetComponentInChildren<TMP_Text>().color = new Color(1, 1, 1, 1f);

        }

        _fortuneText.text = $"{startLevel}/{endLevel}";
    }

    public void SetReadyFortune()
    {
        int spins = PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey, 0);
        int startLevel = PlayerPrefs.GetInt("CurrentFortuneLevel", 0);
        int endLevel = 0;

        bool is5Lvl = false;
        bool is10Lvl = false;

        int Level3 = PlayerPrefs.GetInt(SaveData.FortuneWheelLevel3Key, 0);
        int Level5 = PlayerPrefs.GetInt(SaveData.FortuneWheelLevel5Key, 0);

        if (Level5 < 5)
            is5Lvl = true;
        else
            is10Lvl = true;

        if (is5Lvl)
            endLevel = 5;
        else if (is10Lvl)
            endLevel = 10;

        _spinButton.interactable = true;
        _fortuneText.text = $"{endLevel}/{endLevel}";
        _spinButton.GetComponentInChildren<TMP_Text>().color = new Color(1, 1, 1, 1f);
    }

    private void Start()
    {
        SetFortuneWheel();
    }

    //private void SetFortuneWheel()
    //{
    //    int currentLevel = PlayerPrefs.GetInt("CurrentFortuneLevel", 1);
    //    int spins = PlayerPrefs.GetInt(SaveData.FortuneWheelSpineKey, 0);

    //    // Определяем endLevel
    //    int endLevel;
    //    if (currentLevel <= 3)
    //        endLevel = 3;
    //    else if (currentLevel <= 5)
    //        endLevel = 5;
    //    else 
    //        endLevel = 10;

    //    // Даём спин при достижении конца уровня
    //    if (currentLevel >= endLevel)
    //    {
    //        spins = 1;
    //        //PlayerPrefs.SetInt("CurrentFortuneLevel", 0);
    //        PlayerPrefs.SetInt(SaveData.FortuneWheelSpineKey, spins);
    //    }

    //    // Обновляем UI
    //    _fortuneText.text = $"{currentLevel}/{endLevel}";
    //    _spinButton.interactable = spins > 0;
    //}
}