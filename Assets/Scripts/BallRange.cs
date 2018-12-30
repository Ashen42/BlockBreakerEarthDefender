using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallRange : MonoBehaviour {

    private Ball ball;
    private GameSettings gameSettings;

    public Sprite[] SpriteFireball; // Fireball animation continuously looping
    int spriteIndex = 0;

    // Use this for initialization
    void Start() {
        gameSettings = GameObject.FindObjectOfType<GameSettings>();
        ball = GameObject.FindObjectOfType<Ball>();

        this.GetComponent<SpriteRenderer>().sprite = SpriteFireball[spriteIndex];
    }

    // Update is called once per frame
    void Update() {
        FireBallSpriteUpdate();
    }

    void FireBallSpriteUpdate() {
        bool ballReturn = ball.getBallLockState() && ball.getBallReturnState();
        if (spriteIndex > 46 && !ballReturn) { // cycle through sprites 32-46
            spriteIndex = 32;
        } else if (gameSettings.queryLevelStarted() && !ballReturn) {
            spriteIndex++;
        } else if (ballReturn && spriteIndex > 0 && spriteIndex < 49) {
            spriteIndex++;
        }  else {
            spriteIndex = 0;
        }

        this.GetComponent<SpriteRenderer>().sprite = SpriteFireball[spriteIndex];
        this.GetComponent<Transform>().eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, ball.calcAngleBallToCollision());
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (ball.getBallLockState()) {
            return; // if the ball is locked, destruction of ships is not possible
        }
        //print("OnTriggerExit2D detected: " + collider.name);
        if (collider.tag == "Breakable") {
            //print(collider.name + " destroyed");
            //collider.gameObject.GetComponent<Brick>().destroyObject();
            collider.gameObject.GetComponent<Brick>().HandleHits();
        } else {
            //print(collider.name + " trigger ignored");
        }

    }

}
