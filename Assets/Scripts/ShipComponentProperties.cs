using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipComponentProperties : MonoBehaviour {

    public bool ConnectorRight;         // Connections for ship generation
    public bool ConnectorLeft;
    public bool ConnectorUp;
    public bool ConnectorDown;
    [Range(0,4)] public int BlockSize;              // Amount of blocks (square = 4)
    public bool OrientationHorizontal;
    public bool OrientationVertical;

    private int[] ConnectorArray;
    private int ConnectorCount;
    private bool calculationsPerformed = false;
    private Vector3 ComponentPosition;
    private int BlockSizeWidth = 1;
    private int BlockSizeHeight = 1;

    // Use this for initialization
    void Start () {
        calculateProperties();
    }
	

    void calculateProperties() {
        // just run through this function if ConnectorArray has not yet been populated
        if (!calculationsPerformed) {
            // Setup an array of size 4 with the bool Connectors indicated within
            ConnectorArray = new int[4];

            // Fill up ConnectorArray as follows: [up down left right]
            if (ConnectorUp) ConnectorArray[0] = 1;
            else ConnectorArray[0] = 0;

            if (ConnectorDown) ConnectorArray[1] = 1;
            else ConnectorArray[1] = 0;

            if (ConnectorLeft) ConnectorArray[2] = 1;
            else ConnectorArray[2] = 0;

            if (ConnectorRight) ConnectorArray[3] = 1;
            else ConnectorArray[3] = 0;

            // Calculations done
            calculationsPerformed = true;
        } else {

            // Change public connectors to match connectorArray
            if (ConnectorArray[0] == 1) ConnectorUp = true;
            else ConnectorUp = false;

            if (ConnectorArray[1] == 1) ConnectorDown = true;
            else ConnectorDown = false;

            if (ConnectorArray[2] == 1) ConnectorLeft = true;
            else ConnectorLeft = false;

            if (ConnectorArray[3] == 1) ConnectorRight = true;
            else ConnectorRight = false;
        }

        // Count amount of connections
        ConnectorCount = ConnectorArray[0] + ConnectorArray[1] + ConnectorArray[2] + ConnectorArray[3];

        // Get Width and height
        if (BlockSize == 2) {
            if (OrientationHorizontal) BlockSizeWidth = 2;
            else if (OrientationVertical) BlockSizeHeight = 2;
        } else if (BlockSize == 4) {
            BlockSizeWidth = 2;
            BlockSizeHeight = 2;
        }
    }

    // Functions to set private variables
    public void setComponentPosition(Vector3 position) { ComponentPosition = position; }
    public void setConnectorArray(int[] NewConnectorArray) { ConnectorArray = NewConnectorArray; }

    // Functions to access private variables in other scripts
    public Vector3 getComponentPosition() { return ComponentPosition; }
    public int[] getConnectorArray() { calculateProperties();  return ConnectorArray; } // [up down left right]
    public int getConnectorCount() { calculateProperties();  return ConnectorCount; }
    public int getBlockSizeWidth() { return BlockSizeWidth; }
    public int getBlockSizeHeight() { return BlockSizeHeight; }
}
