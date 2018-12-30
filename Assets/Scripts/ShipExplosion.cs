using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipExplosion : MonoBehaviour {
    public Sprite[] explosionSprites;
    public int spriteIndex = 0;

    // Use this for initialization
    void Start () {
        this.GetComponent<SpriteRenderer>().sprite = explosionSprites[spriteIndex];
    }

    // Update is called once per frame
    void Update () {
        shipExplosionUpdate();

    }

    void shipExplosionUpdate() {
        spriteIndex++;
        this.GetComponent<SpriteRenderer>().sprite = explosionSprites[spriteIndex];
        if (spriteIndex > 38) { // cycle through sprites 32-46
            spriteIndex = 10;
            //Destroy(gameObject);             // GameObject.DestroyObject(gameObject);
        }

    }
}
