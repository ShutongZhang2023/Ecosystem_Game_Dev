using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using UnityEngine.Rendering;

public enum LightBugState
{
    Flying,
    Captured,
    Dead,
    End
}
public class LightBug : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private Vector2 areaSize = new Vector2(18f, 10f);
    [SerializeField] private float minSwitchTime = 1.5f;
    [SerializeField] private float maxSwitchTime = 4f;
    [SerializeField] private float orbitSpeed;

    [Header("Life Settings")]
    [SerializeField] private float lifeSpan;
    [SerializeField] private float shineSpan;

    private Vector2 targetPos;
    private float switchTimer;
    private float currentSwitchTime;

    [Header("Capture Settings")]
    [SerializeField] private float captureDelay =5f;
    private bool canBeCaptured = false;

    public LightBugState currentState = LightBugState.Flying;
    public Flower targetFlower;
    private Rigidbody2D rb;
    private Light2D bugLight;
    private SpriteRenderer spriteRenderer;
    private Coroutine currentCoroutine;
    private Tweener lightTween;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bugLight = GetComponent<Light2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        generateLightBug();
    }

    public void generateLightBug() {
        float x = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
        float y = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
        transform.position = new Vector2(x, y);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
        bugLight.intensity = 0f;
        orbitSpeed = Random.Range(90f, 120);
        lifeSpan = Random.Range(45f, 75f);
        shineSpan = Random.Range(1f, 2f);
        currentCoroutine = null;
        targetFlower = null;
        canBeCaptured = false;
        changeToFly();
        StartCoroutine(captureEnable());
        StartCoroutine(Dying());
    }

    private void Flying()
    {
        Vector2 dir = (targetPos - rb.position).normalized;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, dir * speed, 0.1f);
        switchTimer += Time.deltaTime;

        if (switchTimer > currentSwitchTime) {
            PickNewTarget();
        }

        if (Vector2.Distance(rb.position, targetPos) <= 0.3f)
        {
            PickNewTarget();
        }
    }

    private void PickNewTarget()
    {
        float x = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
        float y = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
        targetPos = new Vector2(x, y);

        currentSwitchTime = Random.Range(minSwitchTime, maxSwitchTime);
        switchTimer = 0f;
    }

    private void Update()
    {
        switch (currentState)
        {
            case LightBugState.Flying:
                Flying();
                break;
        }
    }

    public void changeToFly() {
        lightTween?.Kill();
        spriteRenderer.DOFade(1f, 1f);
        lightTween = DOTween.To(() => bugLight.intensity, x => bugLight.intensity = x, 5f, shineSpan).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        currentState = LightBugState.Flying;
    }

    public IEnumerator captureEnable() {
        yield return new WaitForSeconds(captureDelay);
        canBeCaptured = true;
    }

    public void SetTargetFlower(Flower flower)
    {
        if (!canBeCaptured || flower == null) return;
        targetFlower = flower;
        targetFlower.OnFlowerWithered += Release;
        currentState = LightBugState.Captured;
        rb.linearVelocity = Vector2.zero;

        float stopDistance = Random.Range(0.3f, 1f);
        Vector3 flowerPosition = flower.transform.position;
        Vector3 direction = (transform.position - flowerPosition).normalized;
        Vector3 targetPos = flowerPosition + direction * stopDistance;
        float travelTime = Vector3.Distance(transform.position, targetPos) / speed;
        transform.DOMove(targetPos, travelTime).SetEase(Ease.InOutSine).OnComplete(() => {
            rb.linearVelocity = Vector2.zero;
            if (this != null && gameObject.activeInHierarchy)
            {
                currentCoroutine = StartCoroutine(moveAroundFlower());
            }
        });
    }

    private IEnumerator moveAroundFlower() 
    {
        if (targetFlower == null) yield break;

        float radius = Vector2.Distance(transform.position, targetFlower.transform.position);
        float startAngle = Vector2.SignedAngle(Vector2.right, (transform.position - targetFlower.transform.position));
        float angle = startAngle;

        while (targetFlower != null) { 
            angle += orbitSpeed * Time.deltaTime;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
            transform.position = targetFlower.transform.position + offset;
            yield return null;
        }
    }

    public void Release()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        if (targetFlower != null)
        {
            targetFlower.OnFlowerWithered -= Release;
        }
        targetFlower = null;
        currentState = LightBugState.Flying;
    }

    private IEnumerator Dying() {
        yield return new WaitForSeconds(lifeSpan);
        lightTween?.Kill();
        transform.DOKill();
        spriteRenderer.DOKill();
        spriteRenderer.DOFade(0f, 2f);
        DOTween.To(() => bugLight.intensity, x => bugLight.intensity = x, 0f, 2f)
            .SetTarget(gameObject)
            .OnComplete(() => {
            if (currentCoroutine != null) { 
                StopCoroutine(currentCoroutine);
            }
            if (targetFlower != null)
            {
                targetFlower.OnFlowerWithered -= Release;
                targetFlower = null;
            }
            currentState = LightBugState.Dead;
            GameManager.Instance.RecycleBug();
            Destroy(gameObject);
        });
    }

}
