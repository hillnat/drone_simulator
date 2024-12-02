using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource[] emitters;
    private GameObject emitterPrefab;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        emitterPrefab = (GameObject)Resources.Load("AudioEmitter");
        emitters = new AudioSource[10];
        for (int i = 0; i < emitters.Length; i++)
        {
            emitters[i] = Instantiate(emitterPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<AudioSource>();
            emitters[i].playOnAwake = false;
            emitters[i].spatialBlend = 1f;
        }
    }
    public void PlaySound(AudioClip clip, Vector3 position)
    {
        AudioSource emitter = GetFreeEmitter();
        if (emitter != null)
        {
            emitter.spatialBlend = 1f;
            emitter.volume = SettingsManager.instance.playerSettings.masterVolume * SettingsManager.instance.playerSettings.soundFxVolume;
            emitter.transform.position= position;
            emitter.PlayOneShot(clip);
        }
    }
    public void PlaySound2D(AudioClip clip, Vector3 position)
    {
        AudioSource emitter = GetFreeEmitter();
        if (emitter != null)
        {
            emitter.spatialBlend = 0;
            emitter.volume = SettingsManager.instance.playerSettings.masterVolume * SettingsManager.instance.playerSettings.soundFxVolume;
            emitter.transform.position = position;
            emitter.PlayOneShot(clip);
        }
    }
    private AudioSource GetFreeEmitter()
    {
        for (int i = 0; i < emitters.Length; i++)
        {
            if (emitters[i].isPlaying == false) { return emitters[i]; }
        }
        Debug.LogWarning("Free Emitter not found");
        return null;
    }
}
