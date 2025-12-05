using UnityEngine;

public class SwordMovement : PlayerMovement
{
    protected Sword sword;
    protected int oldDirection;
    protected void Start()
    {
        Load();
        sword = GetComponentInParent<Sword>();
    }
    void Update()
    {
        if (!sword.CanMove()) return;
        if (sword.attack.DetectPlayer()) return;
        //if (ChangeDirectionCheck())
        //    directionMove *= -1;

        Move(directionMove);
    }
}
