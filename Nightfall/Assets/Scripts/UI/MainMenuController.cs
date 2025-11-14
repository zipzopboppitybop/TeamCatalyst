using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip clickSound;
    [SerializeField] AudioClip PageChange;
    [SerializeField] Loading load;

    private VisualElement root;
    private VisualElement mainMenu;
    private VisualElement settingsMenu;
    private VisualElement creditsMenu;

    private Button playButton;
    private Button settingsButton;
    private Button creditsButton;
    private Button quitButton;
    private Button backButton;
    private Button creditBackButton;

    private SliderInt volume;
    private SliderInt music;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;

        mainMenu = root.Q<VisualElement>("MainMenu");
        settingsMenu = root.Q<VisualElement>("SettingsMenu");
        creditsMenu = root.Q<VisualElement>("CreditsMenu");

        playButton = root.Q<Button>("PlayButton");
        settingsButton = root.Q<Button>("SettingsButton");
        creditsButton = root.Q<Button>("CreditsButton");
        quitButton = root.Q<Button>("QuitButton");
        backButton = root.Q<Button>("BackButton");
        creditBackButton = root.Q<Button>("CreditBackButton");

        volume = root.Q<SliderInt>("VolumeSlider");
        music = root.Q<SliderInt>("MusicSlider");


        Show();

        if (playButton != null)
            playButton.clicked += () => { OnClickSound(); OnPlayButtonClicked(); };

        if (settingsButton != null)
            settingsButton.clicked += () => { OnClickSound(); OnSettingsButtonClicked(); };

        if (creditsButton != null)
            creditsButton.clicked += () => { OnClickSound(); OnCreditsButtonClicked(); };

        if (quitButton != null)
            quitButton.clicked += () => { OnClickSound(); OnQuitButtonClicked(); };

        if (backButton != null)
            backButton.clicked += () => { OnClickSound(); OnBackSettingsButtonClicked(); };
        if (creditBackButton != null)
            creditBackButton.clicked += () => { OnClickSound(); OnCreditsBackButtonClicked(); };
    }
    private void OnClickSound()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
    private void OnPlayButtonClicked()
    {
        Hide();
        load.LoadScene(1);
    }
    private void OnSettingsButtonClicked()
    {
        Hide();
        settingsMenu.style.display = DisplayStyle.Flex;
    }
    private void OnCreditsButtonClicked()
    {
        Hide();
        creditsMenu.style.display = DisplayStyle.Flex;
    }
    private void OnBackSettingsButtonClicked()
    {
        settingsMenu.style.display = DisplayStyle.None;
        Show();
    }
    private void OnCreditsBackButtonClicked()
    {
        creditsMenu.style.display = DisplayStyle.None;
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
        if (mainMenu != null)
            mainMenu.style.display = DisplayStyle.Flex;
    }
    public void Hide()
    {
        if (mainMenu != null || Input.GetButtonDown("Pause"))
            mainMenu.style.display = DisplayStyle.None;
    }
}
