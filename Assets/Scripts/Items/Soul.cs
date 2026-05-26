using UnityEngine;

public class SoulFloat : MonoBehaviour
{
    public float altura = 0.2f;
    public float velocidadFlotar = 2f;
    public float velocidadRotacion = 50f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        float movimientoY = Mathf.Sin(UnityEngine.Time.time * velocidadFlotar) * altura;

        transform.position = new Vector3(
            posicionInicial.x,
            posicionInicial.y + movimientoY,
            posicionInicial.z
        );

        transform.Rotate(Vector3.up * velocidadRotacion * UnityEngine.Time.deltaTime, Space.Self);
    }
}