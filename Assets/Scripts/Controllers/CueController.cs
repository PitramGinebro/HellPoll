using System.Collections;
using ThreeDPool.Managers;
using ThreeDPool.EventHandlers;
using UnityEngine;

namespace ThreeDPool.Controllers
{
    class CueController : MonoBehaviour
    {
        [SerializeField]
        private Transform _cueBall = null; // Referència a la bola blanca.

        private float _defaultDistFromCueBall; // Distància base entre el taco i la bola.
        private float _maxClampDist = 9;       // Màxima distància que es pot estirar el taco.
        private float _forceGathered = 0.0f;   // Força acumulada segons la posició.
        private float _forceThreshold = 0.5f;  // Força mínima per considerar un tir vàlid.
        private float _speed = 10.0f;          // Velocitat del taco en colpejar.
        private bool _cueReleasedToStrike = false; // Flag per saber si el taco ha estat llançat.

        private Vector3 _initialPos; // Posició inicial del taco.
        private Vector3 _initialDir; // Direcció inicial del taco.
        private Vector3 _posToRot = Vector3.one; // Punt de pivot per al gir.

        public float ForceGatheredToHit { get { return _forceGathered; } }

        private void Start()
        {
            // Guarda les dades inicials per a futurs reinicis.
            _initialPos = transform.position;
            _initialDir = transform.forward;
            _defaultDistFromCueBall = Vector3.Distance(_cueBall.position, transform.position);

            // Subscripció als esdeveniments d'entrada, accions de la bola i estat del joc.
            EventManager.Subscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnDestroy()
        {
            // Cancel·lació de subscripcions al destruir l'objecte.
            EventManager.Unsubscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Unsubscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Unsubscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnGameInputEvent(object sender, IGameEvent gameEvent)
        {
            GameInputEvent gameInputEvent = (GameInputEvent)gameEvent;

            switch (gameInputEvent.State)
            {
                // CONTROL DE GIR AMB TELES A / D (Eix Horitzontal)
                case GameInputEvent.States.HorizontalAxisMovement:
                    {
                        // Llegeix l'entrada de l'eix horitzontal del teclat (A/D).
                        float rotationInput = Input.GetAxis("Horizontal");
                        float sensitivity = 100f;

                        // Fa rotar el taco al voltant de la bola blanca o del pivot fixat.
                        if (_posToRot == Vector3.one)
                            transform.RotateAround(_cueBall.position, Vector3.up, sensitivity * rotationInput * Time.deltaTime);
                        else
                            transform.RotateAround(_posToRot, Vector3.up, sensitivity * rotationInput * Time.deltaTime);
                    }
                    break;

                // MOVIMENT VERTICAL PER FER FORÇA
                case GameInputEvent.States.VerticalAxisMovement:
                    {
                        if (_posToRot != Vector3.one)
                            return;

                        // Calcula la nova posició del taco en estirar-lo cap enrere.
                        var newPosition = transform.position + transform.forward * gameInputEvent.axisOffset;
                        _forceGathered = Vector3.Distance(_cueBall.position, newPosition);

                        // Limita el moviment perquè el jugador no allunyi el taco infinitament.
                        if ((_forceGathered < _defaultDistFromCueBall + _maxClampDist) &&
                            _forceGathered > _defaultDistFromCueBall)
                        {
                            transform.position = newPosition;
                            EventManager.Notify(typeof(CueActionEvent).ToString(), this, new CueActionEvent() { ForceGathered = _forceGathered });
                        }
                    }
                    break;

                // ACCIÓ DE DEIXAR ANAR (Disparar)
                case GameInputEvent.States.Release:
                    {
                        if (_posToRot != Vector3.one)
                            return;

                        // Si s'ha acumulat prou força, activa el moviment de colpeig.
                        if (_forceGathered > _defaultDistFromCueBall + _forceThreshold)
                            _cueReleasedToStrike = true;
                    }
                    break;
            }
        }

        private void OnCueBallEvent(object sender, IGameEvent gameEvent)
        {
            CueBallActionEvent cueBallActionEvent = (CueBallActionEvent)gameEvent;

            switch (cueBallActionEvent.State)
            {
                case CueBallActionEvent.States.Stationary:
                case CueBallActionEvent.States.Default:
                    {
                        // Torna el taco a la posició de repòs darrere la bola quan aquesta s'atura.
                        _forceGathered = 0f;
                        transform.position = _cueBall.transform.position - transform.forward * _defaultDistFromCueBall;
                        transform.LookAt(_cueBall);
                        _posToRot = Vector3.one;
                    }
                    break;

                case CueBallActionEvent.States.Striked:
                    {
                        // Atura el moviment de colpeig i allunya el taco per efecte visual.
                        _cueReleasedToStrike = false;
                        if (GameManager.Instance.CurrGameState == GameManager.GameState.Play)
                        {
                            StartCoroutine(MoveCueAfterStrike(transform.position, _cueBall.transform.position - transform.forward * _defaultDistFromCueBall * 1.5f, 1.0f));
                        }
                        transform.LookAt(_cueBall);
                        _posToRot = _cueBall.transform.position;
                    }
                    break;
            }
        }

        private void OnGameStateEvent(object sender, IGameEvent gameEvent)
        {
            GameStateEvent gameStateEvent = (GameStateEvent)gameEvent;
            if (gameStateEvent.GameState == GameStateEvent.State.Play)
            {
                // Reseteja el taco quan comença la partida.
                PlaceInInitialPosAndRot();
            }
        }

        // Corrutina per moure el taco suaument enrere després del xoc.
        IEnumerator MoveCueAfterStrike(Vector3 source, Vector3 target, float overTime)
        {
            float startTime = Time.time;
            while (Time.time < startTime + overTime)
            {
                transform.position = Vector3.Lerp(source, target, (Time.time - startTime) / overTime);
                yield return null;
            }
            transform.position = target;
        }

        private void FixedUpdate()
        {
            // Moviment físic del taco cap a la bola quan s'ha disparat.
            if (_cueReleasedToStrike)
            {
                float step = _speed * Time.deltaTime * (_forceGathered / _speed);
                transform.position = Vector3.MoveTowards(transform.position, _cueBall.transform.position, step);
            }
        }

        // Mètode per netejar variables i tornar a la posició d'inici.
        private void PlaceInInitialPosAndRot()
        {
            _forceGathered = 0f;
            _cueReleasedToStrike = false;
            _posToRot = Vector3.one;
            transform.position = _initialPos;
            transform.forward = _initialDir;
        }
    }
}