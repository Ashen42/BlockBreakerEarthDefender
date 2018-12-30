using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class gameHandler : MonoBehaviour {

    // lvl properties
    private bool hasStarted = false;
    private GameSettings gameSettings;

    private Crosshair Crosshair;

    // general ship properties
    public GameObject EnemyShip;
    private GameObject localShip;

    //GameScene
    private bool new_ship_ready = false;
    private bool _first_ship = true;

    // ShipGeneratorScene
    private float componentValue;
    private Vector3 ShipPosition;
    private Slider componentsSlider;
    private float timeStamp_shipSpawned = -999; // start with indication that no ship has been spawned for a looooooong time
    private bool shipSpawnAllowed = true;
    private bool shipPositioningAllowed = true;

    // Use this for initialization
    void Start() {
        Crosshair = GameObject.FindObjectOfType<Crosshair>();
        ShipPosition = new Vector3(8f, 7f, 0f);
        gameSettings = GameObject.FindObjectOfType<GameSettings>();
        if (SceneManager.GetActiveScene().name == "ShipGenerator") {
            componentsSlider = GameObject.FindObjectOfType<Slider>();
            //Adds a listener to the main slider and invokes a method when the value changes.
            componentsSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
            ShipGeneratorScene();
            gameSettings.setShipSettings();
        }
    }

    // Update is called once per frame
    void Update() {
        if (SceneManager.GetActiveScene().name == "ShipGenerator")
            Update_ShipGeneratorScene();
        else Update_GameScene();
    }

    void Update_GameScene() {
        if (!gameSettings.queryLevelStarted()) {
            Debug.Log("GameScene not updated because level not started.");
            return;
        }

        if (gameSettings.queryTimesShipSpawn()) { //Input.GetMouseButtonDown(1) // if right mouse button pressed && gameSettings gives the OK-go based on the spawnrate
            if (shipSpawnAllowed) {
                shipSpawnAllowed = false; // this makes sure we won't request a new ship while the GameScene_spawnShip() CoRoutine is running in the background
                GameScene_spawnShip();
            }
            new_ship_ready = localShip.GetComponent<ShipGeneration>().getShipGenerationSuccessfull();
            if (new_ship_ready) {
                GameScene_positionShip();
                shipSpawnAllowed = true;
                if (_first_ship) { // one time adjustment to first ship speed
                    StartCoroutine("GameScene_FirstShipVelocityAdjustments");
                    _first_ship = false;
                    // straight away ask for a new ship to be generated
                    gameSettings.setShipSpawnRequest(true); 
                }
            }
        }

    }

    void Update_ShipGeneratorScene() {
        if (Input.GetMouseButtonUp(0)) { // if left mouse button pressed
            gameSettings.setShipSettings();
            ShipGeneratorScene();
        }
    }

    public bool queryLevelStarted() {
        return hasStarted;
    }

    void GameScene_spawnShip() {
        if (new_ship_ready) return; // ship already in place. No need to spawn a new one

        shipSpawnAllowed = false; // to make sure we don't request a new one while the ShipGeneration Coroutine is running in the background
        Vector3 spawnLoc = new Vector2(-10,6f);

        // Spawn a ship object to the side (left) of the play field (set proper spawnLoc here <TODO>)
        localShip = Instantiate(EnemyShip, spawnLoc, Quaternion.identity);
        // Generate the ship
        localShip.GetComponent<ShipGeneration>().generateEnemyShip(Random.Range(10,GameSettings.shipComponents), GameSettings.shipScale);

        // update if new ship is successfully created
        new_ship_ready = localShip.GetComponent<ShipGeneration>().getShipGenerationSuccessfull();
    }

    void GameScene_positionShip() {
        if (!new_ship_ready) return; // No ship in place. First spawn a new one!
        if (localShip.GetComponent<ShipGeneration>().getSize()[0] > 15f || localShip.GetComponent<ShipGeneration>().getSize()[1] > 8f) {
            // ship is ready but too large. Destroy it, and request new ship
            localShip.GetComponent<ShipGeneration>().destroyShip(); // <- not really needed apparently
            Destroy(localShip);
            Debug.Log("Generated ship too large (Width = " + localShip.GetComponent<ShipGeneration>().getSize()[0] +
                ", Height = " + localShip.GetComponent<ShipGeneration>().getSize()[1] + "). Destroyed request new one");
            gameSettings.setShipSpawnRequest(true);
            new_ship_ready = false;
        }

        Vector2 spawnLineA = new Vector2(0f, 13f);
        Vector2 spawnLineB = new Vector2(16f, 13f);

        // Check line collision between horizontal line and all vertical bounding box lines of all existing enemy ships
        Vector2[] intersections = checkShipCollisionsLine(spawnLineA, spawnLineB);

        // Based on where there are ships on the collisionLine, and the width of the current ship, see where the ship fits in
        // and randomly allocate the ship to a position on the line
        Vector2 spawnPoint = spawnShipOnLine(localShip, spawnLineA, spawnLineB, intersections);

        if (spawnPoint.x != localShip.transform.position.x) {
            // Adjust spawnPoint y-component to make the ship at the front align with the starting line
            float height = localShip.GetComponent<ShipGeneration>().getSize()[1];
            spawnPoint.y = spawnPoint.y + height / 2 + 1f;

            // Move ship to starting point
            localShip.transform.position = spawnPoint;

            // Set the ship velocity vector
            if (gameSettings.movingShips)
                localShip.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, -GameSettings.shipSpeed);

            // ship has been moved. Space is available for the next ship
            new_ship_ready = false;
            timeStamp_shipSpawned = Time.time;
        }

        shipSpawnAllowed = true; // ship is done. A new spawn is allowed again.
    }

    IEnumerator GameScene_FirstShipVelocityAdjustments() {
        Debug.Log("First ship adjusted to higher speed.");
        GameObject tempShip = localShip;
        tempShip.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, -GameSettings.shipSpeed * 5);
        yield return new WaitForSeconds(2);

        tempShip.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, -GameSettings.shipSpeed);
        yield return null;
    }

    public float getTimeStamp_LastShipSpawned() {
        return timeStamp_shipSpawned;
    }

    public void setTimeStamp_LastShipSpawned(float input) {
        timeStamp_shipSpawned = input;
    }

    Vector2[] checkShipCollisionsLine(Vector2 pointA, Vector2 pointB) {
        // The pointA and pointB indicate the two 2D points between where a line is drawn
        // The points on the line that intersect with any of bounding boxes of spawned enemy ships are detected

        // Get all ship objects in the scene
        ShipGeneration[] ships = Object.FindObjectsOfType(typeof(ShipGeneration)) as ShipGeneration[];
        Vector2[] intersections = new Vector2[ships.Length * 2];

        // loop through each ship and see if the four lines describing the bounding box of the ship 
        // intersect with the linesegment from pointA to pointB
        for (int i = 0; i < ships.Length; i++) {
            Vector2 intersection_left = new Vector2(0f, 0f);
            Vector2 intersection_right = new Vector2(0f, 0f);

            Vector2 size = ships[i].GetComponent<ShipGeneration>().getSize();//Vector2(width, height);
            Vector2 position = new Vector2(ships[i].transform.position.x, ships[i].transform.position.y);

            Vector2 leftUp;
            leftUp.x = position.x - size[0] / 2; // additional 1f for extra offset
            leftUp.y = position.y + 1f + size[1] / 2;
            Vector2 leftDown;
            leftDown.x = position.x - size[0] / 2;
            leftDown.y = position.y - 1f - size[1] / 2;
            // print("Left Up [" + leftUp.x + "," + leftUp.y + "], Down [" + leftDown.x + ", " + leftDown.y + "]");
            // intersection lineAB with left bounding box line
            bool success = LineIntersection(pointA, pointB, leftUp, leftDown, ref intersection_left);

            if (!success) {
                // print("no ship intersection detected for ship " + i);
            } else {
                // Intersection detected. Now also calculate the intersection on the right side of the ship
                Vector2 rightUp;
                rightUp.x = position.x + size[0] / 2;
                rightUp.y = leftUp.y;
                Vector2 rightDown;
                rightDown.x = position.x + size[0] / 2;
                rightDown.y = leftDown.y;
                // intersection lineAB with right bounding box line
                LineIntersection(pointA, pointB, rightUp, rightDown, ref intersection_right);

                // print("intersection ship " + i + " left at (" + intersection_left.x + ", " + intersection_left.y + ")");
                // print("intersection ship " + i + " right at (" + intersection_right.x + ", " + intersection_right.y + ")");
            }

            intersections[i * 2] = intersection_left;
            intersections[i * 2 + 1] = intersection_right;
        }

        // If intersection is (0,0) then no intersection was found. Otherwise intersection IS found
        // for every ship two intersections are recorded (left and right)
        return intersections;
    }

    Vector2 spawnShipOnLine(GameObject localShip, Vector2 pointA, Vector2 pointB, Vector2[] intersections) {
        if (localShip == null) {
            return new Vector2(0f,0f);
        }
        // we'll assume we're operating on a horizontal line. If not, return debug error.
        if (pointA.y != pointB.y) {
            Debug.Log("The provided spawnline is not horizontal.");
            return localShip.transform.position;
        }

        // Now go through all intersections, record x-values in array, and sort this array.
        float[] x_values = new float[intersections.Length+2];
        for (int i=0; i < intersections.Length; i++) {
            x_values[i+1] = intersections[i].x;
        }
        x_values[0] = 0f;
        x_values[x_values.Length - 1] = 16f;

        // sort the x_values, and reverse (large to small, this way we don't have to remove the zero-elements)
        System.Array.Sort(x_values);
        System.Array.Reverse(x_values);

        // Get empty ranges
        // Assumption, 1st to 2nd value is a ship. 2nd to 3rd is empty space, etc. If ships overlap, this may not be true! (shouldn't happen however in game)
        float[] spawnPoint = new float[10]; // max 10 possible points
        int c = 0;
        float width = localShip.GetComponent<ShipGeneration>().getSize()[0];
        for (int i=0; i < x_values.Length; i=i+2) {
            //print("range ["+ x_values[i] + "," + x_values[i+1] + "] is empty");
            if (x_values[i]-x_values[i+1] > localShip.GetComponent<ShipGeneration>().getSize()[0] && c<10) { // ship fits in range
                // print("Ship of width " + localShip.GetComponent<ShipGeneration>().getSize()[0] + "fits in range [" + x_values[i + 1] + ", " + x_values[i] + "]");
                // select random spot within range
                spawnPoint[c] = Random.Range(x_values[i + 1]+width/2, x_values[i]-width/2);
                c++;
            }
        }

        if (c == 0) {
            //print("no possible new position for ship is found");
            return localShip.transform.position;
        }

        // Select one of the ranges at random with width larger than the shipwidth, and select random spawn position within that range
        return new Vector2(spawnPoint[Random.Range(0, c - 1)], pointA.y); ;

    }

    /// <summary>
    /// ShipGeneratorScene is intended for displaying randomly generated spaceships
    /// </summary>
    void ShipGeneratorScene() {
        Vector3 ShipPosition = new Vector3(8f, 7f, 0f);
        // Destroy the last created ship
        if (localShip != null) {
            localShip.GetComponent<ShipGeneration>().destroyShip();
            Destroy(localShip);
        }
        // create new ship
        localShip = Instantiate(EnemyShip, ShipPosition, Quaternion.identity);
        // We enforce ships to not be generated larger than the workspace
        localShip.GetComponent<ShipGeneration>().setEnforceFieldClamp(true);
        // Generate the ship
        localShip.GetComponent<ShipGeneration>().generateEnemyShip((int)componentsSlider.value, GameSettings.shipScale);
    }

    // Invoked when the value of the slider changes.
    void ValueChangeCheck() {
        // return out of function if no localShip object is available
        if (localShip == null) { return; };
        if (componentsSlider.value != componentValue) {
            int componentValue = (int) componentsSlider.value;
        }
    }

    public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection) {
        // Faster Line Segment Intersection
        // Franklin Antonio, Graphics Gems III, 1992, pp. 199 - 202
        // ISBN: 0 - 12 - 409673 - 5

        // Also check http://mathworld.wolfram.com/Line-LineIntersection.html for more math...
        // And https://forum.unity3d.com/threads/line-intersection.17384/ where this code came from (now slightly edited)

        float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;
        float x1lo, x1hi, y1lo, y1hi;

        Ax = p2.x - p1.x;
        Bx = p3.x - p4.x;

        // X bound box test/
        if (Ax < 0) {
            x1lo = p2.x;
            x1hi = p1.x;
        } else {
            x1lo = p1.x;
            x1hi = p2.x;
        }

        if (Bx > 0) {
            if (x1hi < p4.x || p3.x < x1lo)
                return false;
        } else {
            if (x1hi < p3.x || p4.x < x1lo)
                return false;
        }

        Ay = p2.y - p1.y;
        By = p3.y - p4.y;

        // Y bound box test//
        if (Ay < 0) {
            y1lo = p2.y;
            y1hi = p1.y;
        } else {
            y1hi = p2.y;
            y1lo = p1.y;
        }

        if (By > 0) {
            if (y1hi < p4.y || p3.y < y1lo)
                return false;
        } else {
            if (y1hi < p3.y || p4.y < y1lo)
                return false;
        }

        Cx = p1.x - p3.x;
        Cy = p1.y - p3.y;
        d = By * Cx - Bx * Cy;  // alpha numerator//
        f = Ay * Bx - Ax * By;  // both denominator//

        /* Note:
           if denominator > 0
            then if numerator < 0 or numerator > denominator
                then segments  do not intersect
            else if numerator > 0 or numerator < denominator
                then segments  do not intersect 
        */

        // alpha tests//
        if (f > 0) {
            if (d < 0 || d > f) return false;
        } else {
            if (d > 0 || d < f) return false;
        }

        e = Ax * Cy - Ay * Cx;  // beta numerator//

        // beta tests //
        if (f > 0) {
            if (e < 0 || e > f) return false;
        } else {
            if (e > 0 || e < f) return false;
        }

        // check if they are parallel (denominator = 0 means havnig colinear linesegments)
        if (f == 0)
            return false;

        // compute intersection coordinates //
        num = d * Ax; // numerator //
        intersection.x = p1.x + num / f;

        num = d * Ay;
        intersection.y = p1.y + num / f;

        return true;
    }

}
