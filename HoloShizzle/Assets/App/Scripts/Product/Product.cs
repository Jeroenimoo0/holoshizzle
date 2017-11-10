using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product : MonoBehaviour {

    public string id;
    public string description;
    public string unitSize;
    public string brandName;
    public string[] categories;
    public string[] infos;

    private void Update()
    {
        if(transform.position.y < -5)
        {
            transform.position = FindObjectOfType<HoloShizzler>().spawnPosition.transform.position;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
