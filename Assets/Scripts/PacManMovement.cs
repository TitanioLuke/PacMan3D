using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PacManMovement : MonoBehaviour
{
    public float speed = 5f;
    private float originalSpeed;
    private float boostTimer = 0f;
    private Rigidbody rb;

    private float invulnerabilityTimer = 0f;
    public float invulnerabilityDuration = 1.5f;

    private bool isInvulnerable = false;
    private bool hasCollidedThisFrame = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalSpeed = speed;
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.linearVelocity = movement * speed;

        if (boostTimer > 0f)
        {
            boostTimer -= Time.fixedDeltaTime;
            if (boostTimer <= 0f)
                speed = originalSpeed;
        }

        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.fixedDeltaTime;
            if (invulnerabilityTimer <= 0f)
                isInvulnerable = false;
        }

        // Reset da flag de colisão no final de cada frame física
        hasCollidedThisFrame = false;
    }

    public void ActivateSpeedBoost(float duration, float newSpeed)
    {
        speed = newSpeed;
        boostTimer = duration;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollidedThisFrame) return;

        if (!isInvulnerable && other.CompareTag("Enemy"))
        {
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
            hasCollidedThisFrame = true;

            GameManager.Instance.LoseLife();
            Debug.Log("Pac-Man perdeu uma vida ao tocar num fantasma!");
        }
    }
}