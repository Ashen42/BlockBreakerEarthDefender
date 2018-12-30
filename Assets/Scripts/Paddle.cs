using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour {

    public Vector2 xRange;
    public Color LineColor1 = Color.blue;
    public Color LineColor2 = Color.red;

    private GameSettings gameSettings;
    private Ball ball;
    private Vector3 paddlePos;
    private Vector3 mousePositionInBlocks;
    private Vector3 CollisionLocation;
    private bool paddleLock = false;

    // Use this for initialization
    void Start () {
        // Find gameobjects
        gameSettings = GameObject.FindObjectOfType<GameSettings>();
        ball = GameObject.FindObjectOfType<Ball>();

        // Set vectors
        paddlePos = new Vector3(0.5f, this.transform.position.y, 0f);

        // Line renderer: 2 color gradient with a fixed alpha of 1.0f
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(LineColor1, 0.0f), new GradientColorKey(LineColor2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.colorGradient = gradient;
    }

    // Update is called once per frame
    void Update () {
        getMousePosition();
        
        // if level hasn't started yet
        if (!gameSettings.queryLevelStarted()) {
            MovePaddleWithMouse();
            return; // The 'return' skips all code hereafter
        }

        // Calls related to mouse aiming
        if (gameSettings.mouseAiming) {
            drawLineToMousePosition();
        }

        // Calls related to autoplay vs. paddle moving with mouse
        if (!gameSettings.autoPlay || ball.getBallReturnState()) {
            MovePaddleWithMouse();
        } else if (ball.getBallLockState()) {
            // paddle locked in place
        } else {
            AutoPlay();
        }
    }

    void OnCollisionEnter2D(Collision2D collision) { // if ball collides
        if (gameSettings.mouseAiming) {
            // --- script to make the ball bounce towards the location of the mouse ---
            // Calculate angle of mouse w.r.t. paddle location (with offset of ball to paddle)
            Vector3 deltaVec = paddlePos + ball.paddleToBallVector - mousePositionInBlocks;
            float angle = -Mathf.Atan2(deltaVec.y, deltaVec.x); //0 left of paddle, 0.5pi right above, pi rot the right
            // Get speedvector and recalculate x- and y-components in the right direction
            float velocity_x = ball.startVelocity * Mathf.Cos(angle);
            float velocity_y = ball.startVelocity * Mathf.Sin(angle);
            ball.GetComponent<Rigidbody2D>().velocity = new Vector2(-velocity_x, velocity_y);
        }
    }

    void getMousePosition() {
        mousePositionInBlocks = Input.mousePosition / Screen.width * 16;
        mousePositionInBlocks.x = Mathf.Clamp(mousePositionInBlocks.x, 0f, 16f);
        mousePositionInBlocks.y = Mathf.Clamp(mousePositionInBlocks.y, 1f, 12f);
    }

    void drawLineToMousePosition() {
        getMousePosition();
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        // Set start and ending positions
        Vector3 startPosition = paddlePos + ball.paddleToBallVector;
        lineRenderer.SetPosition(0, new Vector3(startPosition[0], startPosition[1], startPosition[2]));
        lineRenderer.SetPosition(1, new Vector3(mousePositionInBlocks[0], mousePositionInBlocks[1], mousePositionInBlocks[2]));
    }

    void MovePaddleWithMouse() {
        if (paddleLock) {
            return;
        }
        // Set horizontal paddle position with mouse 
        // (original code as made in Udemy course)
        /*paddlePos = new Vector3(0.5f, this.transform.position.y, 0f);
        float mousePosInBlocks = Input.mousePosition.x / Screen.width * 16;
        paddlePos.x = Mathf.Clamp(mousePosInBlocks, 0.5f, 15.5f);
        this.transform.position = paddlePos;*/

        // replacement code (mousePositionInBlocks is updated in Update() )
        paddlePos.x = Mathf.Clamp(mousePositionInBlocks.x, xRange[0], xRange[1]); 
        this.transform.position = paddlePos;
    }

    void AutoPlay() {
        // Set horizontal paddle position equal to the horizontal position of the ball
        // Original code as mode in Udemy course)
        /*paddlePos = new Vector3(0.5f, this.transform.position.y, 0f);
        Vector3 ballPos = ball.transform.position;
        paddlePos.x = Mathf.Clamp(ballPos.x, 0.5f, 15.5f);
        this.transform.position = paddlePos;*/

        // Set horizontal paddle position with ball position (own code)
        paddlePos.x = Mathf.Clamp(ball.transform.position.x, xRange[0], xRange[1]); // Shortcut alternative to set paddle position to ball horizontal position at all times
        this.transform.position = paddlePos;
    }

    public void lockPaddle() {
        print("paddle locked in place");
        paddleLock = true;
    }

    public bool querryPaddleLock() {
        return paddleLock;
    }

    public void unlockPaddle() {
        print("paddle unlocked");
        paddleLock = false;
    }
}
