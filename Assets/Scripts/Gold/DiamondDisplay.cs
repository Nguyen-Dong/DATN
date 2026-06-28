using UnityEngine;
using TMPro;

/// <summary>
/// Gắn vào một Text (TextMeshPro) bất kỳ để LUÔN hiển thị đúng số kim cương hiện có.
/// Tự bám vào CurrencyManager (kể cả khi nó spawn muộn hoặc đổi instance qua scene) và
/// cập nhật theo sự kiện OnChanged nên không cần set text thủ công ở đâu cả.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class DiamondDisplay : MonoBehaviour
{
    [Tooltip("Định dạng hiển thị, {0} là số kim cương. Vd: \"{0}\" hoặc \"Kim cương: {0}\"")]
    [SerializeField] private string format = "{0}";

    private TMP_Text text;
    private CurrencyManager bound;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        Bind();
        Refresh();
    }

    private void OnDisable()
    {
        if (bound != null) bound.OnChanged -= Refresh;
        bound = null;
    }

    private void Update()
    {
        // CurrencyManager (DontDestroyOnLoad) có thể chưa tồn tại lúc Enable, hoặc bị thay instance khi đổi scene.
        // Luôn đảm bảo đang bám đúng instance hiện tại.
        if (CurrencyManager.Instance != bound)
        {
            Bind();
            Refresh();
        }
    }

    private void Bind()
    {
        if (CurrencyManager.Instance == bound) return;
        if (bound != null) bound.OnChanged -= Refresh;
        bound = CurrencyManager.Instance;
        if (bound != null) bound.OnChanged += Refresh;
    }

    private void Refresh()
    {
        if (text == null) return;
        int amount = CurrencyManager.Instance != null ? CurrencyManager.Instance.Diamonds : 0;
        text.text = string.Format(format, amount);
    }
}
