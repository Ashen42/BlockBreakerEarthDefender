using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour {

    private GameSettings gameSettings;
    private Vector3 mousePositionInBlocks;

    // Use this for initialization
    void Start () {
        gameSettings = GameObject.FindObjectOfType<GameSettings>();
        if (gameSettings.mouseAiming) { Cursor.visible = false; }
    }

    // Update is called once per frame
    void Update() {
        if (gameSettings.mouseAiming) {
            getMousePosition();
            // Note: mouse should be in z-layer -1, because it should always be ontop
            gameObject.transform.position = new Vector3(mousePositionInBlocks.x, mousePositionInBlocks.y, -1f) ;
        }
	}

    void getMousePosition() {
        mousePositionInBlocks = Input.mousePosition / Screen.width * 16;
        mousePositionInBlocks.x = Mathf.Clamp(mousePositionInBlocks.x, 0f, 16f);
        mousePositionInBlocks.y = Mathf.Clamp(mousePositionInBlocks.y, 1f, 12f);
    }

    public Vector3 getMousePositionInBlocks() { return mousePositionInBlocks; }
}