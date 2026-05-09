using TMPro;
using UnityEngine;

public class LifeBar : MonoBehaviour
{
    public Transform head;   
    public TextMeshProUGUI text;

    Transform cam;

    void Awake()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        if (!head) return;
        var head1 = new Vector3(head.position.x + 1.5f, head.position.y , head.position.z);

  
        transform.position = head1 + Vector3.up * 0.25f;       
        transform.rotation = Quaternion.LookRotation(cam.forward);
        transform.localScale = Vector3.one * 0.02f;
        SetLife();
    }
    public void SetLife()
    {
        var enemyscr = this.gameObject.GetComponentInParent<EnemyBase>();
        text.text = enemyscr.GetCurrentLife() +  " / " + enemyscr.MaxLife() + "";
    }
}
