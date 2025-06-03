using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    public GameObject explanationDialogueCanvas;
    public MenstruationExplanation menstruationExplanation;
    public GameObject avatarGameObject;
    public Button playPauseButton;
    public Button closeButton; // Add this line

    private bool isPlaying = false;

    void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer not assigned to VideoController!");
            enabled = false;
            return;
        }

        if (videoDisplay == null)
        {
            Debug.LogError("VideoDisplay RawImage not assigned to VideoController!");
            enabled = false;
            return;
        }

        videoDisplay.gameObject.SetActive(false);
        if (explanationDialogueCanvas != null)
        {
            explanationDialogueCanvas.SetActive(true);
        }

        videoPlayer.loopPointReached += OnVideoFinished;

        if (playPauseButton == null)
        {
            Debug.LogError("PlayPauseButton not assigned in VideoController!");
            return;
        }
        playPauseButton.onClick.AddListener(TogglePlayPause);
        playPauseButton.gameObject.SetActive(false);

        // --- NEW: Close Button ---
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseApplication);
        }
        else
        {
            Debug.LogError("CloseButton not assigned in VideoController!");
        }
        // --- END NEW ---
    }



    public void StartStoryVideo()
    {
        if (videoPlayer.isPlaying)
            return;

        if (explanationDialogueCanvas != null)
        {
            explanationDialogueCanvas.SetActive(false);
        }
        if (menstruationExplanation != null)
        {
            menstruationExplanation.StopAllCoroutines();
            menstruationExplanation.enabled = false;
        }

        videoDisplay.gameObject.SetActive(true);
        videoPlayer.Play();
        Debug.Log("Video started.");

        if (avatarGameObject != null)
        {
            avatarGameObject.SetActive(false);
        }
        isPlaying = true;
        if (playPauseButton != null)
            playPauseButton.gameObject.SetActive(true);
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        videoDisplay.gameObject.SetActive(false);
        videoPlayer.Stop();
        Debug.Log("Video finished.");

        if (explanationDialogueCanvas != null)
        {
            explanationDialogueCanvas.SetActive(true);
        }
        if (menstruationExplanation != null)
        {
            menstruationExplanation.enabled = true;
        }

        if (avatarGameObject != null)
        {
            avatarGameObject.SetActive(true);
        }
        isPlaying = false;
        if (playPauseButton != null)
            playPauseButton.gameObject.SetActive(false);
    }

    void TogglePlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            isPlaying = false;
            Debug.Log("Video paused.");
        }
        else
        {
            videoPlayer.Play();
            isPlaying = true;
            Debug.Log("Video playing.");
        }
    }

    // --- NEW: Function to close the application ---
    void CloseApplication()
    {
        Debug.Log("Application Quit Requested");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    // --- END NEW ---
}
