using UnityEngine;

namespace ThreeDPool
{
    // Classe genèrica Singleton que hereta de MonoBehaviour. 
    // S'utilitza com a base per a classes que han de ser úniques.
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        // Variables estàtiques per emmagatzemar el GameObject i el component de la instància.
        private static GameObject _instanceGO;
        private static T _instance;

        // Propietat pública per accedir a la instància des de qualsevol altre script.
        public static T Instance
        {
            get
            {
                // Si la instància encara no existeix, la busquem o la creem.
                if (_instance == null)
                {
                    // Obtenim el nom del tipus de la classe (ex: "GameManager").
                    string typeName = typeof(T).Name;

                    // Intenta trobar un objecte a l'escena que es digui com la classe.
                    _instanceGO = GameObject.Find(typeName);

                    // Si el troba, n'obté el component.
                    if (_instanceGO != null)
                        _instance = _instanceGO.GetComponent<T>();

                    // Si no s'ha trobat cap objecte ni component, el creem de zero.
                    if (_instanceGO == null && _instance == null)
                    {
                        // Crea un nou GameObject buit.
                        _instanceGO = new GameObject();

                        // Li posa el nom del tipus de classe per identificar-lo.
                        _instanceGO.name = typeName;

                        // Afegeix el component del tipus corresponent (T) al nou objecte.
                        _instance = _instanceGO.AddComponent<T>();
                    }

                    // Evita que l'objecte s'elimini en carregar una escena nova (persistència).
                    GameObject.DontDestroyOnLoad(_instanceGO);
                }

                // Retorna la instància única.
                return _instance;
            }
        }

        // Mètode virtual per a inicialització personalitzada.
        protected virtual void Init()
        {

        }

        // Mètodes de cicle de vida de Unity marcats com a virtuals per poder fer 'override'.
        protected virtual void Awake()
        { }

        protected virtual void Start()
        { }

        protected virtual void Update()
        { }

        // Quan es destrueix l'objecte, s'alliberen les referències estàtiques.
        protected virtual void OnDestroy()
        {
            _instanceGO = null;
            _instance = null;
        }
    }
}