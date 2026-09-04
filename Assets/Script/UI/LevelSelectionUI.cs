using UnityEngine;

public class LevelSelectionUI : MonoBehaviour
{
    [SerializeField] private LevelItemUI[] levelItems;

    private void OnEnable()
    {
        RefreshLevelButtons();
    }

    public void RefreshLevelButtons()
    {
        if (DataManager.Instance == null) return;

        for (int i = 0; i < levelItems.Length; i++)
        {
            if (levelItems[i] != null)
            {
                int levelIndex = i + 1;
                LevelStats stats = DataManager.Instance.GetLevelStats(levelIndex);
                levelItems[i].Setup(stats);
            }
        }
    }
}