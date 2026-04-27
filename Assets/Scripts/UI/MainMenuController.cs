using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] public GameObject mainMenuPanel;
    [SerializeField] public GameObject modelSelectionPanel;

    [Header("Buttons")]
    [SerializeField] public Button playGameButton;
    [SerializeField] public Button shopGameButton;
    [SerializeField] public Button settingGameButton;

    [Header("Buttons Model")]
    [SerializeField] public Button classicButton;
    [SerializeField] public Button survivalButton;
    [SerializeField] public Button backButton;

    private void Start()
    {
        // Set up button listeners
        playGameButton.onClick.AddListener(ShowModeSelection);
        backButton.onClick.AddListener(ShowMainMenu);

        classicButton.onClick.AddListener(LoadClassicMode);
        survivalButton.onClick.AddListener(LoadSurvivalMode);

        shopGameButton.onClick.AddListener(() => Debug.Log("Mo giao dien cua hang"));
        settingGameButton.onClick.AddListener(() => Debug.Log("Mo giao dien cai dat"));

        if (PlayerPrefs.GetInt("ReturnToMap", 0) == 1)
        {
            PlayerPrefs.SetInt("ReturnToMap", 0);
            GetComponent<ClassicMapController>().OpenMap();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        modelSelectionPanel.SetActive(false);
    }
    private void ShowModeSelection()
    {
        mainMenuPanel.SetActive(false);
        modelSelectionPanel.SetActive(true);
    }
    private void LoadClassicMode()
    {
        GetComponent<ClassicMapController>().OpenMap();
    }
    private void LoadSurvivalMode()
    {
        Debug.Log("Load Survival Mode");
        //SceneManager.LoadScene("SurvivalModeScene");
    }
}
