using UnityEngine;

public class SettingToggle : MonoBehaviour
{
    public GameObject settingCanvas;   // Canvas yang berisi image, slider, dan tombol X
    public GameObject buttonCanvas;    // Canvas yang berisi tombol play, credit, quit, setting

    private bool isSettingOpen = false;

    public void ToggleSetting()
    {
        isSettingOpen = !isSettingOpen;
        settingCanvas.SetActive(isSettingOpen);
        buttonCanvas.SetActive(!isSettingOpen);
    }

    public void CloseSetting()
    {
        isSettingOpen = false;
        settingCanvas.SetActive(false);
        buttonCanvas.SetActive(true);
    }
}
