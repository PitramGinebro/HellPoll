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

        private void Awake()
        {
            // Auto-inyección del script de estado para asegurar que el sistema de cartas funcione
            if (GetComponent<EstadoBola>() == null)
            {
                gameObject.AddComponent<EstadoBola>();
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

        // --- LÓGICA DE DISPARO CON MEJORAS ROGUELIKE ---
        private void OnStriked(float forceGathered)
        {
            if (_ballType == CueBallType.White)
            {
                GameManager.Instance.NumOfBallsStriked++;
                Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();

                float multiplicadorFuerza = 1f;
                Vector3 direccionDisparo = Camera.main.transform.forward;

                EstadoBola estado = GetComponent<EstadoBola>();
                if (estado != null)
                {
                    // Mejora de FUERZA
                    if (estado.tieneFuerzaExtra) multiplicadorFuerza = 2.5f;

                    // Mejora de PRECISIÓN (Aimbot)
                    if (estado.tienePrecision)
                    {
                        GameObject bolaObjetivo = EncontrarBolaCercanaALaMira();
                        if (bolaObjetivo != null)
                        {
                            Vector3 haciaBola = (bolaObjetivo.transform.position - transform.position).normalized;
                            // Mezclamos la dirección de la cámara con la de la bola para un "imán" suave
                            direccionDisparo = Vector3.Lerp(direccionDisparo, haciaBola, 0.6f);
                            Debug.Log("Asistencia de precisión aplicada sobre: " + bolaObjetivo.name);
                        }
                    }
                }

                rigidBody.AddForce(direccionDisparo * _force * forceGathered * multiplicadorFuerza, ForceMode.Force);
            }
        }

        // Busca qué bola tenemos delante para ayudar al disparo
        private GameObject EncontrarBolaCercanaALaMira()
        {
            float anguloCorte = 20f; // Solo ayuda si apuntamos "cerca"
            GameObject mejorBola = null;
            
            CueBallController[] todasLasBolas = Object.FindObjectsByType<CueBallController>(FindObjectsSortMode.None);
            
            foreach (var bola in todasLasBolas)
            {
                if (bola.BallType == CueBallType.White) continue;

                Vector3 direccionABola = (bola.transform.position - transform.position).normalized;
                float angulo = Vector3.Angle(Camera.main.transform.forward, direccionABola);

                if (angulo < anguloCorte)
                {
                    anguloCorte = angulo;
                    mejorBola = bola.gameObject;
                }
            }
            return mejorBola;
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