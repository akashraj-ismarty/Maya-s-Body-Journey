    using UnityEngine;
using System.Collections;
using TMPro; // Required if you're using TextMeshPro
using UnityEngine.UI;

public class MenstruationExplanation : MonoBehaviour
{
    [Header("Avatar and Text")]
    public Animator avatarAnimator;        // Drag your avatar's Animator component here in the Inspector
    public TextMeshProUGUI dialogueText;  // Drag your TextMeshPro Text object here

    [Header("Audio")]
    public AudioClip[] audioClips;         // Array to hold your audio clips.
    private AudioSource audioSource;       // The AudioSource component on this GameObject (Casual1)

    private bool isPaused = false;         // To track pause state

    [Header("Dialogue")]
    public string[] explanationSegments;  // Array of text segments.
    public float textSpeed = 0.05f;       // Speed at which the text appears
    public float initialDelay = 1f;       // Delay before the first segment starts
    private int currentSegmentIndex = 0; // Current line of dialogue
    private Coroutine typeTextCoroutine;   // Reference to the TypeText coroutine
    private bool isTextTypingFinished = true; // Flag to indicate when the current text has finished displaying

    [Header("UI Buttons")]
    public Button previousButton;
    public Button pauseResumeButton;
    public Button nextButton;
    public TextMeshProUGUI pauseResumeButtonText; // Optional: To change button text to "Pause"/"Resume"


    // Flag to indicate the entire explanation sequence is complete (showing "Thank you")
    private bool isExplanationComplete = false;


    void Start()
    {
        // Get the AudioSource component.  Add one if it doesn't exist.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Null checks for essential components
        if (dialogueText == null) Debug.LogError("DialogueText not assigned in MenstruationExplanation!");
        if (avatarAnimator == null) Debug.LogWarning("AvatarAnimator not assigned in MenstruationExplanation. Animations will not play.");
        if (previousButton == null) Debug.LogError("PreviousButton not assigned!");
        if (pauseResumeButton == null) Debug.LogError("PauseResumeButton not assigned!");
        if (nextButton == null) Debug.LogError("NextButton not assigned!");
        if (pauseResumeButtonText == null) Debug.LogWarning("PauseResumeButtonText not assigned. Button text will not update.");
        // Set up the UI buttons

        // Initialize the explanation segments.  This is your dialogue text.  Keep this consistent with your audioClips array.
        explanationSegments = new string[]
        {
            "Hi there! I’m Maya! I’m here to talk to you about something totally natural and super important – periods!",
            "Have you heard of them before? Don’t worry, I’ll walk you through it step-by-step!",
            "Every month, the body gets ready for a special event – like getting ready for a big celebration!",
            "But instead of a party, it’s your body getting ready to take care of a tiny egg inside your tummy.",
            "If the egg doesn’t need to be used, your body says, ‘Hey! Let’s clean up and get ready for the next month!’",
            "And that’s what a period is – just your body’s way of cleaning up and getting ready for the next cycle.",
            "During a period, you might feel some changes like cramps or tiredness. But don’t worry – there are ways to feel better!",
            "Remember, periods are a normal and healthy part of growing up. You’re amazing just the way you are!"
        };

        // Setup button listeners
        if (previousButton != null) previousButton.onClick.AddListener(GoToPreviousSegment);
        if (pauseResumeButton != null) pauseResumeButton.onClick.AddListener(TogglePauseResume);
        if (nextButton != null) nextButton.onClick.AddListener(GoToNextSegment);

        currentSegmentIndex = 0; // Start at the first segment
        StartCoroutine(StartExplanation()); // Start the explanation sequence
    }

    IEnumerator StartExplanation()
    {
        yield return new WaitForSeconds(initialDelay);
        if (explanationSegments.Length > 0)
        {
            PlaySegment(currentSegmentIndex);
        }
        else
        {
            if (dialogueText != null) dialogueText.text = "No explanation segments loaded.";
            isExplanationComplete = true;
            UpdateNavigationButtons();
        }
    }

    void PlaySegment(int segmentIndex)
    {
        if (typeTextCoroutine != null)
        {
            StopCoroutine(typeTextCoroutine);
            typeTextCoroutine = null;
        }
        audioSource.Stop();
        isPaused = false; // Reset pause state when changing segments


        currentSegmentIndex = segmentIndex;

        if (currentSegmentIndex >= 0 && currentSegmentIndex < explanationSegments.Length)
        {
            isExplanationComplete = false; // We are on a valid, active segment
            isTextTypingFinished = false;  // New text will start typing

            if (avatarAnimator != null) avatarAnimator.SetBool("IsTalking", true);

            // --- AUDIO PLAYBACK ---
            if (audioClips.Length > currentSegmentIndex && audioClips[currentSegmentIndex] != null)
            {
                audioSource.clip = audioClips[currentSegmentIndex];
                audioSource.Play();
            }
            if (dialogueText != null) dialogueText.text = ""; // Clear the text
            typeTextCoroutine = StartCoroutine(TypeText(explanationSegments[currentSegmentIndex]));
        }
        else
        {
            // Explanation is finished or index is out of bounds.
            if (dialogueText != null) dialogueText.text = "Thank you for listening!";
            if (avatarAnimator != null) avatarAnimator.SetBool("IsTalking", false);
            isTextTypingFinished = true;
            isExplanationComplete = true;
        }
        UpdateNavigationButtons();
    }
    public void GoToNextSegment()
    {
        if (currentSegmentIndex < explanationSegments.Length) // If not yet at "Thank you"
        {
            PlaySegment(currentSegmentIndex + 1); // Play next or "Thank you"
        }
    }

    public void GoToPreviousSegment()
    {
        if (currentSegmentIndex > 0)
        {
            PlaySegment(currentSegmentIndex - 1);
        }
    }

    public void TogglePauseResume()
    {
        if (isExplanationComplete) return; // Don't allow pause if explanation is fully done

        isPaused = !isPaused;
        if (isPaused)
        {
            audioSource.Pause();
            if (avatarAnimator != null) avatarAnimator.enabled = false; // Pause animation
            // TypeText coroutine will self-pause
        }
        else
        {
            audioSource.UnPause();
            if (avatarAnimator != null) avatarAnimator.enabled = true; // Resume animation
            // TypeText coroutine will self-resume
        }
        UpdateNavigationButtons();
    }

    void UpdateNavigationButtons()
    {
        // --- Previous Button ---
        if (previousButton != null)
        {
            // Interactable if not paused, and not the first segment.
            previousButton.interactable = currentSegmentIndex > 0 && !isPaused;
        }

        // --- Next Button ---
        if (nextButton != null)
        {
            // Interactable if not paused, and not currently showing the "Thank you" message.
            // (i.e., currentSegmentIndex is for an actual segment that has a "next")
            nextButton.interactable = currentSegmentIndex < explanationSegments.Length && !isPaused;
        }

        // --- Pause/Resume Button ---
        if (pauseResumeButton != null)
        {
            if (isPaused)
            {
                // If paused, button should allow resuming.
                pauseResumeButton.interactable = true;
                if (pauseResumeButtonText != null) pauseResumeButtonText.text = "Resume";
            }
            else // Not paused
            {
                // If not paused, button should allow pausing if content is actively playing.
                bool canBePaused = !isExplanationComplete && (audioSource.isPlaying || !isTextTypingFinished);
                pauseResumeButton.interactable = canBePaused;

                if (pauseResumeButtonText != null) pauseResumeButtonText.text = "Pause";
            }
        }
        if (isPaused && nextButton != null) nextButton.interactable = false; // Disable next/prev when paused
        if (isPaused && previousButton != null) previousButton.interactable = false;
    }


    // Coroutine to display the text character by character.
    IEnumerator TypeText(string text)
    {
        isTextTypingFinished = false;
        foreach (char letter in text.ToCharArray())
        {
            while (isPaused)
            {
                yield return null; // Wait a frame if paused
            }
            if (dialogueText != null) dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
        isTextTypingFinished = true; //  Text display is complete.
        // Avatar stops talking animation if audio is also done, or if no audio for this segment
        if (avatarAnimator != null && (!audioSource.isPlaying || audioSource.clip == null))
        {
            avatarAnimator.SetBool("IsTalking", false);
        }
        StartCoroutine(WaitForAudioAndFinalizeSegment());
    }
    IEnumerator WaitForAudioAndFinalizeSegment()
    {
        // Wait until the text has finished typing AND the audio clip has finished playing.
        while (!isTextTypingFinished || audioSource.isPlaying)
        {
            yield return null;
        }

        // Ensure avatar stops talking if it was set by TypeText due to no audio
        if (avatarAnimator != null)
        {
            avatarAnimator.SetBool("IsTalking", false);
        }
        UpdateNavigationButtons();
    }
    
        public void ExitApplication()
    {
        Debug.Log("ExitApplication requested. Quitting...");
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

}
