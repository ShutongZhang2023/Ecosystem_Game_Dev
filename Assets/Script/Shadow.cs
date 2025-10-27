using DG.Tweening;
using UnityEngine;
using System.Collections;

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

    [Header("Color Change")]
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private Color darkColor;
    [SerializeField] private Color lightColor;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private ShadowState currentState;
    private float spawnTimer;
    private Vector2 direction;
    private Vector2 moveDirection;

    [Header("Reproduction Settings")]
    [SerializeField] private GameObject shadowPrefab;
    [SerializeField] private float reproduceCooldown = 2f;
    public bool canReproduce = true;

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

    private void Update()
    {
        if (currentState == ShadowState.Move)
        {
            bool inLight = Physics2D.OverlapPoint(transform.position, lightMask);
            Color targetColor = inLight ? lightColor : darkColor;

            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, fadeSpeed * Time.deltaTime);
        }
    }

    private void EnterSpawn() { 
        currentState = ShadowState.Spawn;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);

        moveDirection = Random.insideUnitCircle.normalized;
        rb.linearVelocity = moveDirection * speed;

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
        Shadow other = collision.gameObject.GetComponent<Shadow>();
        bool isMaxShadow = GameManager.Instance.shadowCount < GameManager.Instance.maxShadow;
        if (other != null && canReproduce && other.canReproduce && isMaxShadow)
        {
            Vector2 spawnPos = (transform.position + other.transform.position) / 2f;
            GameObject newShadow = Instantiate(shadowPrefab, spawnPos, Quaternion.identity);
            GameManager.Instance.shadowCount++;
            Shadow shadowScript = newShadow.GetComponent<Shadow>();
            shadowScript.StartSpawnScale();
            shadowScript.DisableReproduceTemporarily();
            DisableReproduceTemporarily();
        }

        Vector2 normal = collision.contacts[0].normal;
        moveDirection = Vector2.Reflect(moveDirection, normal);
        rb.linearVelocity = moveDirection * speed;
    }

    public void StartSpawnScale()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(0.2f, 1f).SetEase(Ease.OutBack);
    }

    public void DisableReproduceTemporarily()
    {
        canReproduce = false;
        StartCoroutine(EnableReproduce());
    }

    private IEnumerator EnableReproduce() {
        yield return new WaitForSeconds(reproduceCooldown);
        canReproduce = true;
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
        currentState = ShadowState.Die;
        transform.DOKill();
        spriteRenderer.DOFade(0f, fadeTime).OnComplete(() =>
        {
            rb.linearVelocity = Vector2.zero;
            GameManager.Instance.shadowCount--;
            GameObject.Destroy(gameObject);
        });
    }



}