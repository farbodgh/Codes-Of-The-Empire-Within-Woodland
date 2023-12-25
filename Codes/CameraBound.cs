using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBound : MonoBehaviour
{
    private Camera m_mainCamera;

    private void Start()
    {
        m_mainCamera = Camera.main;
    }

    private void Update()
    {
        if(transform.position.x < -550)
        {
            transform.position = new Vector3(-550, transform.position.y, transform.position.z);
        }
        if (transform.position.x > 550)
        {
            transform.position = new Vector3(550, transform.position.y, transform.position.z);
        }
        if (transform.position.z < -550)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -550);
        }
        if (transform.position.z > 550)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 550);
        }

    }
}
