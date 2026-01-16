using Assets.HeroEditor.Common.CharacterScripts;
using UnityEngine;

public class Archer: Player
{
    public ArcherAttack attack;
    public AnimationEvents animationEvent;
    public PlayerMovement movement;

    protected override void Start()
    {
        base.Start();
        animationEvent = transform.GetComponentInChildren<AnimationEvents>();
        movement = transform.Find("Movement").GetComponent<PlayerMovement>();
        attack = transform.Find("Attack").GetComponent<ArcherAttack>();
        animationEvent.OnCustomEvent += (string eventName) =>
        {
            if (eventName == "shoot")
            {
                attack.ShootEvent();
            }
        };
    }
    public override void TakeDamage(float damage)
    {
        if (dead) return;
        base.TakeDamage(damage);
    }
}
