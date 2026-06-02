using TMPro;
using UnityEngine;

public class LifeBar : MonoBehaviour
{
    public Transform head;
    public TextMeshProUGUI text;

    private Transform cam;
    private EnemyBase enemy;
    private int lastLife = -1;
    private int lastMaxLife = -1;

    private void Awake()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
            cam = mainCamera.transform;

        enemy = GetComponentInParent<EnemyBase>();
        transform.localScale = Vector3.one * 0.02f;
        UpdateLifeText();
    }

    private void Update()
    {
        if (head == null)
            return;

        Vector3 headPosition = new Vector3(head.position.x + 1.5f, head.position.y, head.position.z);
        transform.position = headPosition + Vector3.up * 0.25f;

        if (cam != null)
            transform.rotation = Quaternion.LookRotation(cam.forward);

        UpdateLifeText();
    }

    public void SetLife()
    {
        UpdateLifeText();
    }

    private void UpdateLifeText()
    {
        if (enemy == null || text == null)
            return;

        int currentLife = enemy.GetCurrentLife();
        int maxLife = enemy.MaxLife();

        if (currentLife == lastLife && maxLife == lastMaxLife)
            return;

        lastLife = currentLife;
        lastMaxLife = maxLife;
        text.text = currentLife + " / " + maxLife;
    }
}
