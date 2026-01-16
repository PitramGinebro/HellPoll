using UnityEngine;

public class EstadoBola : MonoBehaviour
{
    [Header("Estado Actual (Roguelike)")]
    public float multiplicadorPuntos = 1f; 
    public bool esAngelical = false;
    public bool tieneFuerzaExtra = false;
    public bool tienePrecision = false;

    // Esta función se activa cuando usas una carta en esta bola
    public void AplicarCarta(CartaData carta)
    {
        switch (carta.tipoEfecto)
        {
            case TipoEfecto.Fuego:
                multiplicadorPuntos += carta.valorMatematico;
                CambiarColorVisual(Color.red);
                break;
            case TipoEfecto.Electrico:
                multiplicadorPuntos += carta.valorMatematico;
                CambiarColorVisual(Color.cyan);
                break;
            case TipoEfecto.Angelical:
                esAngelical = true;
                CambiarColorVisual(Color.yellow);
                break;
            case TipoEfecto.Strength:
                tieneFuerzaExtra = true;
                CambiarColorVisual(Color.black);
                break;
        }
    }

    private void CambiarColorVisual(Color color)
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = color;
    }

    public void ResetearEstado()
    {
        multiplicadorPuntos = 1f;
        esAngelical = false;
        tieneFuerzaExtra = false;
        tienePrecision = false;
    }
}