using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    public Camera m_Camera;
    private Player m_Player;

	// Use this for initialization
	void Start () {
		
	}
	
    public void SetPlayer(Player player)
    {
        m_Player = player;
    }

	// Update is called once per frame
	void Update () {

        if (m_Player) 
        {
            // Vector3 newPos = new Vector3(m_Player.transform.position.x, m_Camera.transform.position.y, m_Camera.transform.position.z);
            Vector3 newPos = new Vector3(m_Player.transform.position.x, m_Player.transform.position.y + 6.0f, m_Camera.transform.position.z);
            m_Camera.transform.position = newPos;
        }        
	}
}
