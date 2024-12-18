using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoScripts : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string sceneName;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (videoPlayer.frame >= (long)videoPlayer.frameCount - 1)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    public void SkipVideo(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
