using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

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
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
        changeToFly();
        StartCoroutine(captureEnable());
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

    private void Capturing()
    {

    }

    private void StartDead()
    {

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

            case LightBugState.Captured:
                Capturing();
                break;
        }
    }

    public void changeToFly() { 
        spriteRenderer.DOFade(1f, 1f);
        DOTween.To(() => bugLight.intensity, x => bugLight.intensity = x, 5f, 1.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
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

        float stopDistance = Random.Range(0.5f, 2f);
        Vector3 flowerPosition = flower.transform.position;
        Vector3 direction = (transform.position - flowerPosition).normalized;
        Vector3 targetPos = flowerPosition - direction * stopDistance;
        float travelTime = Vector3.Distance(transform.position, targetPos) / speed;
        transform.DOMove(targetPos, travelTime).SetEase(Ease.InOutSine).OnComplete(() => rb.linearVelocity = Vector2.zero);
    }

    public void Release()
    {
        if (targetFlower != null)
        {
            targetFlower.OnFlowerWithered -= Release;
        }
        targetFlower = null;
        currentState = LightBugState.Flying;
    }

}
