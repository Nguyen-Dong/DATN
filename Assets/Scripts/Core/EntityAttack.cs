using UnityEngine;

public class EntityAttack : MonoBehaviour
{
    [Header("Base Stats")]
    public float damage = 10f;
    [SerializeField] protected float coolDown = 1f;

    protected float timer = 0f;
    [HideInInspector] public bool attacked;
    protected bool CanAttack()
    {
        if (attacked)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                return false;
            }
            else
            {
                attacked = false;
                timer = coolDown;
                return true;
            }
        }
        else
        {
            return true;
        }
    }
}
