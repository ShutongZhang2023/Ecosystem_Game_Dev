using UnityEngine;

public class BounceObject : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        rb.linearVelocity = randomDir * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 normal = collision.contacts[0].normal;
        rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, normal);
    }
}
