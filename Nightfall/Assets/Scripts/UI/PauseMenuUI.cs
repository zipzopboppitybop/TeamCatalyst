using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private UIDocument uIDocument;

    private VisualElement root;
    private VisualElement pauseMenu;

    private Button resumeButton;
    private Button settingsButton;
    private Button restartButton;
    private Button quitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (uIDocument == null) 
            uIDocument = GetComponent<UIDocument>();

        root = uIDocument.rootVisualElement;


        pauseMenu = root.Q<VisualElement>("PauseMenu");
        
        resumeButton = root.Q<Button>("resumeButton");
        settingsButton = root.Q<Button>("settingsButton");
        restartButton = root.Q<Button>("restartButton");
        quitButton = root.Q<Button>("quitButton");
        Hide();

        if (resumeButton != null)
            resumeButton.clicked += OnResumeButtonClicked;
        if(quitButton != null)
            quitButton.clicked += OnQuitButtonClicked;

        
    }
    private void OnResumeButtonClicked()
    {
        GameManager.instance.StateUnpause();
        Hide();
    }
    private void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void Show()
    {
        if (pauseMenu != null)
            pauseMenu.style.display = DisplayStyle.Flex;
    }
    public void Hide()
    {
        if (pauseMenu != null)
            pauseMenu.style.display = DisplayStyle.None;
    }
}
