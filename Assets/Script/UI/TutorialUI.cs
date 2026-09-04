using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManagerUI : MonoBehaviour
{
    [Header("Slide Data")]
    [SerializeField] private TutorialSlide[] slides;

    [Header("UI Display")]
    [SerializeField] private Image tutorialDisplayImage;
    [SerializeField] private TMP_Text tutorialDescriptionText;

    [Header("Navigation Buttons")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button playButton;

    [Header("References")]
    [Tooltip("Tarik script MainMenuManager di sini")]
    [SerializeField] private MainMenuManager mainMenuManager;

    private int currentSlideIndex = 0;

    // Hapus Awake, atau kosongkan isinya
    // Pindahkan pendaftaran tombol ke OnEnable
    private void OnEnable()
    {
        // Daftarkan aksi tombol setiap kali panel muncul
        if (prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(OnPrevClicked);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextClicked);
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);
        }

        // Reset slide ke index 0
        currentSlideIndex = 0;
        UpdateSlideUI();
    }

    private void OnDisable()
    {
        // Bersihkan listener saat panel ditutup
        if (prevButton != null) prevButton.onClick.RemoveAllListeners();
        if (nextButton != null) nextButton.onClick.RemoveAllListeners();
        if (playButton != null) playButton.onClick.RemoveAllListeners();
    }

    public void OnNextClicked()
    {
        Debug.Log(">>> TOMBOL NEXT KEPENCET! <<<");
        if (currentSlideIndex < slides.Length - 1)
        {
            PlaySFX();
            currentSlideIndex++;
            UpdateSlideUI();
        }
    }

    public void OnPrevClicked()
    {
        if (currentSlideIndex > 0)
        {
            PlaySFX();
            currentSlideIndex--;
            UpdateSlideUI();
        }
    }

    private void OnPlayClicked()
    {
        PlaySFX();

        // Tandai tutorial selesai dan buka Level Selection di MainMenuManager
        if (mainMenuManager != null)
        {
            mainMenuManager.OnFinishTutorial();
        }
        else if (DataManager.Instance != null)
        {
            DataManager.Instance.CompleteTutorial();
            gameObject.SetActive(false);
        }
    }

    private void UpdateSlideUI()
    {
        if (slides == null || slides.Length == 0) return;

        // 1. Update Konten Slide
        TutorialSlide current = slides[currentSlideIndex];
        if (tutorialDisplayImage != null && current.slideImage != null)
        {
            tutorialDisplayImage.sprite = current.slideImage;
        }

        if (tutorialDescriptionText != null)
        {
            tutorialDescriptionText.text = current.descriptionText;
        }

        // 2. Cek Kondisi Posisi Slide
        bool isFirstSlide = currentSlideIndex == 0;
        bool isLastSlide = currentSlideIndex == slides.Length - 1;

        // 3. Atur Visibilitas Tombol Navigasi
        if (prevButton != null)
        {
            prevButton.gameObject.SetActive(!isFirstSlide);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(!isLastSlide);
        }

        // Tombol Play HANYA aktif di slide paling akhir
        if (playButton != null)
        {
            playButton.gameObject.SetActive(isLastSlide);
        }
    }

    private void PlaySFX()
    {
        if (AudioSystem.Instance != null)
        {
            AudioSystem.PlaySfx(AudioSystem.Instance.sfx_HitPlay);
        }
    }
}

[System.Serializable]
public class TutorialSlide
{
    [Tooltip("Gambar/ilustrasi tutorial untuk slide ini")]
    public Sprite slideImage;

    [TextArea(2, 5)]
    [Tooltip("Teks deskripsi tutorial (pengganti 'lorem ipsum dolor')")]
    public string descriptionText;
}