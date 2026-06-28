using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class ClassicMapController : MonoBehaviour
{
    [Header("UI Map")]
    [SerializeField] public GameObject mapPanel;
    [SerializeField] public GameObject modeSelectionPanel;

    [Header("Popup Info")]
    [SerializeField] public GameObject levelInfoPopup;
    [SerializeField] public TextMeshProUGUI levelNameText;
    [SerializeField] public Button startGameButton;
    [SerializeField] public Button closePopupButton;

    [Header("Checkpoint")]
    [SerializeField] public Button[] levelButtons;
    [Tooltip("Tên scene từng màn (index khớp levelButtons). Vd [0]=Classic_map1, [1]=Classic_map2...")]
    [SerializeField] private string[] levelScenes;
    [Tooltip("Tên hiển thị từng màn (tùy chọn, index khớp levelButtons)")]
    [SerializeField] private string[] levelNames;
    [SerializeField] private int currentSelectedLevel = -1;
    [SerializeField] private int highestUnlockLevel = 0;

    private void Start()
    {
        highestUnlockLevel = PlayerPrefs.GetInt("HighestUnlockLevel", 0);

        startGameButton.onClick.AddListener(LoadSelectedLevelScene);
        closePopupButton.onClick.AddListener(() => levelInfoPopup.SetActive(false));
    }
    public void OpenMap()
    {
        // Reload in case it was updated after winning a game
        highestUnlockLevel = PlayerPrefs.GetInt("HighestUnlockLevel", 0);

        mapPanel.SetActive(true);
        modeSelectionPanel.SetActive(false);
        levelInfoPopup.SetActive(false);

        UpdateMapUI();
    }
    public void CloseMap()
    {
        mapPanel.SetActive(false);
        modeSelectionPanel.SetActive(true);
    }

    private void UpdateMapUI()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i;
            if (i <= highestUnlockLevel)
            {
                levelButtons[i].interactable = true;

                levelButtons[i].onClick.RemoveAllListeners();
                levelButtons[i].onClick.AddListener(() => OnLevelPointClicked(levelIndex));
            }
            else
            {
                levelButtons[i].interactable = false;
            }
        }
    }
    private void OnLevelPointClicked(int levelIndex)
    {
        currentSelectedLevel = levelIndex;
        levelInfoPopup.SetActive(true);

        if (levelNames != null && levelIndex < levelNames.Length && !string.IsNullOrEmpty(levelNames[levelIndex]))
            levelNameText.text = levelNames[levelIndex];
        else
            levelNameText.text = levelIndex == 0 ? "MAM HUONG DAN" : "MAN CHOI" + levelIndex;
    }
    private void LoadSelectedLevelScene()
    {
        // Lưu lại level hiện tại để GameResult biết đang chơi level nào
        PlayerPrefs.SetInt("CurrentPlayingLevel", currentSelectedLevel);
        PlayerPrefs.Save();

        if (currentSelectedLevel >= 0 && levelScenes != null
            && currentSelectedLevel < levelScenes.Length
            && !string.IsNullOrEmpty(levelScenes[currentSelectedLevel]))
        {
            SceneManager.LoadScene(levelScenes[currentSelectedLevel]);
        }
        else
        {
            Debug.LogWarning($"ClassicMapController: chưa gán scene cho màn {currentSelectedLevel} (kiểm tra mảng 'Level Scenes').");
        }
    }
}
