using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityMovement : MonoBehaviour
{
    [SerializeField] public float speed;
    [SerializeField] protected float jumpForce;
    [SerializeField] protected GameObject pointGroundCheck;
    [SerializeField] protected GameObject pointWallCheck;
    [SerializeField] protected Vector2 sizeBoxCheckGround;
    [SerializeField] protected Vector2 sizeBoxCheckWall;

    public abstract void Move(float direction);
    public abstract bool GroundCheck();
    public abstract bool WallCheck();

}