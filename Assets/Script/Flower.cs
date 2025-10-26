using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System;

public enum PlantState
{
    Grow,
    Bloom,
    Wither,
    End
}
public class Flower : MonoBehaviour
{
    public event Action OnFlowerWithered;

    public PlantState currentState;
    public float growTime;
    public float bloomDuration;
    public float witherTime = 5f;
    public Vector3 targetScale;

    private SpriteRenderer spriteRenderer;
    private Light2D flowerLight;
    private Rigidbody2D rb;
    public Collider2D col;
    public Transform flowerTransform;

    [SerializeField] private float minLight = 0f;
    [SerializeField] private float maxLight = 5f;

    private Coroutine currentCoroutine;
    public int positionIndex = -1;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        flowerLight = GetComponentInChildren<Light2D>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponentInChildren<Collider2D>();
        
    }

    public void flowerGenerate(Vector3 spawnPos)
    {
        col.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        transform.position = spawnPos;
        currentState = PlantState.Grow;
        flowerTransform.localScale = Vector3.zero;
        flowerLight.intensity = minLight;
        float scale = UnityEngine.Random.Range(0.05f, 0.15f);
        targetScale = new Vector3(scale, scale, scale);
        growTime = UnityEngine.Random.Range(10f, 20f);
        bloomDuration = UnityEngine.Random.Range(20f, 40f);
        ChangeState();
    }

    private void ChangeState()
    {
        switch (currentState) { 

            case PlantState.Grow:
                Grow();
                break;

            case PlantState.Bloom:
                Bloom();
                break;

            case PlantState.Wither:
                Wither();
                break;
        }
    }

    private void Grow() {
        DOTween.To(() => flowerLight.intensity, x => flowerLight.intensity = x, maxLight, growTime);
        flowerTransform.DOScale(targetScale, growTime).SetEase(Ease.InQuad).OnComplete(() =>
        {
            currentState = PlantState.Bloom;
            ChangeState();
        });

        flowerTransform.DORotate(new Vector3(0, 0, 360f), growTime, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }

    private void Bloom()
    {
        col.enabled = true;
        flowerTransform.DOKill();

        float breathingScale = targetScale.x * 0.1f;
        flowerTransform.DOScale(targetScale + Vector3.one * breathingScale, 1.5f)
                 .SetLoops(-1, LoopType.Yoyo)
                 .SetEase(Ease.InOutSine);

        currentCoroutine =  StartCoroutine(BloomTimer());
    }

    private IEnumerator BloomTimer() {
        yield return new WaitForSeconds(bloomDuration);
        currentState = PlantState.Wither;
        ChangeState();
    }

    private void Wither() 
    {
        flowerTransform.DOKill();
        col.enabled = false;

        OnFlowerWithered?.Invoke();

        DOTween.To(() => flowerLight.intensity, x => flowerLight.intensity = x, minLight, witherTime).SetEase(Ease.InQuad);
        Vector3 witherScale = targetScale * 0.5f;
        flowerTransform.DOScale(witherScale, witherTime).SetEase(Ease.InQuad).OnComplete(() =>
        {
            currentState = PlantState.End;
            rb.bodyType = RigidbodyType2D.Dynamic;
            spriteRenderer.DOFade(0f, 5f).OnComplete(() =>
            {
                GameManager.Instance.RecycleFlower(this);
            });
        });
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("LightBug"))
        {
            LightBug bug = collision.GetComponent<LightBug>();
            if (bug.targetFlower == null) {
                bug.SetTargetFlower(this);
            }
        }
    }



}
