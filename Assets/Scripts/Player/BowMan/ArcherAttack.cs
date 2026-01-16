using UnityEngine;

public class ArcherAttack : PlayerAttack
{
    private Archer archer;
    public Animator animator;

    [Header("Ranged Config")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;

    [Header("Ballistic Config")]
    [SerializeField] private float axesX = 15f;
    [SerializeField] private float axesY = 10f;
    [SerializeField] private LayerMask enemyLayer;


    [SerializeField] private float detectRange = 10f;

    [HideInInspector] public bool attackeing;

    private void Start()
    {
        archer = GetComponentInParent<Archer>();
        timer = coolDown;
    }

    private void Update()
    {
        if (!archer.CanMove()) return;
        bool enemyDetected = DetectEnemy();

        // Cập nhật Animator (Trạng thái Ready bắn cung)
        animator.SetBool("Ready", enemyDetected);
        animator.SetInteger("WeaponType", 3);
        if (enemyDetected)
        {
            if (enemyDetected && CanAttack())
            {
                Attack();
            }
        }
    }
    private void Attack()
    {
        attacked = true;
        attackeing = true;

        //animation ban
        animator.SetTrigger("SimpleBowShot");

    }
    public void ShootEvent()
    {
        if(arrowPrefab!= null && shootPoint != null)
        {
            GameObject gameObject = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
            Arrow arrow = gameObject.GetComponent<Arrow>();

            float direction = transform.parent.localScale.x;
            Vector2 velocity = new Vector2(axesX * direction, axesY);

            arrow.Initialize(damage, velocity);
        }
    }
    public bool DetectEnemy()
    {
        Vector2 direction = (transform.parent.localScale.x > 0) ? Vector2.right : Vector2.left;
        Debug.DrawRay(shootPoint.position, direction * detectRange, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(shootPoint.position, direction, detectRange, enemyLayer);

        return hit.collider != null;
    }
}
