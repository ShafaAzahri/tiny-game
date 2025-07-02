using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoPlayerController : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "credit.mp4");
        videoPlayer.url = path;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu"); // ganti dengan scene menu kamu
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene("MainMenu");
    }
}
