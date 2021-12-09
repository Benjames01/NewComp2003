using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
public class BasicController : MonoBehaviour
{
    // Controller just to demonstrate AI
    private void Update()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"),0, Input.GetAxis("Vertical"));

        if (input.x != 0 && input.y != 0)
        {
            // stops moving faster diagonally
            input *= 0.666f;
        }
        
        transform.position += input * Time.deltaTime * 10f;
    }
    
}
