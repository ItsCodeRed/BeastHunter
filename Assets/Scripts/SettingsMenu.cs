using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public Slider slider;
    public Slider slider2;

    public AudioMixer mixer;
    public AudioMixer mixer2;

    public void SetLevel(AudioMixer mix, float sliderValue)
    {
        mix.SetFloat("volume", Mathf.Log10(sliderValue) * 20);
    }

    private void Start()
    {
        if (PlayerPrefs.GetFloat(mixer.name) > 0)
        {
            slider.value = PlayerPrefs.GetFloat(mixer.name);
            SetLevel(mixer, PlayerPrefs.GetFloat(mixer.name));
        }
        if (PlayerPrefs.GetFloat(mixer2.name) > 0)
        {
            slider2.value = PlayerPrefs.GetFloat(mixer2.name);
            SetLevel(mixer2, PlayerPrefs.GetFloat(mixer2.name));
        }

        gameObject.SetActive(false);
    }
}
