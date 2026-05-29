using UnityEngine;

public class TestCooldownUI : MonoBehaviour
{
    [SerializeField] private AbilityCooldownUI rollCooldown;
    [SerializeField] private AbilityCooldownUI tripleCooldown;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rollCooldown.StartCooldown(3f);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            tripleCooldown.StartCooldown(5f);
        }
    }
}