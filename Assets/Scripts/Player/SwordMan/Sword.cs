using Assets.HeroEditor.Common.CharacterScripts;
using UnityEngine;

public class Sword : Player
{
    public SwordAttack attack;
    public AnimationEvents animationEvent;
    public PlayerMovement movement;

    protected override void Start()
    {
        base.Start();
        animationEvent = transform.GetComponentInChildren<AnimationEvents>();
        movement = transform.Find("Movement").GetComponent<PlayerMovement>();
        attack = transform.Find("Attack").GetComponent<SwordAttack>();

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
        base.EntityDie();
        attack.animator.SetInteger("State", 7);
        float timetoDestroy = attack.animator.GetCurrentAnimatorStateInfo(0).length;
        if (timetoDestroy <= 0) timetoDestroy = 1f;
        Destroy(gameObject, timetoDestroy);
    }
}
