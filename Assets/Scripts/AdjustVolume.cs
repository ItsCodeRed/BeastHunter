using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AdjustVolume : MonoBehaviour
{
    public AudioMixer mixer;

    public void SetLevel(float sliderValue)
    {
        mixer.SetFloat("volume", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat(mixer.name, sliderValue);
    }
}
