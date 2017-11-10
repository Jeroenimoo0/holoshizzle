using System.Collections;
using System.Collections.Generic;
using HoloToolkitExtensions.Messaging;
using UnityEngine;


public class BasketInitializer : MonoBehaviour
{

    public GameObject Basket;

	// Use this for initialization
	void Start ()
	{
        Messenger.Instance.AddListener<PositionFoundMessage>(ProcessMessage);
	    Basket.SetActive(false);
	}

    private void ProcessMessage(PositionFoundMessage msg)
    {
        if (msg.Status == PositionFoundStatus.Accepted)
        {
            Basket.SetActive(true);
            Basket.transform.position = msg.Location;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
