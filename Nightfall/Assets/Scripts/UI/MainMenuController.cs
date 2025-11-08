using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement mainMenu;

    private Button playButton;
    private Button settingsButton;
    private Button creditsButton;
    private Button quitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;

        mainMenu = root.Q<VisualElement>("MainMenu");
        playButton = root.Q<Button>("PlayButton");
        settingsButton = root.Q<Button>("SettingsButton");
        creditsButton = root.Q<Button>("CreditsButton");
        quitButton = root.Q<Button>("QuitButton");

        Show();

        if(playButton  != null)
            playButton.clicked += OnPlayButtonClicked;

        if(settingsButton != null)
            settingsButton.clicked += OnSettingsButtonClicked;

        if(creditsButton != null)
            creditsButton.clicked += OnCreditsButtonClicked;

        if (quitButton != null)
            quitButton.clicked += OnQuitButtonClicked;

    }
    private void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("ShowcaseLevel");
        Hide();
    }
    private void OnSettingsButtonClicked()
    {

    }
    private void OnCreditsButtonClicked()
    {

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
        if (mainMenu != null)
            mainMenu.style.display = DisplayStyle.Flex;
    }
    public void Hide()
    {
        if (mainMenu != null)
            mainMenu.style.display = DisplayStyle.None;
    }
}
