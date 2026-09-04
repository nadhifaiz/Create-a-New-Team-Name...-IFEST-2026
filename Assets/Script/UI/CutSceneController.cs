using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CutsceneController : MonoBehaviour
{
    [Header("Scene Navigation")]
    [Tooltip("Nama scene Main Menu yang mau dituju")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Cutscene Sequence")]
    [SerializeField] private CutsceneLine[] lines;

    [Header("UI Components")]
    [SerializeField] private Image backgroundDisplay;
    [SerializeField] private TMP_Text narrativeBodyText;
    [SerializeField] private GameObject clickIndicator; // Ikon panah tanda siap klik lanjut

    [Header("Buttons")]
    [SerializeField] private Button screenClickButton; // Tombol transparan full-screen

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed = 0.035f;

    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private void Start()
    {
        if (screenClickButton != null) screenClickButton.onClick.AddListener(OnAdvanceInput);

        currentLineIndex = 0;
        DisplayCurrentLine();
    }

    private void Update()
    {
        // Support tombol spasi atau enter di keyboard
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
        {
            OnAdvanceInput();
        }
    }

    public void OnAdvanceInput()
    {
        // 1. Kalau teks sedang mengetik -> Klik langsung menampilkan teks lengkap
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            narrativeBodyText.text = lines[currentLineIndex].narrativeText;
            isTyping = false;
            if (clickIndicator != null) clickIndicator.SetActive(true);
            return;
        }

        // 2. Kalau teks sudah lengkap -> Klik maju ke slide/kalimat berikutnya
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
        CutsceneLine line = lines[currentLineIndex];

        // Ganti ilustrasi/background jika ada
        if (backgroundDisplay != null && line.sceneImage != null)
        {
            backgroundDisplay.sprite = line.sceneImage;
        }

        if (clickIndicator != null) clickIndicator.SetActive(false);

        // Putar efek ketik teks narasi
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

[System.Serializable]
public class CutsceneLine
{
    [Tooltip("Gambar latar/ilustrasi untuk baris ini (opsional)")]
    public Sprite sceneImage;

    [TextArea(2, 5)]
    [Tooltip("Isi kalimat narasi")]
    public string narrativeText;
}