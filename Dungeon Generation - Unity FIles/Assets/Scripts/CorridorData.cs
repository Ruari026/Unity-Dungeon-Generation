using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorData : MonoBehaviour
{
    public int value;

    public bool triggered = false;

	// Use this for initialization
	void Start ()
    {
        value = Random.Range(0, 10);
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
