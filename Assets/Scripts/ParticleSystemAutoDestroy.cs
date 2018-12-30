using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemAutoDestroy : MonoBehaviour {

    private ParticleSystem ps;

    public void Start() {
        ps = GetComponent<ParticleSystem>();
    }

    public void Update() {
        if (ps) {
            if (!ps.IsAlive()) {
                // Destroy with "duration" delay
                //Destroy(gameObject, GetComponent<ParticleSystem>().duration);
                // http://answers.unity3d.com/questions/219609/auto-destroying-particle-system.html
                ParticleSystem.MainModule main = gameObject.GetComponent<ParticleSystem>().main;
                Destroy(gameObject, main.duration);
            }
        }
    }
}
