using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class MasterMixer : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private AudioMixer mixer;

    [SerializeField] string MasterVol = "MasterVolume";
    [SerializeField] string MusicVol = "MusicVolume";
    [SerializeField] string SfxVol = "SFXVolume";
    [SerializeField] string PlayerVol = "PlayerVolume";

    private SliderInt masterVolume;
    private SliderInt musicVolume;
    private SliderInt sfxVolume;
    private SliderInt playerVolume;

    private Toggle muteToggle;
    private Toggle musicToggle;
    private Toggle sfxToggle;
    private Toggle playerToggle;

    private int savedMaster = 80;
    private int savedMusic = 80;
    private int savedSfx = 80;
    private int savedPlayer = 80;


    private void Awake()
    {
        var root = uiDocument.rootVisualElement;

        masterVolume = root.Q<SliderInt>("MasterVolume");
        musicVolume = root.Q<SliderInt>("MusicVolume");
        sfxVolume = root.Q<SliderInt>("SFXVolume");
        playerVolume = root.Q<SliderInt>("PlayerVolume");

        muteToggle = root.Q<Toggle>("Mute");
        musicToggle = root.Q<Toggle>("Music");
        sfxToggle = root.Q<Toggle>("SFX");
        playerToggle = root.Q<Toggle>("Player");


        InitChannel(masterVolume, muteToggle, MasterVol);
        InitChannel(musicVolume, musicToggle, MusicVol);
        InitChannel(sfxVolume, sfxToggle, SfxVol);
        InitChannel(playerVolume, playerToggle, PlayerVol);


        masterVolume?.RegisterValueChangedCallback(value =>
        {
            SetValue(MasterVol, value.newValue);
            if (value.newValue > 0 && muteToggle != null) muteToggle.SetValueWithoutNotify(false);
            savedMaster = value.newValue > 0 ? value.newValue : savedMaster;
        });

        musicVolume?.RegisterValueChangedCallback(value =>
        {
            SetValue(MusicVol, value.newValue);

            if (value.newValue > 0 && musicToggle != null) musicToggle.SetValueWithoutNotify(false);
            savedMusic = value.newValue > 0 ? value.newValue : savedMusic;
        });

        sfxVolume?.RegisterValueChangedCallback(value =>
        {
            SetValue(SfxVol, value.newValue);
            if (value.newValue > 0 && sfxToggle != null) sfxToggle.SetValueWithoutNotify(false);
            savedSfx = value.newValue > 0 ? value.newValue : savedSfx;
        });


        playerVolume?.RegisterValueChangedCallback(value =>
        {
            SetValue(PlayerVol, value.newValue);
            if (value.newValue > 0 && playerToggle != null) playerToggle.SetValueWithoutNotify(false);
            savedPlayer = value.newValue > 0 ? value.newValue : savedPlayer;

        });



        muteToggle?.RegisterValueChangedCallback(value => ToggleMute(value.newValue, masterVolume, MasterVol, ref savedMaster));


        musicToggle?.RegisterValueChangedCallback(value => ToggleMute(value.newValue, musicVolume, MusicVol, ref savedMusic));

        sfxToggle?.RegisterValueChangedCallback(value => ToggleMute(value.newValue, sfxVolume, SfxVol, ref savedSfx));

        playerToggle?.RegisterValueChangedCallback(value => ToggleMute(value.newValue, playerVolume, PlayerVol, ref savedPlayer));
    }

    // Set slider from current mixer value, set toggle if effectively muted
    private void InitChannel(SliderInt slider, Toggle toggle, string param)
    {
        if (slider == null) return;

        if (mixer.GetFloat(param, out float dB))
        {
            int volume = dB <= -79.9f ? 0 : Mathf.Clamp(Mathf.RoundToInt(Mathf.Pow(10f, dB / 20f) * 100f), 0, 100);
            slider.SetValueWithoutNotify(volume);
            if (toggle != null) toggle.SetValueWithoutNotify(volume == 0);
        }
        else
        {
            // If mixer has no value yet, push from slider to mixer
            SetValue(param, slider.value);
            if (toggle != null) toggle.SetValueWithoutNotify(slider.value == 0);
        }
    }

    private void ToggleMute(bool isOn, SliderInt slider, string param, ref int saved)
    {
        if (slider == null) return;

        if (isOn)
        {
            // Save current (if > 0), then mute
            if (slider.value > 0) saved = slider.value;
            slider.SetValueWithoutNotify(0);
            SetValue(param, 0);
        }
        else
        {
            // Unmute to saved value (fallback to 100 if somehow 0)
            int restore = Mathf.Clamp(saved == 0 ? 100 : saved, 0, 100);
            slider.SetValueWithoutNotify(restore);
            SetValue(param, restore);
        }
    }

    // Map [0..100] -> dB and push to mixer
    private void SetValue(string param, int sliderValue)
    {
        float v01 = Mathf.Clamp01(sliderValue / 100f);
        float dB = (v01 <= 0.0001f) ? -80f : Mathf.Log10(v01) * 20f;
        mixer.SetFloat(param, dB);
    }
}
