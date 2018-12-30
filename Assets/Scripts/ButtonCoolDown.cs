using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCoolDown : MonoBehaviour {

    Button playAgainButton;

    // Use this for initialization
    void Start () {
        playAgainButton = Object.FindObjectOfType<Button>();
        playAgainButton.GetComponent<Button>().interactable = false;
        StartCoroutine("buttonCoolDown");
    }

    IEnumerator buttonCoolDown() {
        yield return new WaitForSeconds(3);
        playAgainButton.GetComponent<Button>().interactable = true;
        yield return null;
    }
}
