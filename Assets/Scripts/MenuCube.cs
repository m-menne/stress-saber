using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// This script moves a cube that is part of the game's menu and activates its function when hit with a saber.
public class MenuCube : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Move();
    }

    // Called if hit by a saber.
    public void Hit()
    {
        // Perform the function that is associated with this cube's text.
        GameObject.Find("MenuManager").GetComponent<MenuManager>().SendMessage("CubeHit", this.gameObject.GetComponentInChildren<TextMeshProUGUI>().text);
        Destroy(this.gameObject);
    }

    // Method used to move the cube.
    void Move()
    {
        // Moves fast until given position.
        if(transform.localPosition.z > -31.5) transform.position -= Time.deltaTime * transform.forward * 20;
    }
}
