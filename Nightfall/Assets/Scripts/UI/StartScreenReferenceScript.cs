using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace WTIM.UI
{
    public class StartScreenReferenceScript : MonoBehaviour
    {
        [SerializeField] private UIDocument _document;
        [SerializeField] private StyleSheet _styleSheet;


        //private void OnStartButtonClicked()
        //{
        //    Debug.Log("Start button clicked");
        //}
        //private void OnOptionsButtonClicked()
        //{
        //    Debug.Log("Options button clicked");
        //}
        //private void OnExitButtonClicked()
        //{
        //    Application.Quit();
        //}


        private void Start()
        {
            StartCoroutine(Generate());

        }

        private void OnValidate()
        {
            if (Application.isPlaying && _document != null)
            {
                StartCoroutine(Generate());
            }
        }

        private IEnumerator Generate()
        {
            yield return null;
            var root = _document.rootVisualElement;
            root.Clear();
            if (_styleSheet != null && !root.styleSheets.Contains(_styleSheet))
            {
                root.styleSheets.Add(_styleSheet);
            }

            var container = Create("main-container");
            container.AddToClassList("container");

            var viewBox = Create("view-box", "bordered-box");

            container.Add(viewBox);

            var controlBox = Create("control-box", "bordered-box");
            container.Add(controlBox);


            root.Add(container);
            // Here you can add buttons and other UI elements to the container


        }
        VisualElement Create(params string[] classNames)
        {

            return Create<VisualElement>(classNames);
        }

        T Create<T>(params string[] classNames) where T : VisualElement, new()
        {
            var element = new T();

            foreach (var className in classNames)
                element.AddToClassList(className);


            return element;
        }

        private void OnDestroy()
        {
            var root = _document.rootVisualElement;
            root.Clear();
        }

    }
}
