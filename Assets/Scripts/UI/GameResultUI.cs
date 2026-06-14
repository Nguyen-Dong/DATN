using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameResultUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;

    [Header("Buttons")]
    public Button continueButton;
    public Button replayButton;

    private Vector2 originalContinuePos;
    private Vector2 originalReplayPos;

    private void Start()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // Căn giữa tiêu đề text và reset vị trí X của RectTransform tiêu đề về 0
        if (resultTitleText != null)
        {
            resultTitleText.alignment = TextAlignmentOptions.Center;
            RectTransform titleRect = resultTitleText.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                Vector2 pos = titleRect.anchoredPosition;
                pos.x = 0f;
                titleRect.anchoredPosition = pos;
            }
        }

        // Lưu lại vị trí ban đầu của các nút
        if (continueButton != null)
        {
            originalContinuePos = continueButton.GetComponent<RectTransform>().anchoredPosition;
        }
        if (replayButton != null)
        {
            originalReplayPos = replayButton.GetComponent<RectTransform>().anchoredPosition;
        }

        // Đăng ký sự kiện
        if (GameResult.Instance != null)
        {
            GameResult.Instance.OnVictory += HandleVictory;
            GameResult.Instance.OnDefeat += HandleDefeat;
        }

        continueButton.onClick.AddListener(OnContinueClicked);
        replayButton.onClick.AddListener(OnReplayClicked);
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện
        if (GameResult.Instance != null)
        {
            GameResult.Instance.OnVictory -= HandleVictory;
            GameResult.Instance.OnDefeat -= HandleDefeat;
        }
    }

    private void HandleVictory()
    {
        resultPanel.SetActive(true);
        resultTitleText.text = "CHIẾN THẮNG";
        resultTitleText.color = Color.yellow;

        // Chỉ hiện nút Tiếp tục khi thắng
        replayButton.gameObject.SetActive(false);

        // Căn giữa nút Tiếp tục theo chiều ngang (X = 0)
        if (continueButton != null)
        {
            RectTransform rect = continueButton.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
            }
        }
    }

    private void HandleDefeat()
    {
        resultPanel.SetActive(true);
        resultTitleText.text = "THUA CUỘC";
        resultTitleText.color = Color.red;

        // Hiện cả 2 nút
        replayButton.gameObject.SetActive(true);

        // Khôi phục vị trí ban đầu cho cả 2 nút
        if (continueButton != null)
        {
            RectTransform rect = continueButton.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = originalContinuePos;
            }
        }
        if (replayButton != null)
        {
            RectTransform rect = replayButton.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = originalReplayPos;
            }
        }
    }

    private void OnContinueClicked()
    {
        Time.timeScale = 1f; // Resume time

        // Trở về Main menu, set flag để bật sẵn giao diện Map
        PlayerPrefs.SetInt("ReturnToMap", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Main");
    }

    private void OnReplayClicked()
    {
        Time.timeScale = 1f; // Resume time
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
