﻿using UnityEngine;
using System.Collections;

public class NPCSensor : MonoBehaviour
{
    public Collider2D col;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerSensor ps = collision.GetComponent<PlayerSensor>();
        if (!ps) return;
        Debug.Log(collision.gameObject.name);
    }
}
