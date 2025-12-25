using UnityEngine;

public class UnitClick : MonoBehaviour
{
    private StatUpgrader _myUpgrade;
    private void Start()
    {
        _myUpgrade = GetComponent<StatUpgrader>();
    }
    private void OnMouseDown()
    {
        if (_myUpgrade != null && UpgradeUIManager.Instance != null)
        {
            UpgradeUIManager.Instance.OpenPanel(_myUpgrade);
        }
    }
}
