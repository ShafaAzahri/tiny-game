using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    public static VolumeControl instance;
    public Slider volumeSlider;
    public Toggle volumeToggle; // 🆕 Tambahkan referensi ke toggle

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("volume", 1f);
        AudioListener.volume = savedVolume;

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (volumeToggle != null)
        {
            volumeToggle.onValueChanged.AddListener(OnToggleChanged);
            volumeSlider.interactable = volumeToggle.isOn; // Aktifkan atau matikan slider
        }
    }

    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("volume", value);
    }

    public void OnToggleChanged(bool isOn)
    {
        if (volumeSlider != null)
        {
            volumeSlider.interactable = isOn;
        }

        // Jika ingin mute saat toggle dimatikan:
        AudioListener.volume = isOn ? volumeSlider.value : 0f;
    }
}
