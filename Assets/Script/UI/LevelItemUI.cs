using UnityEngine;
using UnityEngine.UI;

public class LevelItemUI : MonoBehaviour
{
    [Header("Level Info")]
    [SerializeField] private int levelIndex = 1;
    [SerializeField] private string targetSceneName = "GameplayScene"; // Nama scene level ini

    [Header("Sprites")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite lockedSprite;

    [Header("Button")]
    [SerializeField] private Button levelButton;

    [Header("Stars (Array 3 Bintang)")]
    [SerializeField] private GameObject[] stars;

    [Header("Star Accuracy Thresholds")]
    [SerializeField] private float star1Accuracy = 50f;
    [SerializeField] private float star2Accuracy = 75f;
    [SerializeField] private float star3Accuracy = 90f;

    public void Setup(LevelStats stats)
    {
        bool isUnlocked = stats != null && stats.isUnlocked;

        if (levelButton != null) levelButton.interactable = isUnlocked;
        if (buttonImage != null)
        {
            buttonImage.sprite = isUnlocked ? unlockedSprite : lockedSprite;
        }

        if (!isUnlocked || stats == null || stats.bestAccuracy <= 0)
        {
            SetStarCount(0);
        }
        else
        {
            int starCount = 0;
            if (stats.bestAccuracy >= star3Accuracy) starCount = 3;
            else if (stats.bestAccuracy >= star2Accuracy) starCount = 2;
            else if (stats.bestAccuracy >= star1Accuracy) starCount = 1;

            SetStarCount(starCount);
        }

        if (levelButton != null)
        {
            levelButton.onClick.RemoveAllListeners();
            if (isUnlocked)
            {
                levelButton.onClick.AddListener(OnLevelClicked);
            }
        }
    }

    private void SetStarCount(int count)
    {
        if (stars == null) return;
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].SetActive(i < count);
            }
        }
    }

    private void OnLevelClicked()
    {

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartLevel(levelIndex, targetSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }
}