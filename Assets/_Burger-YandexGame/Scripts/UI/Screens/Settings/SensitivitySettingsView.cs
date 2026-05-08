using UnityEngine;
using UnityEngine.UI;

public class SensitivitySettingsView : MonoBehaviour
{
    [SerializeField] private ImageSlider _mouseSensitivitySlider;
    [SerializeField] private ImageSlider _keyboardSensitivitySlider;

    private void Start()
    {
        _mouseSensitivitySlider.SetValue(PlayerPrefs.GetFloat(SaveData.MouseSensitivityKey, 2f));
        _keyboardSensitivitySlider.SetValue(PlayerPrefs.GetFloat(SaveData.KeyboardSensitivityKey, 2f));

    }

    public void SetMouseSensitivity()
    {
        PlayerPrefs.SetFloat(SaveData.MouseSensitivityKey, _mouseSensitivitySlider.Value);
        GameManager.Instance.Player.UpdateSensitivity(_mouseSensitivitySlider.Value, true);
    }

    public void SetKeyboardSensitivity()
    {
        PlayerPrefs.SetFloat(SaveData.KeyboardSensitivityKey, _keyboardSensitivitySlider.Value);
        GameManager.Instance.Player.UpdateSensitivity(_keyboardSensitivitySlider.Value, false);
    }
}
