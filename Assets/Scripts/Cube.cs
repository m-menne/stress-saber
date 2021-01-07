using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class handles the behaviour of a cube in the game.
public class Cube : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Move();
    }

    // Method used to move the cube.
    void Move()
    {
        // Moves fast until given position.
        if(transform.localPosition.z > -35.0) transform.position -= Time.deltaTime * transform.forward * 20;
        // After the cube passes this position it moves with normal speed.
	    else transform.position -= Time.deltaTime * transform.forward * 2;
    }

    // Change the material of the colored part of the cube.
    public void changeColor(Material mat_a)
    {
        this.transform.GetChild(0).gameObject.GetComponentInChildren<Renderer>().material = mat_a;
        ParticleSystem.MainModule psMain = this.GetComponent<ParticleSystem>().main;
        psMain.startColor = mat_a.color;
    }
 }
