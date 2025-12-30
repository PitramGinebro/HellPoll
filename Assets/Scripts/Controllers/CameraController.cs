using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Transform _cueBall = null; // Referència a la bola blanca.

        // Distància respecte a la bola blanca.
        private float _distFromCueBall;

        private Vector3 _initialPos; // Posició inicial de la càmera.
        private Vector3 _initialDir; // Direcció inicial (forward) de la càmera.

        // Aquesta és la posició sobre la qual rotar quan la bola es colpeja fins que s'atura.
        // El valor per defecte és Vector3.one per evitar comportaments inesperats.
        private Vector3 _posToRot = Vector3.one;

        // S'executa en iniciar l'script.
        private void Start()
        {
            // Desa en memòria la posició i rotació inicials.
            _initialPos = transform.position;
            _initialDir = transform.forward;

            // Assegura que la distància sigui la mateixa amb la que hem començat.
            _distFromCueBall = Vector3.Distance(_cueBall.position, transform.position);

            // Subscripció als esdeveniments del joc.
            EventManager.Subscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnDestroy()
        {
            // Cancel·la la subscripció als esdeveniments en destruir l'objecte per evitar errors de memòria.
            EventManager.Unsubscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Unsubscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Unsubscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        // Gestiona l'acció de la càmera basada en els esdeveniments de la bola.
        private void OnCueBallEvent(object sender, IGameEvent gameEvent)
        {
            CueBallActionEvent cueBallActionEvent = (CueBallActionEvent)gameEvent;

            switch (cueBallActionEvent.State)
            {
                case CueBallActionEvent.States.Stationary: // Quan la bola s'atura.
                case CueBallActionEvent.States.Default:    // Estat per defecte.
                    {
                        float yPos = transform.position.y; // Guarda l'alçada actual.

                        // Mou la càmera a prop de la bola blanca mantenint la distància de seguretat.
                        transform.position = _cueBall.transform.position - transform.forward * _distFromCueBall;
                        // Manté l'alçada original de la càmera.
                        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);

                        // Fa que la càmera miri fixament a la bola blanca.
                        transform.LookAt(_cueBall);

                        // Reinicia el vector de rotació.
                        _posToRot = Vector3.one;
                    }
                    break;
                case CueBallActionEvent.States.Striked: // Quan la bola és colpejada.
                    {
                        // Estableix la posició actual de la bola com el nou centre de rotació.
                        _posToRot = _cueBall.transform.position;
                    }
                    break;
            }
        }

        // Gestiona el moviment de la càmera segons l'entrada de l'usuari (Input).
        private void OnGameInputEvent(object sender, IGameEvent gameEvent)
        {
            GameInputEvent gameInputEvent = (GameInputEvent)gameEvent;

            switch (gameInputEvent.State)
            {
                case GameInputEvent.States.HorizontalAxisMovement:
                    {
                        if (_posToRot == Vector3.one)
                        {
                            // Fa rotar la càmera al voltant de la bola blanca.
                            transform.RotateAround(_cueBall.position, Vector3.up, 20f * gameInputEvent.axisOffset * Time.deltaTime);
                        }
                        else
                        {
                            // Fa rotar la càmera al voltant del punt on estava la bola quan va ser colpejada.
                            transform.RotateAround(_posToRot, Vector3.up, 20f * gameInputEvent.axisOffset * Time.deltaTime);
                        }
                    }
                    break;
                case GameInputEvent.States.VerticalAxisMovement:
                    {
                        // No es fa res aquí de moment (per a possibles futures funcionalitats).
                    }
                    break;
            }
        }

        // Gestiona canvis d'estat globals del joc (ex: començar a jugar).
        private void OnGameStateEvent(object sender, IGameEvent gameEvent)
        {
            GameStateEvent gameStateEvent = (GameStateEvent)gameEvent;
            switch (gameStateEvent.GameState)
            {
                case GameStateEvent.State.Play:
                    {
                        // Col·loca la càmera a la posició i rotació inicials del joc.
                        PlaceInInitialPosAndRot();
                    }
                    break;
            }
        }

        // Mètode per restablir la càmera als valors de l'inici.
        private void PlaceInInitialPosAndRot()
        {
            _posToRot = Vector3.one;

            transform.position = _initialPos;
            transform.forward = _initialDir;
        }
    }
}