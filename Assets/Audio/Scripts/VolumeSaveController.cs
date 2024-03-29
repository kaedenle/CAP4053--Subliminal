using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeSaveController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider = null;
    [SerializeField] private TMP_Text volumeTextUI = null;

    private void Start() 
    {
        LoadValues();
        VolumeManager.instance.volumeValue = 0.5f;
        volumeSlider.value = VolumeManager.instance.volumeValue;
    }

    public void VolumeSlider(float volume)
    {
        volumeTextUI.text = volume.ToString("0.0");
        VolumeManager.instance.volumeValue = volume;
        ApplyVolumeToAudioSources();
    }

    public void SaveVolumeButton()
    {
        float volumeValue = volumeSlider.value;
        PlayerPrefs.SetFloat("VolumeValue", volumeValue);
        LoadValues();
    }

    void LoadValues()
    {
        float volumeValue = PlayerPrefs.GetFloat("VolumeValue");
        volumeSlider.value = volumeValue;
        ApplyVolumeToAudioSources();
    }

    void ApplyVolumeToAudioSources() {
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in audioSources) {
            if (audioSource.tag == "LevelTheme") {
                audioSource.volume = VolumeManager.instance.volumeValue;
            }
        }
    }
}
