using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.source.pitch = s.pitch;
        }
    }

    public void Play(string name)
    {
        Sound storedSound = new Sound();
        foreach (Sound s in sounds)
        {
            if (s.name.Equals(name))
            {
                 storedSound = s;
            }
        }
        if (storedSound != null)
        {
            if (storedSound.randomizeable == true) storedSound.source.pitch = (Random.Range(0.8f, 1.2f));
            storedSound.source.Play();
        }
        else return;
    }

    public void Stop(string name)
    {
        Sound storedSound = new Sound();
        foreach (Sound s in sounds)
        {
            if (s.name.Equals(name))
            {
                storedSound = s;
            }
        }
        if (storedSound != null)
        {
            storedSound.source.Stop();
        }
        else return;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
