using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

//public class MasterMixer : MonoBehaviour
//{
//    [SerializeField] private UIDocument uiDocument;

//    //[SerializeField] private AudioSource audioSource;

//    [SerializeField] private AudioMixer mixer;
//    [SerializeField] string MasterVol = "MasterVolume";
//    [SerializeField] string MusicVol = "MusicVolume";
//    [SerializeField] string SfxVol = "SFXVolume";
//    [SerializeField] float multiplier = 20f;

//    private SliderInt masterVolume;
//    private SliderInt musicVolume;
//    private SliderInt sfxVolume;
//    private Toggle muteToggle;
//    private Toggle musicToggle;
//    private Toggle sfxToggle;

//    private bool disableToggleEvent;

//    private void Awake()
//    {
//        var root = uiDocument.rootVisualElement;
    
//        masterVolume = root.Q<SliderInt>("MasterVolume");
//        musicVolume = root.Q<SliderInt>("MusicVolume");
//        sfxVolume = root.Q<SliderInt>("SFXVolume");

//        muteToggle = root.Q<Toggle>("MuteToggle");
//        musicToggle = root.Q<Toggle>("MusicToggle");
//        sfxToggle = root.Q<Toggle>("SFXToggle");

//        if(masterVolume != null)
//            masterVolume.RegisterValueChangedCallback(set => OnMasterVolumeChanged(set.newValue));
//        //if(musicVolume != null)
//        //    musicVolume.RegisterValueChangedCallback(set=> OnMusicVolumeChanged(set.newValue));
//        //if(sfxVolume != null)
//        //    sfxVolume.RegisterValueChangedCallback(set=> OnSfxVolumeChanged(set.newValue));
//        //if (muteToggle != null)
//        //    muteToggle.RegisterValueChangedCallback(set => OnMute(set.newValue));
//        //if(musicToggle != null)
//        //    musicToggle.RegisterValueChangedCallback(set => OnMusic(set.newValue));
//        //if(sfxToggle != null)
//        //    sfxToggle.RegisterValueChangedCallback(set => OnSFX(set.newValue));       

//    }
//    private void OnMute(bool enableSound)
//    {
//        if (disableToggleEvent)
//            return;

//        //if (enableSound)
//            //masterVolume.value = masterVolume * multiplier;
//        else
//            masterVolume.value = 0;
//    }
//    private void OnDisable()
//    {
//        //PlayerPrefs.SetFloat;
//    }
//    private void OnMasterVolumeChanged(float value)
//    {
        
//    }
   
//}
