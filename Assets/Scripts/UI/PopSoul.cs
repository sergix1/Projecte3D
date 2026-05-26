using UnityEngine;

public class PopSoul : MonoBehaviour
{
    public float escalaPop = 1.2f;
    public float velocidad = 8f;

    private Vector3 escalaInicial;
    private bool hacerPop;

    void Start()
    {
        escalaInicial = transform.localScale;
    }

    void Update()
    {
        if (hacerPop)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                escalaInicial * escalaPop,
               UnityEngine.Time.deltaTime * velocidad
            );

            if (Vector3.Distance(transform.localScale, escalaInicial * escalaPop) < 0.02f)
            {
                hacerPop = false;
            }
        }
        else
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                escalaInicial,
                UnityEngine.Time.deltaTime * velocidad
            );
        }
    }

    public void Pop()
    {
        hacerPop = true;
    }
}