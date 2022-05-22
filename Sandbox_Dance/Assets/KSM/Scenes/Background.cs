using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Background : MonoBehaviour
{
    public AudioSource audio;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "3. Game")
        {
            audio.mute = true;
            audio.Stop();
        }
        else
        {
            audio.mute = false;
            if(!audio.isPlaying)
            audio.Play();
        }
    }
}
