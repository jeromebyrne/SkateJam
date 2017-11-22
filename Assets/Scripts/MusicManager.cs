using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    private int m_CurrentMenuMusicIndex = -1;
    private int m_CurrentLevelMusicIndex = -1;

    public AudioClip[] m_MenuMusic;
    public AudioClip[] m_LevelMusic;
    public float m_musicVolume = 1.0f;

    private AudioSource m_AudioSourceCurrentMusic;

    // Use this for initialization
    void Start () {
        m_AudioSourceCurrentMusic = gameObject.AddComponent<AudioSource>();
        m_AudioSourceCurrentMusic.volume = m_musicVolume;

        m_CurrentLevelMusicIndex = Random.Range(0, m_LevelMusic.Length - 1);

        PlayNextLevelMusic();
    }
	
	// Update is called once per frame
	void Update () {
		
        if (m_AudioSourceCurrentMusic && !m_AudioSourceCurrentMusic.isPlaying)
        {
            PlayNextLevelMusic();
        }
	}

    public void PlayNextLevelMusic()
    {
        ++m_CurrentLevelMusicIndex;

        if (m_CurrentLevelMusicIndex > m_LevelMusic.Length - 1)
        {
            m_CurrentLevelMusicIndex = 0;
        }

        if (m_AudioSourceCurrentMusic)
        {
            m_AudioSourceCurrentMusic.Stop();

            m_AudioSourceCurrentMusic.clip =  m_LevelMusic[m_CurrentLevelMusicIndex];

            m_AudioSourceCurrentMusic.Play();
        }
    }
}
