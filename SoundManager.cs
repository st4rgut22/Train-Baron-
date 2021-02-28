using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    static SoundManager instance;
    public AudioSource sound_effect_src;

    public AudioClip explosion;
    public AudioClip cash;
    public AudioClip nice_bird;
    public AudioClip angry_bird;
    public AudioClip train_bell;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void play_explosion()
    {
        sound_effect_src.clip = explosion;
        sound_effect_src.Play();
    }

    public void play_cash()
    {
        sound_effect_src.clip = cash;
        sound_effect_src.Play();
    }

    public void play_nice_bird()
    {
        sound_effect_src.clip = nice_bird;
        sound_effect_src.Play();
    }

    public void play_angry_bird()
    {
        sound_effect_src.clip = angry_bird;
        sound_effect_src.Play();
    }

    public void play_train_bell()
    {
        sound_effect_src.clip = train_bell;
        sound_effect_src.Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
