using UnityEngine;
using TMPro; 
using ThreeDPool.Managers;

namespace ThreeDPool.UIControllers
{
    public class ScoreUI : MonoBehaviour
    {
        private TextMeshProUGUI _textMesh;

        private void Awake()
        {
            _textMesh = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (GameManager.Instance != null && _textMesh != null)
            {
                // El truco está en el ToString("D4")
                // "D4" significa: Formato Decimal con 4 dígitos fijos.
                // Si quieres 5 ceros, pondrías "D5".
                _textMesh.text = GameManager.Instance.CurrentScore.ToString("D4");
            }
        }
    }
}