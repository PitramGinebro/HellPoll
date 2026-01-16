using UnityEngine;

// Estos son los tipos de efectos que definiste
public enum TipoEfecto
{
    Strength,
    Precision,
    Fuego,
    Electrico,
    Angelical,
    Congelar,
    DuplicarBolas
}

public enum TipoCarta
{
    Pasiva_Roja,
    Activa_Azul,
    Satanica_Global
}

[CreateAssetMenu(fileName = "NuevaCarta", menuName = "HellPool/Carta")]
public class CartaData : ScriptableObject
{
    [Header("Configuración Visual")]
    public string nombreCarta;
    [TextArea] public string descripcion;
    public Sprite imagenIcono;

    [Header("Tipo de Carta")]
    public TipoCarta tipoCarta; 
    public TipoEfecto tipoEfecto; 

    [Header("Valores")]
    public float valorMatematico; 
}