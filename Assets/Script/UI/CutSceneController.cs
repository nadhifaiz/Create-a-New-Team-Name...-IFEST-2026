using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CutSceneController : MonoBehaviour
{
    [Header("Scene Navigation")]
    [Tooltip("Nama scene Main Menu yang mau dituju")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Cutscene Sequence")]
    [SerializeField] private CutsceneLineData[] lines;

    [Header("UI Components")]
    [SerializeField] private Image backgroundDisplay;
    [SerializeField] private TMP_Text narrativeBodyText;
    [SerializeField] private GameObject clickIndicator;

    [Header("Buttons")]
    [SerializeField] private Button screenClickButton;

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed = 0.035f;

    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private void Start()
    {
        if (screenClickButton != null) screenClickButton.onClick.AddListener(OnAdvanceInput);

        // Hapus PlayAudio() di sini agar BGM dari Main Menu tetap mengalun mulus tanpa restart

        currentLineIndex = 0;
        DisplayCurrentLine();
    }
    private void Update()
    {
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
        {
            OnAdvanceInput();
        }
    }

    public void OnAdvanceInput()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            narrativeBodyText.text = lines[currentLineIndex].narrativeText;
            isTyping = false;
            if (clickIndicator != null) clickIndicator.SetActive(true);
            return;
        }
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.PlayAudio(AudioSystem.Instance.sfx_HitPlay);
        }

        currentLineIndex++;

        if (currentLineIndex < lines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            FinishCutscene();
        }
    }
    private void DisplayCurrentLine()
    {
        CutsceneLineData line = lines[currentLineIndex];

        if (backgroundDisplay != null && line.sceneImage != null)
        {
            backgroundDisplay.sprite = line.sceneImage;
        }

        if (clickIndicator != null) clickIndicator.SetActive(false);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypewriterRoutine(line.narrativeText));
    }

    private IEnumerator TypewriterRoutine(string line)
    {
        isTyping = true;
        narrativeBodyText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            narrativeBodyText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        if (clickIndicator != null) clickIndicator.SetActive(true);
    }

    public void FinishCutscene()
    {
        MainMenuManager.cameFromCutscene = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

// Struct/Class internal untuk menampung data cutscene tanpa membuat file terpisah
[System.Serializable]
public struct CutsceneLineData
{
    [Tooltip("Gambar latar/ilustrasi untuk baris ini (opsional)")]
    public Sprite sceneImage;

    [TextArea(2, 5)]
    [Tooltip("Isi kalimat narasi")]
    public string narrativeText;
}