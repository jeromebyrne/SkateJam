using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resetable : MonoBehaviour {

    Vector3 originalPosition;
    Quaternion originalRotation;

    Rigidbody2D rigidBody = null;

	// Use this for initialization
	void Start () {
        originalPosition = gameObject.transform.position;
        originalRotation = gameObject.transform.rotation;
        rigidBody = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ResetAll()
    {
        // gameObject.transform.
        if (rigidBody)
        {
            rigidBody.velocity = Vector2.zero;
            rigidBody.angularVelocity = 0.0f;
        }

        gameObject.transform.position = originalPosition;
        gameObject.transform.rotation = originalRotation;
    }
}
