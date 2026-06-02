using UnityEngine;

public class Potion : MonoBehaviour
{
    public int healQuantity = 25;

    private bool collected;

    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        collected = true;
        playerHealth.Heal(healQuantity);
        Destroy(gameObject);
    }
}
