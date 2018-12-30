using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class showScore : MonoBehaviour {
    private Text scoreText;
    private Text highscoreText;
    private ScoreTracker scoreTracker;
    private int score = 0;
    private int highscore = 0;

	// Use this for initialization
	void Start () {
        scoreTracker = GameObject.FindObjectOfType<ScoreTracker>();
        scoreText = GameObject.Find("Score").GetComponent<Text>();
        highscoreText = GameObject.Find("Highscore").GetComponent<Text>();

        outputScore();
        outputHighScore();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void outputScore() {

        if (scoreTracker == null) {
            Debug.Log("scoreTracker was not initialized (happeneds at start)");
            return;
        }
        score = scoreTracker.getScoreCount();
        scoreText.text = score.ToString();
    }

    public void outputHighScore() {

        if (scoreTracker == null) {
            Debug.Log("scoreTracker was not initialized (happeneds at start)");
            return;
        }
        highscore = scoreTracker.getHighscoreCount();
        highscoreText.text = highscore.ToString();
    }
}
