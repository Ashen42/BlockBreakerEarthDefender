using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    private ScoreTracker scoreTracker;
    private GameSettings gameSettings;
    private MusicPlayer musicPlayer;

    private void Start() {
        scoreTracker = GameObject.FindObjectOfType<ScoreTracker>();
        gameSettings = GameObject.FindObjectOfType<GameSettings>();
        musicPlayer = GameObject.FindObjectOfType<MusicPlayer>();
    }

    public void LoadLevel(string name) {
        Cursor.visible = true;
        Debug.Log("Level load request for: " + name);
        //Application.LoadLevel(name);

        if (name == "Level_02") {
            scoreTracker.updateGameCount();
            musicPlayer.playLevel1();
        } else if (name == "Lose Screen") {
            gameSettings.endLevel();
            musicPlayer.playCredits();
        } else if (name == "Credits") {
            musicPlayer.playCredits();
        } else if (name == "Start") {
            musicPlayer.playStartMusic();
        }

        SceneManager.LoadScene(name);
    }

    public void LoadNextLevel(){
        Cursor.visible = true;
        //Application.LoadLevel(Application.loadedLevel +1); // <- depracated
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex +1);
        // see SceneManager.LoadScene https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadScene.html
        // see SceneManager Unity Class https://docs.unity3d.com/ScriptReference/30_search.html?q=SceneManager
    }

    public void QuitRequest() {
        Debug.Log("Quit Requested");
        Application.Quit();
    }

    public void BrickDestroyed() {
        if (Brick.breakableCount <= 0) {
            levelCleanup();
            LoadNextLevel();
        }
    }

    private void levelCleanup() {
        // Set brick breakableCount to 0, to make sure that when a scene is loaded
        // (and all the bricks get counted), the old brick count is added to this value;
        Brick.breakableCount = 0;
    }
}