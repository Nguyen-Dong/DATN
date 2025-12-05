using UnityEngine;
public class Player : Entity
{
    [HideInInspector] public bool lieDown;

    public void StartLie(float time = 1)
    {
        lieDown = true;
        //rb.bodyType = RigidbodyType2D.Static;
        Invoke(nameof(EndLie), time);
    }
    private void EndLie()
    {
        lieDown = false;
        rb.AddForce(Vector3.up * 450);
        //rb.bodyType = RigidbodyType2D.Dynamic;
    }
    public bool CanMove()
    {
        if (dead || lieDown) return false;
        else return true;
    }
    //private static Player instance;
    //public static Player Instance { get { return instance; } }

    //[HideInInspector] public PlayerMovement movement;
    //[HideInInspector] public PlayerAttack attack;

    //protected override void Start()
    //{
    //    base.Start();
    //    movement = GetComponent<PlayerMovement>();
    //    attack = GetComponent<PlayerAttack>();
    //}

    //private void Awake()
    //{
    //    if (instance != null)
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }
    //    instance = this;
    //}

    //private void Update()
    //{
    //    if (CanMove())
    //    {
    //        if (attack != null && attack.IsAttacking)
    //        {
    //            movement.Move(0); // Stop movement
    //        }
    //        else
    //        {
    //            movement.Move(1); // Move right
    //        }
    //    }
    //}

    //public override void TakeDamage(float damage)
    //{
    //    if (dead) return;
    //    base.TakeDamage(damage);
    //    if (!dead)
    //    {
    //        //visual.animator.SetTrigger("HitTrigger");
    //        //EntityStun(TimeAnimationClip(visual.animator, "Hit"));
    //    }
    //}

    //public bool CanMove()
    //{
    //    if (dead) return false;
    //    else return true;
    //}
}