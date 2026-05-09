using UnityEngine;



public class GroundClickMarker : MonoBehaviour
    {
    public float lifeTime = 0.3f;
    public float startScale = 0.6f;

    float timer;

        void Start()
        {
            transform.localScale = Vector3.one * startScale;
   
            Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            return;
            Color c = rend.material.color;
            c.a = 0.9f;
            rend.material.color = c;
        }

        }

        void Update()
        {
            timer += UnityEngine.Time.deltaTime;
            float t = timer / lifeTime;


            float scale = Mathf.Lerp(startScale, 0f, Mathf.SmoothStep(0, 1, t));
            transform.localScale = new Vector3(scale, scale, scale);

            if (t >= 1f)
                Destroy(gameObject);
        }
    
}

