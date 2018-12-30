using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour {
    // Create a static instance of the musicplayer. Static means that it is a property of the class, not of the created instance of the class.
    // see: https://unity3d.com/learn/tutorials/topics/scripting/statics
    static MusicPlayer instance = null;
    public AudioClip startMusic;
    public AudioClip Level1;
    public AudioClip Level2;
    public AudioClip Level3;
    public AudioClip Credits;

    void Awake () {
        //Debug.Log("Music player Awake " + GetInstanceID());
        if (instance != null) {
            Destroy(gameObject);
            print("Dubplicate music player self-destructing");
        } else {
            instance = this;
            // Makes the object target not be destroyed automatically when loading a new scene
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }

	// Use this for initialization
	void Start () {
        Debug.Log("Music player Start " + GetInstanceID());
        playAudioClip(startMusic);
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    void playAudioClip(AudioClip audioClip) {
        //this.GetComponent<AudioSource>().Stop();
        //Debug.Log("Current music playing: " + this.GetComponent<AudioSource>().clip.name);
        //Debug.Log("Requested music: " + audioClip.name);
        if (this.GetComponent<AudioSource>().clip.name != audioClip.name) {
            this.GetComponent<AudioSource>().clip = audioClip;
            this.GetComponent<AudioSource>().Play();
            Debug.Log("Music playing: " + audioClip.name);
        } else {
            Debug.Log("Requested music was already playing.");
        }
    }

    public void playStartMusic()    { playAudioClip(startMusic); }
    public void playLevel1()        { playAudioClip(Level1); }
    public void playLevel2()        { playAudioClip(Level2); }
    public void playLevel3()        { playAudioClip(Level3); }
    public void playCredits()       { playAudioClip(Credits); }


}
