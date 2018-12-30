using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreTracker : MonoBehaviour {
    // Create a static instance of the musicplayer. Static means that it is a property of the class, not of the created instance of the class.
    // see: https://unity3d.com/learn/tutorials/topics/scripting/statics
    static ScoreTracker instance = null;
    private int scoreCount;
    private int highscoreCount;
    private int shipDestructionCount;
    private int gameCount = 0; // number of times game has been played (in one sitting)
    private Text scoreText;
    private MusicPlayer musicPlayer;
    private bool _musicLevel1 = false;
    private bool _musicLevel2 = false;
    private bool _musicLevel3 = false;

    void Awake() {
        //Debug.Log("Music player Awake " + GetInstanceID());
        if (instance != null) {
            Destroy(gameObject);
            Debug.Log("Duplicate scoretracker self-destructing");
        } else {
            instance = this;
            // Makes the object target not be destroyed automatically when loading a new scene
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }

    // Use this for initialization
    void Start() {
        musicPlayer = GameObject.FindObjectOfType<MusicPlayer>();

    }

    // Update is called once per frame
    void Update() {
        if (scoreCount > highscoreCount) {
            highscoreCount = scoreCount;
        }
        updateMusic();
    }

    public void setScoreCount(int input) {
        scoreCount = input;
        //Debug.Log("Current score: " + scoreCount);
    }

    public int getScoreCount() {
        return scoreCount;
    }

    public int getHighscoreCount() {
        return highscoreCount;
    }

    public void setShipDestructionCount(int input) {
        shipDestructionCount = input;
        //Debug.Log("Ships Destroyed: " + shipDestructionCount);
    }

    public void updateGameCount() {
        gameCount++;
        //Debug.Log("Game Startrequest found. Game " + gameCount + " started.");
    }

    void updateMusic() {
        if (scoreCount >= 1000 && scoreCount < 3000 && !_musicLevel2) {
            musicPlayer.playLevel2();
            _musicLevel2 = true;
            _musicLevel3 = false;
        } else if (scoreCount >= 3000 && !_musicLevel3){
            musicPlayer.playLevel3();
            _musicLevel2 = false;
            _musicLevel3 = true;
        }
    }

}
