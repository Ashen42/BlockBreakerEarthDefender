using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipGeneration : MonoBehaviour {

    public GameObject[] ShipComponents;

    private GameObject[] ShipComponentsSpawned;
    private int ShipComponensSpawnCount = 0;
    private int maxShipComponents = 50;                // not counting zero, must be even number
    private Vector3 ShipBasePosition;
    private float Scale;
    private float width;
    private float height;
    private bool _ship_generation_successfull = false;
    private bool _enforce_clamp = false;               // only enforce field clamp on the shipGeneratorScene
    private GameSettings gameSettings;
    private int numComponents;
    private float scale;

    void Start() {
        gameSettings = GameObject.FindObjectOfType<GameSettings>();
    }

    void Update() {
        // every ship gameObject has the ShipGeneration script attached to it. Use the gameObject to destroy any ship that passes under the screen
        if (gameObject.transform.position.y < -10f) {
            gameObject.GetComponent<ShipGeneration>().destroyShip(); // <- not really needed apparently
            Destroy(gameObject);
        }
        // Also check routinely if all components have been created. If so, also destroy the ship object, and add to counter shipDestructionCount.
        // go through all components, and destroy them
        int count = 0;
        for (int i = 0; i < ShipComponensSpawnCount; i++) {
            if (ShipComponentsSpawned[i] != null) {
                count++;
            }
        }
        if (count == 0) {
            Destroy(gameObject);
            gameSettings.shipDestructionCountAdd();
            print("Ship destroyed");
        }
    }

    public void generateEnemyShip(int input1, float input2) { //int numComponents, float scale
        numComponents = input1;
        scale = input2;
        StartCoroutine("generateEnemyShipCoroutine");
    }

    private IEnumerator generateEnemyShipCoroutine() {
        GameObject startBlock_type;
        GameObject startBlock_local;
        Vector3 initialPosition = gameObject.transform.position;
        Scale = scale;

        // Limit number of components generated to max 300 ALWAYS
        if (numComponents > 300) {
            Debug.Log("More then 300 components requested. Scale back to 300.");
        }
        maxShipComponents = Mathf.Clamp(numComponents, 1, 400);

        // set shipsize based on number of components. Must be an even number!
        if (maxShipComponents % 2 == 0) {
            // even number. All is well
        } else { //Debug.Log("numComponents was uneven (" + maxShipComponents + "), set to " + (maxShipComponents + 1));
            maxShipComponents++; // odd number. Add one
        }

        // Pre-create an array to hold all the spawned gameobjects
        ShipComponentsSpawned = new GameObject[maxShipComponents];


        // Create the first ship object
        if (ShipComponensSpawnCount <= 0) {

            // Get starting block, and instantiate at startLocation.
            startBlock_type = getShipStartComponent();

            // Set base ship position
            ShipBasePosition = gameObject.transform.position;

            // Update the startblock position (in x-direction), because ShipBasePosition should be the centre of the overall ship
            // Therefore, we should shift the component to the right, equal to half the start component width.
            Vector3 StartBlockPos = ShipBasePosition;
            StartBlockPos.x = StartBlockPos.x + calcComponentDistCentre2Edge(startBlock_type, 2); // 2 left, 3 right

            // Instantiate component
            startBlock_local = spawnShipComponent(startBlock_type, StartBlockPos, 2);

            // Add startBlock_local to local components array
            addToLocalComponentsArray(startBlock_local);
        }

        // === Start loop from here ===
        bool runtime = true;
        while (runtime) {
            // Select component from ShipComponentsSpawned array, having active connectors
            GameObject baseComponent_local = selectRandomSpawnedComponent();

            if (baseComponent_local == null) { // no components found to have active connectors
                runtime = false;
                break;
            }

            // Select random connector from selected i_startComponent
            int i_connector = selectRandomConnector(baseComponent_local);

            if (i_connector == -1) {  // if i_connector == -1, that means all possible connectors have been filled up
                Debug.Log("-1 connector, Component: " + baseComponent_local + "(" + baseComponent_local.GetComponent<ShipComponentProperties>().getConnectorCount() + ")");
                runtime = false;
                break;
            } 

            // Invert connector to get the required connection for the next component
            int i_connector_opposite = invertConnector(i_connector);

            // Select connecting prefab based on required active connector
            GameObject connectingComponent_prefab = getConnectingComponent(i_connector_opposite);

            // Calculate appropriate position for connectingComponent w.r.t. baseComponent, taking i_connector into account
            Vector3 newPosition = calcNewComponentPosition(baseComponent_local, connectingComponent_prefab, i_connector);

            // Check if current position does not overlap with an already spawned component
            bool allowSpawn = checkOverlapComponentColliders(connectingComponent_prefab, newPosition);

            // Check if object position does not go out of bounds
            bool allowSpawn2 = checkComponentOutOfBounds(newPosition);

            if (allowSpawn && allowSpawn2) { //If no overlap is observed (allowSpawn == true), and component is also not out of bounds (allowSpan2 == true)

                // Instantiate component. 
                GameObject nextComponent_local = spawnShipComponent(connectingComponent_prefab, newPosition, i_connector_opposite);

                // Add to local components array
                addToLocalComponentsArray(nextComponent_local);

                // Stop if we're at half the maximum shipcount (the other half is the mirrored shiphalf)
                if (ShipComponensSpawnCount >= maxShipComponents / 2) {
                    runtime = false;
                    break;
                }
            }

            // This function is set to be a coroutine. That is, this function will run over multiple updates if required. 
            // However, at least two component instantiations are required at every update
            if (ShipComponensSpawnCount % 2 == 0) { // ShipComponensSpawnCount is even (==1 is uneven)
                yield return null;
            }
        }

        // Mirror all components to the left of basePosition
        int tmpLoopCount = ShipComponensSpawnCount;
        for (int i = 0; i < tmpLoopCount; i++) {

            // get the difference between the component position and centre of base component
            Vector3 delta = ShipComponentsSpawned[i].GetComponent<Transform>().position - ShipBasePosition;

            // Set mirrorposition, equal to current position, but shifted to the opposite horizontal side
            Vector3 mirrorposition = ShipComponentsSpawned[i].GetComponent<Transform>().position;
            mirrorposition.x = mirrorposition.x - delta.x * 2;

            // Check if mirrored position does not overlap with an already spawned component
            bool allowSpawn = checkOverlapComponentColliders(ShipComponentsSpawned[i], mirrorposition);

            if (allowSpawn) {
                // Instantiate components
                GameObject mirroredComponent = Instantiate(ShipComponentsSpawned[i], mirrorposition, Quaternion.identity);
                // Set the instantiation of the ShipGeneration gameobject as parent of the components instantiated here
                //mirroredComponent.transform.parent = gameObject.transform;
                // Scale the mirrored object
                Vector3 scale = mirroredComponent.GetComponent<ShipComponentProperties>().transform.localScale;
                scale.x = -scale.x;
                mirroredComponent.GetComponent<ShipComponentProperties>().transform.localScale = scale;
                // Add object to array
                addToLocalComponentsArray(mirroredComponent);
            }
            // require at least two components to be instantiated at every update
            if (i % 2 == 0) {
                yield return null;
            }
        }

        // Now all components have been created at ShipBasePosition ( (0,0,0) or wherever), we should recalulate the overall centre position of the ship.
        // and set the position of the instantiated ShipGeneration object to this new centre position. Note that this position is still at the original 
        // x -position, but y is shifted up or down. Once this position is properly set, all procedurally generated components are made a child of the gameObject.
        resetCentrePosition();
        setParentObject();

        // Lastly, the position of the parent gameObject position is updated to its original location.
        gameObject.transform.position = initialPosition;

        // check if all components were created properly (if not we return a false bool)
        _ship_generation_successfull = checkComponentGeneration();
        if (!_ship_generation_successfull)
            Debug.Log("One or more components of the ship failed to be initialized");

        // This function is a coroutine. That is, this function will run over multiple updates if required.
    }

    public void destroyShip() {
        // go through all components, and destroy them
        for (int i = 0; i < ShipComponensSpawnCount; i++) {
            if (ShipComponentsSpawned[i] != null) {
                Destroy(ShipComponentsSpawned[i]);
            }
        }
        // while loop to check if there are accidentally any garbage components which weren't deleted properly
        // Only do this for the ship generator scene
        if (SceneManager.GetActiveScene().name == "ShipGenerator") {
            GameObject[] list = GameObject.FindGameObjectsWithTag("Breakable");
            if (list.Length == 0) { //print("all is well");
            } else { //print("found unremoved components");
                for (int i = 0; i < list.Length; i++) {
                    Destroy(list[i]);
                }
            }
        }
    }

    GameObject getShipStartComponent() {
        // Create starting block, having 4 connectors (i.e. ConnectorCount = 4), and BlockSize 4
        int j = 0;
        int[] ShipComponentsList = new int[ShipComponents.Length - 1]; // instantiate indices array of length equal to number of objects
        for (int i = 0; i <= ShipComponents.Length - 1; i++) {
            // Go through all components in array ShipComponents, and see which one has 4 connectors. Save that i-indix in ShipComponentsList
            if (ShipComponents[i].GetComponent<ShipComponentProperties>().getConnectorCount() == 4 &&
                ShipComponents[i].GetComponent<ShipComponentProperties>().BlockSize == 4) {
                ShipComponentsList[j] = i;
                j++;
            }
        }

        // Choose random startingblock
        int select = Random.Range(0, j);

        // Return selected ShipComponent
        return ShipComponents[ShipComponentsList[select]];
    }

    GameObject spawnShipComponent(GameObject Component, Vector3 Location, int connector) {
        // Instantiate object
        GameObject localComponent = Instantiate(Component, Location, Quaternion.identity);

        // Set the instantiation of the ShipGeneration gameobject as parent of the components instantiated here
        //localComponent.transform.parent = gameObject.transform;

        // Set startposition for selected ShipComponent
        localComponent.GetComponent<ShipComponentProperties>().setComponentPosition(Location);

        // Scale component
        localComponent.GetComponent<Transform>().localScale = localComponent.GetComponent<Transform>().localScale * Scale;

        // If a connector is specified, that connector is removed from the instantiated component.
        if (connector > -1 && connector <= 3) {
            int[] array = new int[4];
            array = localComponent.GetComponent<ShipComponentProperties>().getConnectorArray();
            array[connector] = 0;
            array[2] = 0; // By definition, remove the left connector
            localComponent.GetComponent<ShipComponentProperties>().setConnectorArray(array);
        } else {
            Debug.Log("Incorrect connector (" + connector + ") specified for component " + localComponent);
        }

        // pass back the local copy
        return localComponent;
    }

    bool addToLocalComponentsArray(GameObject Component) {
        if (ShipComponensSpawnCount < maxShipComponents) {
            ShipComponentsSpawned[ShipComponensSpawnCount] = Component;
            ShipComponensSpawnCount++;
            return true;
        } else {
            Debug.Log("Max ShipComponents spawned (spawncount = " + ShipComponensSpawnCount + ", maxShipComponents = " + maxShipComponents + ")");
            return false;
        }
    }

    GameObject selectRandomSpawnedComponent() {
        // Go through all objects, and see if any have active remaining connectors
        int[] optionComponents = new int[ShipComponensSpawnCount + 1];
        int j = 0;
        for (int i = 0; i < ShipComponensSpawnCount; i++) {
            if (ShipComponentsSpawned[i] != null) {
                // Check to see if the spawned ship component has any open connectors to connect to
                //print("Component " + ShipComponentsSpawned[i] + "(j=" + j + ") has " + ShipComponentsSpawned[i].GetComponent<ShipComponentProperties>().getConnectorCount() + " connectors");
                if (ShipComponentsSpawned[i].GetComponent<ShipComponentProperties>().getConnectorCount() > 0) {
                    optionComponents[j] = i;
                    j++;
                }
            }
        }

        // check if any options were available
        if (j == 0) {
            //Debug.Log("No components found to have an active connector");
            return null;
        }

        // Select one of the components at random
        int choice = Random.Range(0, j);

        /*print("Component " + ShipComponentsSpawned[optionComponents[choice]] + " chosen, with " + 
            ShipComponentsSpawned[optionComponents[choice]].GetComponent<ShipComponentProperties>().getConnectorCount() + " connectors" + 
            "(" + optionComponents.Length + " options)");*/

        // return selection
        return ShipComponentsSpawned[optionComponents[choice]];
    }

    int selectRandomConnector(GameObject Component) {
        if (Component == null) {
            Debug.Log("Component " + Component + " is null");
            return -1;
        }

        // Get the connectorArray (of format [up down left right])
        int[] ConnectorArray = new int[4];
        ConnectorArray = Component.GetComponent<ShipComponentProperties>().getConnectorArray();

        // Get the Component connectorcount
        int ConnectorCount = Component.GetComponent<ShipComponentProperties>().getConnectorCount();

        // Because the generated ships will be symmetrical, the 'right' component needs to be negated
        if (Component.GetComponent<ShipComponentProperties>().ConnectorLeft) {
            ConnectorArray[2] = 0;
            ConnectorCount--;
        }

        // loop through connections, saving all those being active
        int[] activeConnections = new int[ConnectorCount];
        int j = 0; // if j==choice, we select that connector
        for (int i = 0; i < ConnectorArray.Length; i++) {
            if (ConnectorArray[i] == 1) {
                activeConnections[j] = i;
                j++;
            }
        }

        // select one active connector at random
        int chosenConnector;
        if (j - 1 >= 0) {
            chosenConnector = activeConnections[Random.Range(0, j)];
        } else {
            //print("No active connections found = " + activeConnections.Length);
            chosenConnector = -1;
            return chosenConnector;
        }

        /*/ Print results to console
        print("component: " + Component + ", " + 
            "array [" + ConnectorArray[0] + "," + +ConnectorArray[1] + "," + +ConnectorArray[2] + "," + ConnectorArray[3] + "] " + 
            "chosenConnector = " + chosenConnector);*/

        // Remove the chosen connector from the connectorArray of that block (to make it not be connected again)
        ConnectorArray[chosenConnector] = 0;
        Component.GetComponent<ShipComponentProperties>().setConnectorArray(ConnectorArray);

        // output the chosen connector (0=up, 1=down, 2=left, 3=right)
        return chosenConnector;
    }

    int invertConnector(int connector) {
        // the connector for the base block is provided. Inverting this gives the 
        // required connector for the next block.
        int requiredConnector = -1;
        switch (connector) {
            case 0: //connector 'up'
                requiredConnector = 1; // requiring a 'down' connector on the next component
                break;
            case 1: //down
                requiredConnector = 0;
                break;
            case 2: //left
                requiredConnector = 3;
                break;
            case 3: //right
                requiredConnector = 2;
                break;
        }

        return requiredConnector;
    }

    GameObject getConnectingComponent(int requiredConnector) {
        // the requiredConnector is the index in the connectorArray that needs to be 1 (of the chosen prefab)
        int j = 0; // count the amount of object having the proper connector
        int[] ShipComponentsList = new int[ShipComponents.Length - 1]; // instantiate indices array of length equal to number of objects
        for (int i = 0; i <= ShipComponents.Length - 1; i++) {
            // Go through all components in array ShipComponents, and see which one has the proper connector. Save that i-indix in ShipComponentsList
            if (ShipComponents[i].GetComponent<ShipComponentProperties>().getConnectorArray()[requiredConnector] == 1) {
                ShipComponentsList[j] = i;
                j++;
            }
        }

        // Randomly select one of the shipComponents having the appropriate connector index, and return it
        int chosenComponent = Random.Range(0, j);
        return ShipComponents[ShipComponentsList[chosenComponent]];
    }

    Vector3 calcNewComponentPosition(GameObject baseComponent_local, GameObject connectingComponent_prefab, int direction) {

        Vector3 shift = new Vector3(0f, 0f, 0f);
        switch (direction) {
            case 0: //connecting 'up'
                shift.y = 1; // the next position is therefore 1 block 'down'
                break;
            case 1: //down
                shift.y = -1;
                break;
            case 2: //left
                shift.x = -1;
                break;
            case 3: //right
                shift.x = 1;
                break;
        }
        // Use the sizes and orientation of the baseComponent_local and newly selected connectingComponent_prefab to calculate 
        // the distance from centre to component edge. The distances for both blocks together determine the distance from 
        // centre to centre. Based on the unity shift vector, the new shift vector can be calculated.
        float dist1 = calcComponentDistCentre2Edge(baseComponent_local, direction);
        float dist2 = calcComponentDistCentre2Edge(connectingComponent_prefab, direction);
        shift = shift * (dist1 + dist2);

        // Add randomized variable perpendicular placement  
        if (direction == 0 || direction == 1) { // up or down
            // If the number of blocks in the horizontal direction (i.e. width) is unequal between these blocks
            if (baseComponent_local.GetComponent<ShipComponentProperties>().getBlockSizeWidth() >
                connectingComponent_prefab.GetComponent<ShipComponentProperties>().getBlockSizeWidth()) {
                shift.x = calcComponentDistCentre2Edge(connectingComponent_prefab, 2) * Mathf.Pow(-1, Random.Range(1, 3));
            } else if (baseComponent_local.GetComponent<ShipComponentProperties>().getBlockSizeWidth() <
                connectingComponent_prefab.GetComponent<ShipComponentProperties>().getBlockSizeWidth()) {
                shift.x = calcComponentDistCentre2Edge(baseComponent_local, 2) * Mathf.Pow(-1, Random.Range(1, 3));
            }
        } else if (direction == 2 || direction == 3) { // left or right
            if (baseComponent_local.GetComponent<ShipComponentProperties>().getBlockSizeHeight() >
                connectingComponent_prefab.GetComponent<ShipComponentProperties>().getBlockSizeHeight()) {
                shift.y = calcComponentDistCentre2Edge(connectingComponent_prefab, 0) * Mathf.Pow(-1, Random.Range(1, 3));
            } else if (baseComponent_local.GetComponent<ShipComponentProperties>().getBlockSizeHeight() <
                connectingComponent_prefab.GetComponent<ShipComponentProperties>().getBlockSizeHeight()) {
                shift.y = calcComponentDistCentre2Edge(baseComponent_local, 0) * Mathf.Pow(-1, Random.Range(1, 3));
            }
        }

        // Calculate connectingComponent_prefab position based on the baseComponent_local (after adjusting the shift vector based on component sizes)
        Vector3 newPosition = baseComponent_local.GetComponent<ShipComponentProperties>().getComponentPosition() + shift;

        // Make sure the new item does not cross the vertical mirroring line of the ship (i.e. does not go -x w.r.t. baseShipPosition)
        Vector3 delta = newPosition - ShipBasePosition;
        if (delta.x < 0) {
            // set the component position equal to x-ShipBasePosition
            newPosition.x = ShipBasePosition.x;
        }

        // Make sure the object falls within the playing field
        if (_enforce_clamp) {
            newPosition.x = Mathf.Clamp(newPosition.x, 0.5f, 14.5f);
            newPosition.y = Mathf.Clamp(newPosition.y, 1.5f, 10.5f);
        }

        return newPosition;
    }

    float calcComponentDistCentre2Edge(GameObject Component, int direction) {
        int Component_Size = Component.GetComponent<ShipComponentProperties>().BlockSize;
        bool Component_Horizontal = Component.GetComponent<ShipComponentProperties>().OrientationHorizontal;
        bool Component_Vertical = Component.GetComponent<ShipComponentProperties>().OrientationVertical;

        float dist = 0f;
        switch (Component_Size) {

            case 4: // Should be a square block
                if (Component_Horizontal || Component_Vertical) {
                    Debug.LogError("Spaceship component " + Component + " of Blocksize " + Component_Size + " should be square.");
                }
                dist = 0.5f;
                break;

            case 3: // Should not exist
                Debug.LogError("Spaceship component " + Component + " should not use Blocksize " + Component_Size + " (choose 1, 2 or 4)");
                break;

            case 2: // Can either be horizontal or vertical
                if (Component_Horizontal) { // component is horizontal
                    if (direction == 0 || direction == 1) { //(0 = up, 1 = down, 2 = left, 3 = right)
                        dist = 0.25f;
                    } else { // direction == 2 or 3
                        dist = 0.5f;
                    }
                } else if (Component_Vertical) { // component is vertical
                    if (direction == 0 || direction == 1) { //(0 = up, 1 = down, 2 = left, 3 = right)
                        dist = 0.5f;
                    } else { // direction == 2 or 3
                        dist = 0.25f;
                    }
                } else { // component is square
                    Debug.LogError("Spaceship component " + Component + " of Blocksize " + Component_Size + " needs an orientation indication");
                }
                break;

            case 1: // Single square block
                if (Component_Horizontal || Component_Vertical) {
                    Debug.LogError("Spaceship component " + Component + " of Blocksize " + Component_Size + " cannot have an orientation.");
                }
                dist = 0.25f;
                break;
        }

        dist = dist * Scale;
        return dist;
    }

    bool checkOverlapComponentColliders(GameObject Component, Vector3 position) {
        Vector2 point = new Vector2(position.x, position.y);
        // calculate the corners of the rectangle approx equal to the size of the component. 
        // delta_x and y are reduced to 80% to avoid collisions at the boundaries
        float delta_y = calcComponentDistCentre2Edge(Component, 0) * 0.8f; // 0 up, 1 down
        float delta_x = calcComponentDistCentre2Edge(Component, 2) * 0.8f; // 2 left, 3 right
        Vector2 pointA = point - new Vector2(delta_x, delta_y);
        Vector2 pointB = point + new Vector2(delta_x, delta_y);
        Collider2D collider = Physics2D.OverlapArea(pointA, pointB);

        if (collider == null) {
            return true;
        } else { // Debug.Log("collision detected for " + Component);
            return false;
        }
    }

    bool checkComponentOutOfBounds(Vector3 position) {
        Vector3 deltaPos = position - ShipBasePosition;
        //Debug.Log("Coordinates: [" + deltaPos.x + ", " + deltaPos.y + ", " + deltaPos.z + "]");
        if (deltaPos.x > 6.5) {
            Debug.Log("Component x-coordinate out of bounds (x = " + deltaPos.x +"). Clipping...");
            return false;
        }
        if (deltaPos.y > 3.5 || deltaPos.y < -3.5) {
            Debug.Log("Component y-coordinate out of bounds (y = " + deltaPos.y + "). Clipping...");
            return false;
        }
        return true;
    }

    bool checkComponentGeneration() {
        // <TODO> This function needs a better way of checking if shipgeneration was successfull. Currently it is used to check the components 
        // proper instantiation, but if components never get spawned, and the count doesn't get updated, this function still ends up giving 'true'
        bool check = true;
        for (int i =0; i< ShipComponensSpawnCount; i++) {
            if (ShipComponentsSpawned[i] == null) {
                check = false;
                Debug.Log("Found a shipComponent to not be instantiated properly (component " + ShipComponentsSpawned[i] + ", i=" + i + ")");
            }
        }

        return check;
    }

    public Vector3 resetCentrePosition() {
        Vector3 centre = new Vector3(0f, 0f, 0f);
        float x_min = 99f;
        float x_max = -99f;
        float y_min = 99f;
        float y_max = -99f;
        // Loop through all objects, and determine the x-range and y-range
        for (int i = 0; i < ShipComponensSpawnCount; i++) {
            if (ShipComponentsSpawned[i] != null) {
                Vector3 pos = ShipComponentsSpawned[i].GetComponent<Transform>().position;
                if (pos.x < x_min)          x_min = pos.x- calcComponentDistCentre2Edge(ShipComponentsSpawned[i], 2);
                else if (pos.x > x_max)     x_max = pos.x + calcComponentDistCentre2Edge(ShipComponentsSpawned[i], 2);
                if (pos.y < y_min)          y_min = pos.y - calcComponentDistCentre2Edge(ShipComponentsSpawned[i], 0);
                else if (pos.y > y_max)     y_max = pos.y + calcComponentDistCentre2Edge(ShipComponentsSpawned[i], 0);
            }
        }

        // Calculate centre
        centre.x = x_min + (x_max - x_min) / 2;
        centre.y = y_min + (y_max - y_min) / 2;

        // Change the gameobject position (that is, THIS instantiation of the ship) to the actual centre of the ship
        // Note, only after we've position the instantiated ship to the correct centre position, do we set the ship to
        // be the parent of the instantiated separate components
        gameObject.transform.position = centre;

        // Calculate also some general ship data
        width = x_max - x_min;
        height = y_max - y_min;

        //print("x_range = [" + x_min + "," + x_max + "] => width = " + width + ", y_range = [" + y_min + "," + y_max + "] => height = " + height);
        //print("ship centre = (" + centre.x + ", " + centre.y + ", " + centre.z + ")");

        return centre;
    }

    public void setParentObject() {
        // Loop through all objects, and make them child to the THIS ship (being the gameObject)
        for (int i = 0; i < ShipComponensSpawnCount; i++) {
            if (ShipComponentsSpawned[i] != null) {
                ShipComponentsSpawned[i].transform.parent = gameObject.transform;
            }
        }
    }

    public void setMaxShipComponents(int input) {
        maxShipComponents = input;
    }

    public void setEnforceFieldClamp(bool input) {
        _enforce_clamp = input;
    }

    public Vector2 getSize() {
        return new Vector2(width+1.4f, height);
    }

    public bool getShipGenerationSuccessfull() { return _ship_generation_successfull; }
}
