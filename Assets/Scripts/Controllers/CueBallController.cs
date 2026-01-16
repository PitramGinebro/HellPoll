using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool.Controllers
{
    public class CueBallController : MonoBehaviour
    {
        public enum CueBallType
        {
            White = 0, Yellow, Blue, Red, Purple, Orange, Green, Burgandy, Black,
            Striped_Yellow, Striped_Blue, Striped_Red, Striped_Purple,
            Striped_Orange, Striped_Green, Striped_Burgandy,
        }

        [SerializeField] float _force = 30f;
        [SerializeField] CueBallType _ballType = CueBallType.White;

        private CueBallActionEvent.States _currState;
        private Vector3 _initialPos;
        public bool IsPocketedInPrevTurn;
        public CueBallType BallType { get { return _ballType; } }

        // --- SOLUCIÓN PARA EL SPAWNEO ---
        private void Awake()
        {
            // Si al aparecer en el juego la bola no tiene el script de estado, se lo ponemos nosotros
            if (GetComponent<EstadoBola>() == null)
            {
                gameObject.AddComponent<EstadoBola>();
                Debug.Log("Equipo: He inyectado el script EstadoBola automáticamente en " + gameObject.name);
            }
        }

        private void Start()
        {
            _initialPos = transform.position;
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnDestroy()
        {
            EventManager.Unsubscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Unsubscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnCueBallEvent(object sender, IGameEvent gameEvent)
        {
            CueBallActionEvent actionEvent = (CueBallActionEvent)gameEvent;
            if (actionEvent.State == CueBallActionEvent.States.Stationary)
            {
                _currState = CueBallActionEvent.States.Default;
            }
        }

        private void OnGameStateEvent(object sender, IGameEvent gameEvent)
        {
            GameStateEvent gameStateEvent = (GameStateEvent)gameEvent;
            if (gameStateEvent.GameState == GameStateEvent.State.Play)
            {
                PlaceBallInInitialPos();
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.transform.parent == null) return;
            
            CueController cueController = collider.gameObject.transform.parent.GetComponent<CueController>();
            if (cueController != null && _ballType == CueBallType.White)
            {
                EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Striked });
                _currState = CueBallActionEvent.States.Striked;
                OnStriked(cueController.ForceGatheredToHit);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
            {
                GameManager.Instance.AddToBallHitOutList(this);
                PlaceBallInInitialPos();
                if (_ballType == CueBallType.White) NotifyStationary();
            }
        }

        private void FixedUpdate()
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody == null) return;

            if (transform.position.y < -5.0f && _ballType == CueBallType.White)
            {
                PlaceBallInInitialPos();
                NotifyStationary();
            }

            if ((_currState == CueBallActionEvent.States.Placing) && rigidbody.IsSleeping())
                _currState = CueBallActionEvent.States.Default;
            else if ((_currState == CueBallActionEvent.States.Default) && (!rigidbody.IsSleeping()))
            {
                if (GameManager.Instance.CurrGameState == GameManager.GameState.Play)
                    GameManager.Instance.NumOfBallsStriked++;
                _currState = CueBallActionEvent.States.Striked;
            }
            else if ((_currState == CueBallActionEvent.States.Striked || _currState == CueBallActionEvent.States.InMotion) && !rigidbody.IsSleeping())
                _currState = CueBallActionEvent.States.InMotion;
            else if ((_currState == CueBallActionEvent.States.InMotion) && rigidbody.IsSleeping())
            {
                GameManager.Instance.ReadyForNextRound();
                _currState = CueBallActionEvent.States.Stationary;
            }
        }

        private void OnStriked(float forceGathered)
        {
            if (_ballType == CueBallType.White)
            {
                GameManager.Instance.NumOfBallsStriked++;
                Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();

                // ROGUELIKE: Comprobar fuerza extra
                float multiplicadorFuerza = 1f;
                EstadoBola estado = GetComponent<EstadoBola>();
                if (estado != null && estado.tieneFuerzaExtra)
                {
                    multiplicadorFuerza = 2.5f; 
                }

                rigidBody.AddForce(Camera.main.transform.forward * _force * forceGathered * multiplicadorFuerza, ForceMode.Force);
            }
        }

        public void BallPocketed()
        {
            GameManager.Instance.AddToBallPocketedList(this);
            
            if (_ballType == CueBallType.White)
            {
                PlaceBallInInitialPos();
                NotifyStationary();
                GameManager.Instance.ReadyForNextRound();
            }
            else
            {
                // ROGUELIKE: Mandamos el estado de la bola al sumar puntos
                EstadoBola estado = GetComponent<EstadoBola>();
                GameManager.Instance.AddScore(1, estado);

                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.isKinematic = true; 
                }
                transform.position = new Vector3(0, -50f, 0); 
            }
        }

        public void PlaceBallInInitialPos()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; 
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            transform.position = new Vector3(_initialPos.x, _initialPos.y + 0.2f, _initialPos.z);
            IsPocketedInPrevTurn = false;
            _currState = CueBallActionEvent.States.Placing;
            GameManager.Instance.NumOfBallsStriked = 0;

            // ROGUELIKE: Limpiar cartas al reaparecer
            EstadoBola estado = GetComponent<EstadoBola>();
            if (estado != null) estado.ResetearEstado();
        }

        private void NotifyStationary()
        {
            _currState = CueBallActionEvent.States.Stationary;
            EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Stationary });
        }
    }
}