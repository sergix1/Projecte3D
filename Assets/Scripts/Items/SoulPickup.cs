using UnityEngine;

public class SoulPickup : MonoBehaviour
{
    public int soulAmount = 1;
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private Vector3 pickupEffectOffset = new Vector3(0f, 0.3f, 0f);
    [SerializeField] private float pickupEffectDestroyTime = 0.5f;
    private bool collected;
    public void SetSoulAmount(int amount)
    {
        soulAmount = Mathf.Max(1, amount);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (other.CompareTag("Player"))
        {
            collected = true;
            if (GameManager.Instance == null)
            {
 
                return;
            }
            if (pickupEffect != null)
            {
                GameObject effect = Instantiate(pickupEffect, transform.position + pickupEffectOffset, Quaternion.identity);
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    ps.Play();
                }

                Destroy(effect, pickupEffectDestroyTime);
            }


            GameManager.Instance.AddSouls(soulAmount);
            Destroy(gameObject);
        }
    }
}
