using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool.Controllers
{
    public class CueBallController : MonoBehaviour
    {
        // Definició de tots els tipus de boles possibles en el joc de billar.
        public enum CueBallType
        {
            White = 0, Yellow, Blue, Red, Purple, Orange, Green, Burgandy, Black,
            Striped_Yellow, Striped_Blue, Striped_Red, Striped_Purple,
            Striped_Orange, Striped_Green, Striped_Burgandy,
        }

        [SerializeField] float _force = 30f; // Força base de colpeig.
        [SerializeField] CueBallType _ballType = CueBallType.White; // Tipus d'aquesta bola concreta.

        private CueBallActionEvent.States _currState; // Estat actual de la bola (en moviment, quieta, etc.).
        private Vector3 _initialPos; // Posició original per a reinicis.

        // Diferencia si la bola ha entrat al forat en aquest torn o en un d'anterior (per a la puntuació).
        public bool IsPocketedInPrevTurn;

        public CueBallType BallType { get { return _ballType; } }

        private void Start()
        {
            // Guarda la posició inicial per si la bola surt de la taula.
            _initialPos = transform.position;

            // Subscripció als esdeveniments de la bola i de l'estat del joc.
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
            switch (actionEvent.State)
            {
                case CueBallActionEvent.States.Stationary:
                    // Si la bola s'atura, l'estat torna a ser el per defecte (llesta per al següent tir).
                    _currState = CueBallActionEvent.States.Default;
                    break;
            }
        }

        private void OnGameStateEvent(object sender, IGameEvent gameEvent)
        {
            GameStateEvent gameStateEvent = (GameStateEvent)gameEvent;
            if (gameStateEvent.GameState == GameStateEvent.State.Play)
            {
                // Reinicia la bola a la seva posició original en començar la partida.
                PlaceBallInInitialPos();
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            // Detecta si el que ha tocat la bola és el taco (Cue).
            CueController cueController = collider.gameObject.transform.parent.GetComponent<CueController>();

            if (cueController != null)
            {
                // Només la bola blanca pot ser colpejada directament pel taco.
                if (_ballType == CueBallType.White)
                {
                    // Notifica que la bola blanca ha estat colpejada.
                    EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Striked });

                    _currState = CueBallActionEvent.States.Striked;

                    // Recupera la força acumulada pel jugador amb el taco.
                    float forceGatheredToHit = cueController.ForceGatheredToHit;

                    // Aplica el moviment físic a la bola.
                    OnStriked(forceGatheredToHit);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Si la bola toca el terra (fora de la taula), s'afegeix a la llista de boles fora de joc.
            if (collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
            {
                GameManager.Instance.AddToBallHitOutList(this);
            }
        }

        private void FixedUpdate()
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

            // Maquina d'estats per gestionar el cicle de vida del moviment de la bola.
            if ((_currState == CueBallActionEvent.States.Placing) && rigidbody.IsSleeping())
            {
                _currState = CueBallActionEvent.States.Default;
            }
            else if ((_currState == CueBallActionEvent.States.Default) && (!rigidbody.IsSleeping()))
            {
                // Si la bola es mou estant en mode joc, incrementem el comptador de boles colpejades.
                if (GameManager.Instance.CurrGameState == GameManager.GameState.Play)
                    GameManager.Instance.NumOfBallsStriked++;

                _currState = CueBallActionEvent.States.Striked;
            }
            else if ((_currState == CueBallActionEvent.States.Striked || _currState == CueBallActionEvent.States.InMotion) && !rigidbody.IsSleeping())
            {
                _currState = CueBallActionEvent.States.InMotion;
            }
            else if ((_currState == CueBallActionEvent.States.InMotion) && rigidbody.IsSleeping())
            {
                // Quan la bola finalment s'atura després d'un tir, avisem que la ronda ha acabat.
                GameManager.Instance.ReadyForNextRound();
                _currState = CueBallActionEvent.States.Stationary;
            }
        }

        private void OnStriked(float forceGathered)
        {
            // Aplica una força física en la direcció de la càmera (on apunta el jugador).
            if (_ballType == CueBallType.White)
            {
                GameManager.Instance.NumOfBallsStriked++;
                Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
                rigidBody.AddForce(Camera.main.transform.forward * _force * forceGathered, ForceMode.Force);
            }
        }

        // Cridat pel "PocketsCollider" quan la bola entra en un forat.
        public void BallPocketed()
        {
            GameManager.Instance.AddToBallPocketedList(this);
        }

        // Funció per reposicionar la bola durant el mode pràctica.
        public void PlaceBallInPosWhilePractise()
        {
            PlaceBallInInitialPos();
            EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Stationary });
        }

        // Reinicia la bola a la seva posició inicial amb un petit desplaçament vertical per evitar col·lisions.
        public void PlaceBallInInitialPos()
        {
            transform.position = new Vector3(_initialPos.x, _initialPos.y + 0.2f, _initialPos.z);
            IsPocketedInPrevTurn = false;
            _currState = CueBallActionEvent.States.Placing;
            GameManager.Instance.NumOfBallsStriked = 0;
        }
    }
}