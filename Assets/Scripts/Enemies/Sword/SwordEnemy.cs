using Assets.HeroEditor.Common.CharacterScripts;
using UnityEngine;

public class SwordEnemy : Enemy
{
    public SwordEnemyAttack attack;
    public AnimationEvents animationEvent;
    public EnemyMovement movement;
    [SerializeField] private int goldDrop = 20;

    protected override void Start()
    {
        base.Start();
        animationEvent = transform.GetComponentInChildren<AnimationEvents>();
        movement = transform.Find("Movement").GetComponent<EnemyMovement>();
        attack = transform.Find("Attack").GetComponent<SwordEnemyAttack>();

        animationEvent.OnCustomEvent += (string eventName) =>
        {
            if (eventName == "attack")
            {
                attack.AttackEven();
            }
        };
    }
    public override void TakeDamage(float damage)
    {
        if (dead) return;
        base.TakeDamage(damage);
    }

    public override void EntityDie()
    {
        if(!dead)
        {
            CurrencyManager.Instance.AddGold(goldDrop);
        }
        base.EntityDie();
        attack.animator.SetInteger("State", 7);
        float timetoDestroy = attack.animator.GetCurrentAnimatorStateInfo(0).length;
        if (timetoDestroy <= 0) timetoDestroy = 1f;
        Destroy(gameObject, timetoDestroy);
    }
}
