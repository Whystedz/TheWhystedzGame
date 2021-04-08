using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string exposedParameter;

    private void OnEnable()
    {
        this.mainMixer.GetFloat(this.exposedParameter, out var sliderValue);
        this.slider.value = Mathf.Pow(10, sliderValue / 20);
    }

    public void SetAudioLevel(float sliderValue) => this.mainMixer.SetFloat(this.exposedParameter, Mathf.Log10(sliderValue) * 20);
}
