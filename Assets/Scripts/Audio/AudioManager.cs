using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý bật/tắt âm thanh. Gắn 1 cái RIÊNG cho mỗi scene (KHÔNG DontDestroyOnLoad).
/// Trạng thái bật/tắt lưu bằng PlayerPrefs, nên mỗi scene tự đọc lại lúc Awake -> giữ nguyên qua các scene.
/// Nút bấm: text TMP hiển thị "Tắt Âm thanh" khi đang bật, "Bật Âm thanh" khi đang tắt.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private const string KEY = "SoundEnabled";

    [Header("Audio")]
    [Tooltip("AudioSource cần bật/tắt theo trạng thái nút (vd nhạc nền)")]
    [SerializeField] private AudioSource audioSource;

    [Header("UI (tùy chọn)")]
    [SerializeField] private Button toggleButton;
    [Tooltip("Text (TMP) bên trong nút")]
    [SerializeField] private TMP_Text toggleLabel;
    [Tooltip("Hiển thị khi âm thanh ĐANG BẬT (bấm để tắt)")]
    [SerializeField] private string textWhenOn = "Tắt Âm thanh";
    [Tooltip("Hiển thị khi âm thanh ĐANG TẮT (bấm để bật)")]
    [SerializeField] private string textWhenOff = "Bật Âm thanh";

    /// <summary>Trạng thái âm thanh hiện tại (đọc/ghi PlayerPrefs - dùng chung mọi scene).</summary>
    public static bool SoundEnabled
    {
        get => PlayerPrefs.GetInt(KEY, 1) == 1;
        private set { PlayerPrefs.SetInt(KEY, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    private void Awake()
    {
        // Load realtime trạng thái đã lưu và áp dụng cho scene hiện tại
        Apply(SoundEnabled);
    }

    private void Start()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(Toggle);
        RefreshLabel();
    }

    /// <summary>Đảo trạng thái bật/tắt.</summary>
    public void Toggle() => SetSound(!SoundEnabled);

    /// <summary>Đặt trạng thái bật/tắt âm thanh (API dùng được từ code khác).</summary>
    public void SetSound(bool enabled)
    {
        SoundEnabled = enabled;
        Apply(enabled);
        RefreshLabel();
    }

    private void Apply(bool enabled)
    {
        // Bật/tắt toàn bộ âm thanh game qua AudioListener
        AudioListener.volume = enabled ? 1f : 0f;

        // Bật/tắt AudioSource được tham chiếu theo trạng thái nút
        if (audioSource != null)
        {
            audioSource.mute = !enabled;
            if (enabled && !audioSource.isPlaying && audioSource.clip != null)
                audioSource.Play();
        }
    }

    private void RefreshLabel()
    {
        if (toggleLabel != null)
            toggleLabel.text = SoundEnabled ? textWhenOn : textWhenOff;
    }
}
