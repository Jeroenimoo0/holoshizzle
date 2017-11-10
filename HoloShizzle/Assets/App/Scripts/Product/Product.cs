using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product : MonoBehaviour, IFocusable
{

    public string id;
    public string description;
    public string unitSize;
    public string brandName;
    public string[] categories;

    public string[] Infos {
        get
        {
            return _infos;
        }

        set
        {
            _infos = value;

            var text = "";

            foreach(var line in _infos)
            {
                text += line + "\n";
            }

            GetComponentInChildren<TextMesh>(true).text = text;
        }
    }

    private string[] _infos;
    private GameObject textObject;

    private void Start()
    {
        textObject = GetComponentInChildren<TextMesh>().gameObject;
        textObject.SetActive(false);
    }

    private void Update()
    {
        if(transform.position.y < -5)
        {
            transform.position = FindObjectOfType<HoloShizzler>().spawnPosition.transform.position;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    public void OnFocusEnter()
    {
        textObject.SetActive(true);
    }

    public void OnFocusExit()
    {
        textObject.SetActive(false);
    }
}
