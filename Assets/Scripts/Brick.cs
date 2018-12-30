using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour {

    public AudioClip crack;
    public Sprite[] hitSprites; //Sprite array
    public static int breakableCount = 10000; // set back to 0 to make finish level when all bricks destroyed work again.
    public GameObject smoke;
    public GameObject ComponentEarthCollision;

    private int timesHit;
    private LevelManager levelManager;
    private GameSettings gameSettings;
    private bool isBreakable;
    private Ball ball;

    // Use this for initialization
    void Start () {
        ball = GameObject.FindObjectOfType<Ball>();
        isBreakable = (this.tag == "Breakable");
        // Keep track of breakable bricks
        if (isBreakable) {
            breakableCount++;
        }

        levelManager = GameObject.FindObjectOfType<LevelManager>();
        gameSettings = GameObject.FindObjectOfType<GameSettings>();
        timesHit = 0;
	}

    // Update is called once per frame
    void Update () {

    }

    void OnCollisionEnter2D(Collision2D collision) {
        // check item that is being collided with. If it is the ball, check that it is not locked
        if (collision.collider.name == "Ball") {
            if (ball.getBallLockState()) {
                print("No collision allowed with " + collision.collider.name);
                return; // no collisions with ball are allowed
            }
            // else ball not in locked state. So all is well, and code continues
        }
        
        AudioSource.PlayClipAtPoint(crack, transform.position, 0.8f); //0.8f is the volume
        if (isBreakable) {
            HandleHits();
        }
    }

    public void HandleHits() {
        timesHit++;
        int maxHits = hitSprites.Length + 1; //print(gameObject.name + ": timesHit = " + timesHit);
        if (timesHit >= maxHits) {
            breakableCount--;
            // create clone of smoke object at brick location, and set color equal to brick color
            createSmoke();
            // Destroy the brick
            Destroy(gameObject);             // GameObject.DestroyObject(gameObject);
            // Update score
            gameSettings.ScoreAdd();        // <toDo> currently also updates on earth crash
            // Check end-of-level reached
            levelManager.BrickDestroyed();   // check if all bricks have been destroyed in LevelManager
        } else {
            LoadSprites();
        }
    }

    public void destroyObject() {
        //print("Destroyed " + gameObject.name + " in Brick Script");
        print("Destroyed object in Brick Script");
        Destroy(gameObject);
    }

    void createSmoke() {
        GameObject localSmoke = Instantiate(smoke, gameObject.transform.position, Quaternion.identity);
        ParticleSystem.MainModule main = localSmoke.GetComponent<ParticleSystem>().main;
        main.startColor = gameObject.GetComponent<SpriteRenderer>().color;
    }

    void LoadSprites() {
        int spriteIndex = timesHit - 1;
        if (hitSprites[spriteIndex] != null) {
            this.GetComponent<SpriteRenderer>().sprite = hitSprites[spriteIndex];
        } else {
            Debug.LogError("Sprite index " + spriteIndex + " for object " + gameObject.name + " not loaded");
        }
    }

    public void CrashingToEarth() {
        Instantiate(ComponentEarthCollision, this.transform.position, Quaternion.identity);
        gameSettings.ScoreDeduct(); // score is automatically added on HandleHits. So if we pre-emptivately deduct, score will remain unchanged
        gameSettings.EarthHealthDeduct();
        HandleHits();
    }
}