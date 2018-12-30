using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameSettings : MonoBehaviour {

    public bool autoPlay = false;
    public bool mouseAiming = false;
    public bool movingShips = true;
    public bool allowLose = true;
    public bool allowDifficultyScaling = true;

    // Ship parameters
    public float shipScaleStart = 1f;
    public float shipScaleMin = 0.5f;
    public float spawnRateStart = 0.5f;
    public float shipSpeedStart = 0.2f;
    public float shipSpeedMax = 10.0f;
    public int shipComponentsStart = 10;
    public int shipComponentsMax = 300;
    public static float shipScale;
    public static float spawnRate; // Ships spawned per minute
    public static float shipSpeed;
    public static int shipComponents;

    private float startTime;
    private bool hasStarted = false;
    private bool manualSpawnRequest = false;
    private int scoreCount = 0;
    private Text scoreCountText;
    private int shipDestructionCount = 0;
    private Text shipDestructionCountText;
    private int earthHealth = 100;
    private Text earthHealthText;
    private LevelManager levelManager;
    private ScoreTracker scoreTracker;
    private GameSettings gameSettings;

    void Start() {
        shipScale = shipScaleStart;
        spawnRate = spawnRateStart; // ships per minute
        shipSpeed = shipSpeedStart;
        shipComponents = shipComponentsStart;
        scoreCountText = GameObject.Find("ScoreCount").GetComponent<Text>();
        shipDestructionCountText = GameObject.Find("ShipDestructionCount").GetComponent<Text>();
        earthHealthText = GameObject.Find("EarthHealth").GetComponent<Text>();
        levelManager = GameObject.FindObjectOfType<LevelManager>();
        scoreTracker = GameObject.FindObjectOfType<ScoreTracker>();
        gameSettings = GameObject.FindObjectOfType<GameSettings>();

    }

    void Update() {
        if (allowDifficultyScaling) {
            difficultyFunct();
        }
        SetScoreCountText();
        SetShipDestructionCountText();
        SetEarthHealthText();
    }

    public void startLevel() {
        if (hasStarted) return;
        // the level start gets called from the ball script
        // when launching the ball.
        hasStarted = true;
        startTime = Time.time;
        gameSettings.setShipSettingsStart();

        // Remove UI instruction texts
        Destroy(GameObject.Find("InstructionsLB"));
        Destroy(GameObject.Find("InstructionsRB"));
    }

    public bool queryLevelStarted() {
        return hasStarted;
    }

    public void endLevel() {
        Debug.Log("Level Ended");
        hasStarted = false;
    }

    public bool queryTimesShipSpawn() {
        // write something here that outputs true or false depending on the spawnRate
        // <TODO>
        float diff = (Time.time - GameObject.FindObjectOfType<gameHandler>().getTimeStamp_LastShipSpawned()) * spawnRate;
        if (diff > 60 || manualSpawnRequest) {  // spawn new ship if it has been longer than a minute, or manually asked for
            return true;
        } else if (manualSpawnRequest) {
            manualSpawnRequest = false;
            GameObject.FindObjectOfType<gameHandler>().setTimeStamp_LastShipSpawned(Time.time); // ensuring that if a manualSpawnRequest is performed, the timer for the next spawn is reset
            return true;
        } else {
            return false;
        }
    }

    public void setShipSpawnRequest(bool input) {
        manualSpawnRequest = input;
    }

    private void difficultyFunct() {
        float timestep = (Time.time - startTime)/60;                                                 // num of minutes since scene start

        if (shipScale > shipScaleMin) { // 0.5
            shipScale = Mathf.Clamp(shipScaleStart - (0.2f*timestep), shipScaleMin, 2f);                    // decrease scale with 0.2 per minute (default startsize is 1f)
        }

        spawnRate = Mathf.Clamp(spawnRateStart + timestep*10, 1f, 30f);                                                   // Num of ships spawn every 60s - 10s per minute past

        //if (shipSpeed > 0.5)
            shipSpeed = Mathf.Clamp(shipSpeedStart + shipSpeedStart * (0.5f*timestep), 0.5f, shipSpeedMax);   // 50% increase in speed each minute (relative to start speed)

        if (shipComponents < shipComponentsMax) // 200
            shipComponents = Mathf.RoundToInt(shipComponentsStart * (1 + timestep * 10));           // 10 more components each minute

        //print("Scale=" + shipScale + ", Speed=" + shipSpeed + ", Components=" + shipComponents + ", spawnRate=" + spawnRate);
    }

    private void SetScoreCountText() {
        if (scoreTracker == null) {
            Debug.Log("scoreTracker was not initialized (happeneds at start)");
            return;
        }
        scoreCountText.text = scoreCount.ToString();
    }

    public void ScoreAdd() {
        scoreCount++;

        if (scoreTracker == null) {
            Debug.Log("scoreTracker was not initialized (happeneds at start)");
            return;
        }
        scoreTracker.setScoreCount(scoreCount);
    }

    public void ScoreDeduct() {
        scoreCount--;

        if (scoreTracker == null) { 
            Debug.Log("scoreTracker was not initialized (happeneds at start)");
            return;
        }
        scoreTracker.setScoreCount(scoreCount);
    }

    private void SetShipDestructionCountText() {
        shipDestructionCountText.text = shipDestructionCount.ToString();
    }

    public void shipDestructionCountAdd() {
        shipDestructionCount++;

        if (scoreTracker == null) {
            Debug.Log("scoreTracker was not initialized (happeneds at start)");
            return;
        }
        scoreTracker.setShipDestructionCount(shipDestructionCount);
    }

    private void SetEarthHealthText() {
        earthHealthText.text = earthHealth.ToString() + "%";
    }

    public void EarthHealthAdd() {
        earthHealth = Mathf.Clamp(earthHealth+5, 0, 100);
    }

    public void EarthHealthDeduct() {
        earthHealth = Mathf.Clamp(earthHealth - 5, 0, 100);
        if (earthHealth <= 0) {
            print("EarthHealth depleted. Game lost.");
            levelManager.LoadLevel("Lose Screen");
        }
    }

    public void setShipSettings() {
        shipScale = shipScaleMin;
        spawnRate = spawnRateStart; // ships per minute
        shipSpeed = shipSpeedStart;
        shipComponents = shipComponentsMax;
        print("Ship settings applied");
    }

    public void setShipSettingsStart() {
        shipScale = shipScaleStart;
        spawnRate = spawnRateStart; // ships per minute
        shipSpeed = shipSpeedStart;
        shipComponents = shipComponentsStart;
        print("Ship Start settings applied");
    }

}