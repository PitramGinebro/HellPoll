using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _cueBall = null; 
        
        [Header("Configuración de Vista")]
        [SerializeField] private float _rotationSpeed = 100f;
        [SerializeField] private float _topViewHeight = 12f; // Altura para la vista W (ajústalo en el Inspector)
        [SerializeField] private float _smoothTime = 5f; 
        [SerializeField] private float _shootingDistance = 5f; // Distancia estándar detrás de la bola

        private float _distFromCueBall;
        private Vector3 _initialPos; 
        private Vector3 _initialDir; 
        private bool _isFreeLooking = false;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;

        private void Start()
        {
            if (_cueBall == null)
            {
                CueBallController controller = Object.FindFirstObjectByType<CueBallController>();
                if (controller != null) _cueBall = controller.transform;
            }
            
            _initialPos = transform.position;
            _initialDir = transform.forward;
            
            // Si la distancia inicial es muy corta, forzamos una distancia mínima de 5
            _distFromCueBall = (_cueBall != null) ? Vector3.Distance(_cueBall.position, transform.position) : _shootingDistance;
            if(_distFromCueBall < 2f) _distFromCueBall = _shootingDistance;

            EventManager.Subscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void Update()
        {
            if (_cueBall == null) return;

            // 1. VISTA CENITAL (W o Flecha Arriba)
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                _isFreeLooking = true;
                
                // Calculamos la posición arriba (usando _topViewHeight correctamente)
                _targetPosition = new Vector3(_cueBall.position.x, _topViewHeight, _cueBall.position.z);
                
                // Rotación mirando hacia abajo (90 grados en X)
                _targetRotation = Quaternion.Euler(90, transform.eulerAngles.y, 0);
                
                transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _smoothTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _smoothTime);
            }
            // 2. ROTACIÓN HORIZONTAL
            else if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
            {
                _isFreeLooking = true;
                transform.RotateAround(_cueBall.position, Vector3.up, Input.GetAxis("Horizontal") * _rotationSpeed * Time.deltaTime);
            }
            // 3. RETORNO AUTOMÁTICO
            else if (_isFreeLooking && !Input.anyKey)
            {
                ResetCameraBehindBall();
                
                // Si ya estamos muy cerca de la posición final, dejamos de procesar el retorno
                if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
                {
                    _isFreeLooking = false;
                }
            }
        }

        private void OnCueBallEvent(object sender, IGameEvent gameEvent)
        {
            if (_cueBall == null) return;
            CueBallActionEvent ev = (CueBallActionEvent)gameEvent;

            if (ev.State == CueBallActionEvent.States.Stationary || ev.State == CueBallActionEvent.States.Default)
            {
                // Al detenerse la bola, marcamos que debe reubicarse
                _isFreeLooking = true; 
            }
        }

        private void ResetCameraBehindBall()
        {
            // Calculamos la dirección actual respecto a la bola para mantener el ángulo horizontal
            Vector3 dir = (transform.position - _cueBall.position).normalized;
            dir.y = 0; // Aplanamos el vector para que no se incline
            
            if (dir == Vector3.zero) dir = -_cueBall.forward; // Por seguridad

            // Posición ideal: detrás de la bola a la altura inicial
            _targetPosition = _cueBall.position + dir * _distFromCueBall;
            _targetPosition.y = _initialPos.y; 

            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _smoothTime);
            
            // Mirar a la bola
            Quaternion lookRot = Quaternion.LookRotation(_cueBall.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * _smoothTime);
        }

        // ... (Resto de métodos OnGameInputEvent y OnGameStateEvent igual que antes)
        private void OnGameInputEvent(object sender, IGameEvent gameEvent)
        {
            if (_cueBall == null || _isFreeLooking) return;
            GameInputEvent ev = (GameInputEvent)gameEvent;
            if (ev.State == GameInputEvent.States.HorizontalAxisMovement)
            {
                transform.RotateAround(_cueBall.position, Vector3.up, 60f * ev.axisOffset * Time.deltaTime);
            }
        }

        private void OnGameStateEvent(object sender, IGameEvent gameEvent)
        {
            if (((GameStateEvent)gameEvent).GameState == GameStateEvent.State.Play)
            {
                _isFreeLooking = false;
                transform.position = _initialPos;
                transform.forward = _initialDir;
            }
        }
    }
}