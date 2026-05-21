using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{

    public Sprite mute;
    public Sprite unmute;
    public Image soundicon;
    public AudioSource[] bgms;


    Button buttonmute;
    dontDestroy dontDestroyInstance;

    public void Awake()
    {

        mute = Resources.Load("mut", typeof(Sprite)) as Sprite;
        unmute = Resources.Load("unmut", typeof(Sprite)) as Sprite;
        soundicon = GameObject.Find("vol").GetComponent<Image>();
        buttonmute = GameObject.Find("vol").GetComponent<Button>();
        bgms = GameObject.Find("holder").GetComponents<AudioSource>();
        dontDestroyInstance = GameObject.Find("holder").GetComponent<dontDestroy>();
        buttonmute.onClick.AddListener(mutepressed);
        
        // Set sprite berdasarkan soundon state
        UpdateSoundIcon();
    }

    public void UpdateSoundIcon()
    {
        if (dontDestroyInstance != null)
        {
            if (dontDestroyInstance.soundon)
            {
                soundicon.sprite = unmute;
            }
            else
            {
                soundicon.sprite = mute;
            }
        }
    }

    public void mutepressed()
    {
        if (dontDestroyInstance.soundon == true)
        {
            dontDestroyInstance.soundon = false;
            soundicon.sprite = mute;
            foreach (AudioSource bgm in bgms)
                bgm.Pause();
        }
        else
        {
            dontDestroyInstance.soundon = true;
            soundicon.sprite = unmute;
            foreach (AudioSource bgm in bgms)
                bgm.Play();
        }
    }
}