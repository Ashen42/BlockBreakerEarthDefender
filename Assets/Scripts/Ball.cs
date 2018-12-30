using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {

    public Vector3 paddleToBallVector;
    public float startVelocity;
    public Color LineColor1 = Color.yellow;
    public Color LineColor2 = Color.red;

    private GameSettings gameSettings;
    private Paddle paddle;
    private LineRenderer lineRenderer;
    private Vector3 mousePositionInBlocks;
    private float startVelocity_x = 0;
    private float startVelocity_y = 0;
    private new AudioSource audio;
    private Vector3 CollisionLocation;
    private bool _ballReturn = false;
    private bool _ballLock = true;
    private Vector3 prevBallLoc;
    private Vector3 holdPaddlePos;

    // Use this for initialization
    void Start() {
        // Find gameobjects
        gameSettings = GameObject.FindObjectOfType<GameSettings>();
        paddle = GameObject.FindObjectOfType<Paddle>();

        // set audio component
        audio = this.GetComponent<AudioSource>();

        // Info
        paddleToBallVector = this.transform.position - paddle.transform.position;
        CollisionLocation = gameObject.transform.position;

        // Line renderer settings: 2 color gradient with a fixed alpha of 1.0f
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(LineColor1, 0.0f), new GradientColorKey(LineColor2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.colorGradient = gradient;
    }

    // Update is called once per frame
    void Update() {

        if (!gameSettings.queryLevelStarted() || _ballLock) {
            // Lock the ball relative to the paddle if the level hasn't started yet
            this.transform.position = paddle.transform.position + paddleToBallVector;
        };

        // Wait for a mouse press to launch & start the game
        if (Input.GetMouseButtonDown(0) && !gameSettings.queryLevelStarted()) {
            print("Mouse button clicked, launch ball");
            gameSettings.startLevel();

            // Calculating starting speed vector
            if (gameSettings.mouseAiming) {
                randomizeStartBallVector();
            }
        }

        // Call function to create flight trail (changes on every collision)
        drawLineFromBounseToBall();

        // Handle mouseclicks for ball control
        ballControl();

        // Failsafe just in case the ball ends up UNDER the paddle
        keepBallAbovePaddle();
        keepBallInField();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (_ballLock) {
            //print("No collision allowed"); // no collisions allowed when ball is locked in place
            return;
        }
        // Slightly adding a random speed vector to the ball speedvector in the x- and y-directions
        Vector2 tweak = new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));

        if (gameSettings.queryLevelStarted()) {
            audio.Play();
            GetComponent<Rigidbody2D>().velocity += tweak; // old code: rigidbody.velocity += tweak;

            // Save new bounce location to draw line from to ball
            CollisionLocation = gameObject.transform.position;
        }
    }

    void getMousePosition() {
        mousePositionInBlocks = Input.mousePosition / Screen.width * 16;
        mousePositionInBlocks.x = Mathf.Clamp(mousePositionInBlocks.x, 0f, 16f);
        mousePositionInBlocks.y = Mathf.Clamp(mousePositionInBlocks.y, 1f, 12f);
    }

    public float calcAngleBallToCollision() {
        Vector3 BallPosition_Relative_ToCollisionLocation = this.GetComponent<Transform>().position-CollisionLocation;
        // Calculate angle
        float angle = Mathf.Atan2(BallPosition_Relative_ToCollisionLocation.y, BallPosition_Relative_ToCollisionLocation.x) * Mathf.Rad2Deg;
        if (float.IsNaN(angle)) {
            return 0;
        }
        angle = angle + 90;
        return angle;
    }

    void drawLineFromBounseToBall() {
        // Set start and ending positions
        if (!gameSettings.queryLevelStarted()) {
            // if the level hasn't started yet, make the linerenderer start location
            // always be at the ball location. (this way we trick the collisionLocation
            // to be equal to the ball location untill we have a 'real' collision
            CollisionLocation = gameObject.transform.position;
        }
        lineRenderer.SetPosition(0, new Vector3(CollisionLocation[0], CollisionLocation[1], CollisionLocation[2]));
        lineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y, transform.position.z));

    }

    void ballControl() {
        // Upon click, summon ball back to paddle, and lock in place once it is close to the paddle
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && gameObject.transform.position.y > paddle.GetComponent<Transform>().position.y + 0.3f) {
            _ballReturn = true;
            _ballLock = false;
        }
        if (_ballReturn && !_ballLock && gameObject.transform.position.y < paddle.GetComponent<Transform>().position.y+0.8f) {
            _ballLock = true;
        }
        // On Update it is checked if an request is done to send the ball back to the paddle.
        if (_ballReturn) {
            // Send the ball back to the paddle
            pullBallBack2Paddle();
        }

        // At ball lock, make sure, the ball is vertically properly locked w.r.t. paddle
        if (_ballLock) {
            Vector3 paddlePos= paddle.GetComponent<Transform>().position;
            transform.position = new Vector3(paddlePos.x, paddlePos.y + 0.3f, paddlePos.z);
        }

        // Launch ball if locked to paddle & left-mouse clicked
        if (Input.GetMouseButtonDown(0) && _ballLock && !paddle.querryPaddleLock()) {
            _ballLock = false;
            _ballReturn = false;
            unrandomizedStartBallVector();
            paddle.unlockPaddle();
        }

        // Launch aimed-ball if locked to paddle & right-mouse clicked
        if (Input.GetMouseButton(1) /*&& _ballLock*/) {
            paddle.lockPaddle();
        }
        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && _ballLock && paddle.querryPaddleLock()) {
            _ballLock = false;
            _ballReturn = false;
            aimedStartBallVector();
            paddle.unlockPaddle();
        }
    }

    void randomizeStartBallVector() {
        float randomValue = Random.Range(-70, 70);
        randomValue = randomValue / 100;
        startVelocity_x = startVelocity * randomValue;
        startVelocity_y = Mathf.Sqrt((startVelocity * startVelocity) - (startVelocity_x * startVelocity_x));

        // Set net speedvector
        this.GetComponent<Rigidbody2D>().velocity = new Vector2(startVelocity_x, startVelocity_y); // rigidbody2D.velocity is depracated
    }

    void unrandomizedStartBallVector() {
        this.GetComponent<Rigidbody2D>().velocity = new Vector2(0, startVelocity); // rigidbody2D.velocity is depracated
    }

    void aimedStartBallVector() {
        getMousePosition();
        Vector3 deltaPos = mousePositionInBlocks - paddle.GetComponent<Transform>().position;
        float angle = Mathf.Atan2(deltaPos.y, deltaPos.x);
        float xVel = startVelocity * Mathf.Cos(angle);
        float yVel = Mathf.Sqrt((startVelocity * startVelocity) - (xVel * xVel));
        this.GetComponent<Rigidbody2D>().velocity = new Vector2(xVel, yVel); // rigidbody2D.velocity is depracated
    }

    void pullBallBack2Paddle() {
        // set velocity back to paddle
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, -startVelocity);

        // Make x-position of ball approach paddle with maximum speed of shift per frame
        Vector3 paddleLoc = paddle.GetComponent<Transform>().position;
        Vector3 newBallLoc;
        float shift = 0.5f;
        if (transform.position.x - paddleLoc.x > shift) {
            newBallLoc = new Vector3(transform.position.x - shift, transform.position.y, transform.position.z);
        } else if (paddleLoc.x - transform.position.x > shift) {
            newBallLoc = new Vector3(transform.position.x + shift, transform.position.y, transform.position.z);
        } else {
            //newBallLoc = new Vector3(paddle.GetComponent<Transform>().position.x, transform.position.y, transform.position.z); //and lock to x-position of paddle once close enough to paddle
            newBallLoc = transform.position; // alternative. NOT lock to paddle, but keep tracking paddle with shift.
        }

        CollisionLocation = prevBallLoc; // This is required to make the trail of the ball originate from the proper location
        prevBallLoc = newBallLoc;
        gameObject.GetComponent<Transform>().position = newBallLoc;

    }

    public bool getBallLockState() {
        return _ballLock;
    }
    public bool getBallReturnState() {
        return _ballReturn;
    }

    private void keepBallAbovePaddle() {
        if (transform.position.y < 1f) {
            transform.position = new Vector3(paddle.GetComponent<Transform>().position.x, paddle.GetComponent<Transform>().position.y + 0.3f, transform.position.z);
            _ballLock = true;
            _ballReturn = false;
            CollisionLocation = transform.position; // fixing the tail of the ball
        }
    }

    private void keepBallInField() {
        // constrain x-direction
        if (transform.position.x > 16f) {
            transform.position = new Vector3(15.7f, transform.position.y, transform.position.z);
        } else if (transform.position.x < 0f) {
            transform.position = new Vector3(0.3f, transform.position.y, transform.position.z);
        }

        // constrain y-direction
        if (transform.position.y > 12f) {
            transform.position = new Vector3(transform.position.x, 11.7f, transform.position.z);
        } else if (transform.position.y < 1f) {
            transform.position = new Vector3(transform.position.x, 1.3f, transform.position.z);
        }
    }
}
