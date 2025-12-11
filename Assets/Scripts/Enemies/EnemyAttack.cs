using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float damage = 3;
    [SerializeField] protected float coolDown = 1;
    protected float timer = 1;
    [HideInInspector] public bool attacked; // used for the CanAttack method
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
