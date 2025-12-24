using UnityEngine;

public class PlayerAttack : EntityAttack
{
    protected void Start()
    {
        timer = coolDown;
    }
}