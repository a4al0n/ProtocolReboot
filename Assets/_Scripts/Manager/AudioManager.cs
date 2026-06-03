using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] AudioClipArray;

    private static Dictionary<string, AudioClip> _DicAudio;
    private static AudioSource audioBGM;
    private static AudioSource[] audioSources;

    //[Header("VOL")]
    //[Range(0, 1)]
    //public float volumeOfBGM;
    //[Range(0, 1)]
    //public float volumeOfEffect;

    //[Header("VOL Slider")]
    public Slider volumeSlider;
    public float Volume { get; set; }

    void Awake()
    {
        _DicAudio = new Dictionary<string, AudioClip>();
        foreach (var item in AudioClipArray)
        {
            _DicAudio.Add(item.name, item);
        }
        audioBGM = GetComponent<AudioSource>();
        if (audioBGM == null)
            audioBGM = gameObject.AddComponent<AudioSource>();

        audioSources = GetComponents<AudioSource>();

        Volume = volumeSlider.value;
    }


    public void PlayEffect(string acName)
    {
        if (_DicAudio.ContainsKey(acName) && !string.IsNullOrEmpty(acName))
        {
            AudioClip ac = _DicAudio[acName];
            PlayEffect(ac);
        }
    }

    private void PlayEffect(AudioClip ac)
    {
        if (ac)
        {
            audioSources = gameObject.GetComponents<AudioSource>();

            for (int i = 1; i < audioSources.Length; i++)
            {
                if (!audioSources[i].isPlaying)
                {
                    audioSources[i].loop = false;
                    audioSources[i].clip = ac;
                    audioSources[i].volume = Volume;
                    audioSources[i].Play();
                    return;
                }
            }

            AudioSource newAs = gameObject.AddComponent<AudioSource>();
            newAs.loop = false;
            newAs.clip = ac;
            newAs.volume = Volume;
            newAs.Play();
        }
    }

    public void BGMPlay(string acName)
    {
        if (_DicAudio.ContainsKey(acName) && !string.IsNullOrEmpty(acName))
        {
            AudioClip ac = _DicAudio[acName];
            BGMPlay(ac);
        }
    }

    private void BGMPlay(AudioClip ac)
    {
        if (ac)
        {
            audioBGM.clip = ac;
            audioBGM.loop = true;
            audioBGM.volume = Volume;
            audioBGM.Play();
        }
    }

    public void StopBGMPlay()
    {
        audioBGM.Stop();
    }

    public void SetVolume()
    {
        if (volumeSlider == null) return;

        Volume = volumeSlider.value;

        // Получаем актуальные компоненты прямо перед изменением громкости
        audioSources = GetComponents<AudioSource>();

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null) // Дополнительная проверка на всякий случай
                audioSources[i].volume = Volume;
        }
    }
}
