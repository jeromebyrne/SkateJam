using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    // public
    public MusicManager m_MusicManager;
    public Player m_PlayerPrefab;
    public CameraControl m_CameraControl;
    public GameObject m_SpawnPoint;

    // private
    private Player m_Player;
    private Resetable[] m_Resetables;

    // Use this for initialization
    void Start () {
        ResetPlayer();

        m_Resetables = GameObject.FindObjectsOfType<Resetable>();
    }
	
	// Update is called once per frame
	void Update () {
        CheckInput();
    }

    public void ResetPlayer()
    {
        if (m_Player)
        {
            m_Player.Reset();
            m_Player = null;
        }

        m_Player = Instantiate(m_PlayerPrefab, m_SpawnPoint.transform.position, new Quaternion()) as Player;

        m_CameraControl.SetPlayer(m_Player);
    }

    void CheckInput()
    {
        if (Input.GetMouseButton(0) && m_Player.AnyWheelsOnGround() && !m_Player.IsCrouching() && !m_Player.IsPlayingLandAnim())
        {
            if (m_Player)
            {
                m_Player.SetIsCrouching(true);
                m_Player.PushSkateboard();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (m_Player)
            {
                m_Player.SetIsCrouching(false);
                m_Player.Ollie();
            }
        }
        else if (Input.GetMouseButtonUp(2))
        {
            if (m_Player)
            {
                ResetPlayer();
            }

            ResetObjects();
        }
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }

    private void ResetObjects()
    {
        foreach (var r in m_Resetables)
        {
            r.ResetAll();
        }
    }
}
