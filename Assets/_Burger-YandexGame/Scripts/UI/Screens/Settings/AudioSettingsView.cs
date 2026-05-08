using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsView : MonoBehaviour
{
    [SerializeField] private ImageSlider _musicSlider;
    [SerializeField] private ImageSlider _soundSlider;

    private void Start()
    {
        _musicSlider.SetValue(PlayerPrefs.GetFloat(SaveData.MusicKey, 0.3f));
        _soundSlider.SetValue(PlayerPrefs.GetFloat(SaveData.SoundKey, 0.3f));

        //SetMusic();
        //SetSound();
    }

    public void SetMusic()
    {
        PlayerPrefs.SetFloat(SaveData.MusicKey, _musicSlider.Value);
        GameManager.Instance.UpdateMusic(_musicSlider.Value);
    }

    public void SetSound()
    {
        PlayerPrefs.SetFloat(SaveData.SoundKey, _soundSlider.Value);
    }
}
