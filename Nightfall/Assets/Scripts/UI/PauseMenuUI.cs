using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private AudioClip clickSound;
    [SerializeField]private AudioSource audioSource;

    private VisualElement root;
    private VisualElement pauseMenu;
    private VisualElement settingsMenu;

    private Button resumeButton;
    private Button settingsButton;
    private Button restartButton;
    private Button quitButton;
    private Button backButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (uIDocument == null)
            uIDocument = GetComponent<UIDocument>();

        root = uIDocument.rootVisualElement;

        pauseMenu = root.Q<VisualElement>("PauseMenu");
        settingsMenu = root.Q<VisualElement>("SettingsMenu");

        resumeButton = root.Q<Button>("resumeButton");
        settingsButton = root.Q<Button>("settingsButton");
        restartButton = root.Q<Button>("restartButton");
        quitButton = root.Q<Button>("quitButton");
        backButton = root.Q<Button>("backButton");


        Hide();

        if (resumeButton != null)
            resumeButton.clicked += () => { OnClickSound(); OnResumeButtonClicked(); };

        if (settingsButton != null)
            settingsButton.clicked += () => { OnClickSound(); OnSettingsButtonClicked(); };

        if (restartButton != null)
            restartButton.clicked += () => { OnClickSound(); OnRestartButtonClicked(); };

        if (quitButton != null)
            quitButton.clicked += () => { OnClickSound(); OnQuitButtonClicked(); };

        if (backButton != null)
            backButton.clicked += () => { OnClickSound(); OnBackButtonClicked(); };

        
    }
    private void OnClickSound()
    {
        if(audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
    private void OnResumeButtonClicked()
    {
        GameManager.instance.StateUnpause();
        Hide();
    }
    private void OnSettingsButtonClicked()
    {
        Hide();
        settingsMenu.style.display = DisplayStyle.Flex;
    }
    private void OnRestartButtonClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Hide();
    }
    private void OnBackButtonClicked()
    {
        settingsMenu.style.display = DisplayStyle.None;
        Show();
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
