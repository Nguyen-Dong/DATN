using UnityEngine;

public class Sword : Player
{
    public SwordAttack attack;
    public PlayerMovement movement;
    public ParticleSystem takeDamgeParticles;

    protected override void Start()
    {
        base.Start();
        movement = transform.Find("Movement").GetComponent<PlayerMovement>();
        attack = transform.Find("Attack").GetComponent<SwordAttack>();
        takeDamgeParticles = transform.GetComponentInChildren<ParticleSystem>();
    }
    public override void TakeDamage(float damage)
    {
        if (dead) return;
        base.TakeDamage(damage);
        takeDamgeParticles.Play();
    }
}
