using DG.Tweening;
using UnityEngine;

public enum ShadowState
{
    Spawn,
    Move,
    Die
}
public class Shadow : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;
    [SerializeField] private LayerMask lightMask;
    [SerializeField] private float fadeTime = 0.8f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private ShadowState currentState;
    private float spawnTimer;
    private Vector2 direction;
    private Vector2 moveDirection;

    [Header("Reproduction Settings")]
    [SerializeField] private GameObject shadowPrefab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;
        speed = Random.Range(1.5f, 3f);
        lifeTime = Random.Range(50f, 80f);
    }

    private void Start()
    {
        EnterSpawn();
    }

    private void EnterSpawn() { 
        currentState = ShadowState.Spawn;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);

        moveDirection = Random.insideUnitCircle.normalized;
        rb.linearVelocity = moveDirection * speed;

        Debug.Log("Direction: " + moveDirection + " Velocity: " + rb.linearVelocity);

        spriteRenderer.DOFade(0.4f, fadeTime).OnComplete(() =>
        {
            EnterMove();
        });

        transform.DORotate(new Vector3(0f, 0f, 360f), 5f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);

        Invoke(nameof(EnterDie), lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 normal = collision.contacts[0].normal;
        moveDirection = Vector2.Reflect(moveDirection, normal);
        // moveDirection = -moveDirection;

        rb.linearVelocity = moveDirection * speed;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * speed;
    }

    private void EnterMove()
    {
        currentState = ShadowState.Move;
    }

    private void EnterDie()
    {

    }



}