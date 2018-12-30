using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseCollider : MonoBehaviour {

    private LevelManager levelManager;
    private GameSettings gameSettings;
    private Brick brick;

    void Start() {
        levelManager = GameObject.FindObjectOfType<LevelManager>();
        gameSettings = GameObject.FindObjectOfType<GameSettings>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (gameSettings.allowLose) {
            //print(gameObject.name + " trigger with " + collider.name);
            if (collider.name != "BallRange") { // Ballrange will accidentally trigger this collider otherwise
                brick = collider.GetComponent<Brick>();
                brick.CrashingToEarth();
                //levelManager.LoadLevel("Lose Screen");
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        print("Lose Collision");
    }
}
