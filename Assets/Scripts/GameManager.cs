using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    // public
    public MusicManager m_MusicManager;
    public Player m_PlayerPrefab;
    public CameraControl m_CameraControl;
    public GameObject m_SpawnPoint;
    public Text currentSessionTimeText = null;
    public Text bestSessionTimeText = null;

    // private
    private Player m_Player;
    private Resetable[] m_Resetables;
    private float currentSessionTime = 0.0f;
    private float bestSessionTime = 0.0f;
    private bool updateCurrentSessionTime = true;

    // Use this for initialization
    void Start () {
        ResetPlayer();

        m_Resetables = GameObject.FindObjectsOfType<Resetable>();

        bestSessionTime = PlayerPrefs.GetFloat("best_time");

        UpdateTimers();
    }
	
	// Update is called once per frame
	void Update () {

        if (updateCurrentSessionTime)
        {
            currentSessionTime += Time.deltaTime;
        }
        
        UpdateTimers();
        CheckInput();
    }

    public void OnReachedEnd()
    {
        if (bestSessionTime == 0 || currentSessionTime < bestSessionTime)
        {
            bestSessionTime = currentSessionTime;
            PlayerPrefs.SetFloat("best_time", bestSessionTime);
        }

        updateCurrentSessionTime = false;

        UpdateTimers();
    }

    private void UpdateTimers()
    {
        string minSec = string.Format("{0}:{1:00}", (int)currentSessionTime / 60, (int)currentSessionTime % 60);
        currentSessionTimeText.text = minSec;

        // best time
        string bestMinSec = string.Format("{0}:{1:00}", (int)bestSessionTime / 60, (int)bestSessionTime % 60);
        bestSessionTimeText.text = bestMinSec;
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

        currentSessionTime = 0;
        updateCurrentSessionTime = true;
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
            ResetSession();
        }
    }

    public void ResetSession()
    {
        if (m_Player)
        {
            ResetPlayer();
        }

        ResetObjects();
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
