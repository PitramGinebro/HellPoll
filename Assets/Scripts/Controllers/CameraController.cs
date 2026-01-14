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
        [SerializeField] private float _topViewHeight = 10f; // Qué tan alto sube la cámara
        [SerializeField] private float _smoothTime = 5f; // Suavidad de transición

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
            _distFromCueBall = (_cueBall != null) ? Vector3.Distance(_cueBall.position, transform.position) : 7f;

            EventManager.Subscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void Update()
        {
            if (_cueBall == null) return;

            // 1. VISTA CENITAL (Mantener Flecha Arriba o W)
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                _isFreeLooking = true;
                // Calculamos la posición justo encima de la mesa
                _targetPosition = new Vector3(_cueBall.position.x, _topViewHeight, _cueBall.position.z);
                _targetRotation = Quaternion.Euler(90, transform.eulerAngles.y, 0);
                
                // Transición suave
                transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _smoothTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _smoothTime);
            }
            // 2. ROTACIÓN HORIZONTAL (Flechas Izquierda/Derecha)
            else if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
            {
                _isFreeLooking = true;
                transform.RotateAround(_cueBall.position, Vector3.up, Input.GetAxis("Horizontal") * _rotationSpeed * Time.deltaTime);
            }
            else if (_isFreeLooking && !Input.anyKey)
            {
                // Si dejamos de pulsar, volvemos a la vista de tiro gradualmente
                _isFreeLooking = false;
                ResetCameraBehindBall();
            }
        }

        private void OnCueBallEvent(object sender, IGameEvent gameEvent)
        {
            if (_cueBall == null) return;
            CueBallActionEvent ev = (CueBallActionEvent)gameEvent;

            if (ev.State == CueBallActionEvent.States.Stationary || ev.State == CueBallActionEvent.States.Default)
            {
                _isFreeLooking = false;
                ResetCameraBehindBall();
            }
        }

        private void ResetCameraBehindBall()
        {
            Vector3 dir = (transform.position - _cueBall.position).normalized;
            dir.y = 0;
            Vector3 finalPos = _cueBall.position + dir * _distFromCueBall;
            finalPos.y = _initialPos.y; // Volvemos a la altura original de tiro

            transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * _smoothTime);
            
            // Mirar a la bola
            Quaternion lookRot = Quaternion.LookRotation(_cueBall.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * _smoothTime);
        }

        private void OnGameInputEvent(object sender, IGameEvent gameEvent)
        {
            // Bloqueamos el input del ratón/taco si estamos usando las flechas
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