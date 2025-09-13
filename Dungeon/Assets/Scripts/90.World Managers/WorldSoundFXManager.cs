using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;


public class WorldSoundFXManager : Singleton<WorldSoundFXManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private List<AudioSource> sfxAudioSources = new List<AudioSource>();
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Mixer Group Parameters")]
    [SerializeField] private string bgmVolumeParameter = "BGMVolume";
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";
    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    
    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    
    private const float MIN_VOLUME = 0.0001f; // -80dB
    private const float MAX_VOLUME = 1f; // 0dB
    
    [Header("Boss Track")] 
    [SerializeField] private AudioSource bossIntroPlayer;
    [SerializeField] private AudioSource bossLoopPlayer;
    
    [Header("Damage Sounds")]
    public AudioClip[] physicalDamageSfx;
    
    [Header("Parry Sounds")]
    [SerializeField] private AudioClip[] blockSfx;
    [SerializeField] private AudioClip[] parrySfx;

    [Header("Action Sounds")]
    public AudioClip rollSfx;
    
    [Header("Weapon Sounds")]
    [SerializeField] private AudioClip[] swordSwingSfx;

    private void Start()
    {
        InitializeAudioSources();
        LoadVolumeSettings();
    }
    
    private void InitializeAudioSources()
    {
        // Initialize BGM audio source
        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
            bgmAudioSource.loop = true;
        }
        
        // Assign BGM to mixer group
        AssignAudioSourceToMixerGroup(bgmAudioSource, "BGM");
        
        // Initialize boss track audio sources and assign to BGM mixer group
        if (bossIntroPlayer != null)
        {
            AssignAudioSourceToMixerGroup(bossIntroPlayer, "BGM");
        }
        
        if (bossLoopPlayer != null)
        {
            AssignAudioSourceToMixerGroup(bossLoopPlayer, "BGM");
        }
        
        // Ensure all existing SFX sources are assigned to SFX mixer group
        foreach (AudioSource source in sfxAudioSources)
        {
            if (source != null)
            {
                AssignAudioSourceToMixerGroup(source, "SFX");
            }
        }
    }
    
    private void AssignAudioSourceToMixerGroup(AudioSource audioSource, string groupName)
    {
        if (audioMixer == null || audioSource == null) return;
        
        // Try to use pre-assigned mixer groups first
        AudioMixerGroup targetGroup = null;
        
        if (groupName == "BGM" && bgmMixerGroup != null)
        {
            targetGroup = bgmMixerGroup;
        }
        else if (groupName == "SFX" && sfxMixerGroup != null)
        {
            targetGroup = sfxMixerGroup;
        }
        else
        {
            // Fallback to finding by name
            AudioMixerGroup[] groups = audioMixer.FindMatchingGroups(groupName);
            if (groups.Length > 0)
            {
                targetGroup = groups[0];
            }
        }
        
        if (targetGroup != null)
        {
            audioSource.outputAudioMixerGroup = targetGroup;
        }
    }
    
    private void LoadVolumeSettings()
    {
        // Load saved volume settings or use defaults
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        // Apply loaded settings
        SetBGMVolume(bgmVolume);
        SetSfxVolume(sfxVolume);
        SetMasterVolume(masterVolume);
    }
    
     #region Volume Control Methods

    public void SetBGMVolume(float normalizedVolume)
    {
        // Clamp between 0 and 1
        normalizedVolume = Mathf.Clamp01(normalizedVolume);
        
        // Convert to decibels (logarithmic scale)
        float decibelValue = ConvertToDecibels(normalizedVolume);
        
        // Set mixer value
        audioMixer.SetFloat(bgmVolumeParameter, decibelValue);
        
        // Save the setting
        PlayerPrefs.SetFloat("BGMVolume", normalizedVolume);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float normalizedVolume)
    {
        // Clamp between 0 and 1
        normalizedVolume = Mathf.Clamp01(normalizedVolume);
        
        // Convert to decibels (logarithmic scale)
        float decibelValue = ConvertToDecibels(normalizedVolume);
        
        // Set mixer value
        audioMixer.SetFloat(sfxVolumeParameter, decibelValue);
        
        // Save the setting
        PlayerPrefs.SetFloat("SFXVolume", normalizedVolume);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float normalizedVolume)
    {
        // Clamp between 0 and 1
        normalizedVolume = Mathf.Clamp01(normalizedVolume);
        
        // Convert to decibels (logarithmic scale)
        float decibelValue = ConvertToDecibels(normalizedVolume);
        
        // Set mixer value
        audioMixer.SetFloat(masterVolumeParameter, decibelValue);
        
        // Save the setting
        PlayerPrefs.SetFloat("MasterVolume", normalizedVolume);
        PlayerPrefs.Save();
    }

    // Helper method to convert linear volume (0-1) to logarithmic decibels
    private float ConvertToDecibels(float normalizedVolume)
    {
        // Prevent log(0) which would give -infinity
        normalizedVolume = Mathf.Max(normalizedVolume, MIN_VOLUME);
        
        // Convert to decibels (logarithmic scale)
        return Mathf.Log10(normalizedVolume) * 20f;
    }

    #endregion

    #region BGM Control Methods

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        bgmAudioSource.clip = clip;
        bgmAudioSource.Play();
    }

    public void StopBGM()
    {
        bgmAudioSource.Stop();
    }

    public void PauseBGM()
    {
        bgmAudioSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmAudioSource.UnPause();
    }

    #endregion

    #region SFX Control Methods

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;

        // Find an available SFX audio source
        AudioSource audioSource = GetAvailableSfxAudioSource();
        audioSource.clip = clip;
        audioSource.volume = 1f; // Reset to full volume, mixer will control actual output
        audioSource.PlayOneShot(clip);
    }

    public void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null) return;

        AudioSource audioSource = GetAvailableSfxAudioSource();
        audioSource.clip = clip;
        audioSource.PlayOneShot(clip, volume);
    }

    // Helper method to get an available SFX audio source
    private AudioSource GetAvailableSfxAudioSource()
    {
        // Check if any existing sources are not playing
        foreach (AudioSource source in sfxAudioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // If all sources are playing, create a new one
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.loop = false;
        
        // Assign to SFX mixer group
        AssignAudioSourceToMixerGroup(newSource, "SFX");
        
        // Add to the list
        sfxAudioSources.Add(newSource);
        return newSource;
    }

    #endregion

    #region Helper Methods

    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat("BGMVolume", 0.75f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("SFXVolume", 0.75f);
    }

    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat("MasterVolume", 1f);
    }

    #endregion
    
    #region Boss Track Methods

    public void PlayBossTrack(AudioClip introTrack, AudioClip loopTrack)
    {
        if (bossIntroPlayer == null || bossLoopPlayer == null) return;
        
        // Reset volumes to full - mixer will control the actual output
        bossIntroPlayer.volume = 1f;
        bossIntroPlayer.clip = introTrack;
        bossIntroPlayer.loop = false;
        bossIntroPlayer.Play();

        bossLoopPlayer.volume = 1f;
        bossLoopPlayer.clip = loopTrack;
        bossLoopPlayer.loop = true;
        bossLoopPlayer.PlayDelayed(bossIntroPlayer.clip.length);
    }

    public void StopBossTrack()
    {
        StartCoroutine(FadeOutBossMusicThenStop());
    }

    private IEnumerator FadeOutBossMusicThenStop()
    {
        float fadeTime = 1f; // Fade duration in seconds
        float startVolume = 1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float currentVolume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeTime);
            
            if (bossLoopPlayer != null)
                bossLoopPlayer.volume = currentVolume;
            if (bossIntroPlayer != null)
                bossIntroPlayer.volume = currentVolume;
                
            yield return null;
        }
        
        // Ensure volumes are set to 0 and stop
        if (bossIntroPlayer != null)
        {
            bossIntroPlayer.volume = 0f;
            bossIntroPlayer.Stop();
        }
        
        if (bossLoopPlayer != null)
        {
            bossLoopPlayer.volume = 0f;
            bossLoopPlayer.Stop();
        }
    }

    #endregion

    #region Random SFX Selection Methods

    public AudioClip ChooseRandomSfxFromArray(AudioClip[] array)
    {
        if(array.Length == 0) return null;

        var index = Random.Range(0, array.Length);

        return array[index];
    }
    
    public AudioClip ChoosePhysicalDamageSfx() => ChooseRandomSfxFromArray(physicalDamageSfx);
    
    public AudioClip ChooseBlockSfx() => ChooseRandomSfxFromArray(blockSfx);
    
    public AudioClip ChooseParriedSfx() => ChooseRandomSfxFromArray(parrySfx);
    
    public AudioClip ChooseSwordSwingSfx() => ChooseRandomSfxFromArray(swordSwingSfx);

    #endregion
    
    #region Weapon Sound Methods
    
    public void PlaySwordSwingSound()
    {
        AudioClip swingClip = ChooseSwordSwingSfx();
        if (swingClip != null)
        {
            PlaySfx(swingClip);
        }
    }
    
    public void PlaySwordSwingSound(float volume)
    {
        AudioClip swingClip = ChooseSwordSwingSfx();
        if (swingClip != null)
        {
            PlaySfx(swingClip, volume);
        }
    }

    #endregion
}