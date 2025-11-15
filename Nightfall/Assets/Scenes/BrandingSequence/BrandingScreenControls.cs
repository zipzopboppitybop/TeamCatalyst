using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class BrandingScreenControls : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;

    private bool hasSkipped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoOver;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if(!hasSkipped && Input.GetKeyDown(KeyCode.Escape))
        {
            hasSkipped = true;
            LoadMainMenu();
        }
    }

    private void OnVideoOver(VideoPlayer vPlayer)
    {
        if (!hasSkipped)
        {
            hasSkipped = true;
            LoadMainMenu();
        }
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene(1);
    }
}
