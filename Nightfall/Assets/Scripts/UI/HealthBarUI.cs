using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private Catalyst.Player.PlayerData playerData;

    private VisualElement root;
    private VisualElement HUD;
    private VisualElement healthBar;
    private VisualElement staminaBar;
    private Label currencyLabel;
    //private Label healthBarLabel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(uIDocument == null)  
            uIDocument = GetComponent<UIDocument>();

        root = uIDocument.rootVisualElement;
        HUD = root.Q<VisualElement>("HUDContainer");
        healthBar = root.Q<VisualElement>("HealthBarGREEN");
        staminaBar = root.Q<VisualElement>("StamBarFront");
        currencyLabel = root.Q<Label>("moneyText");

        HUD.style.display = DisplayStyle.Flex;
        UpdateHealthBar();
        UpdateStaminaBar();
        UpdateCurrency();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealthBar();
        UpdateStaminaBar();
        UpdateCurrency();
    }

    private void UpdateHealthBar()
    {
        float healthPercent = playerData.Health / playerData.HealthMax;
        healthBar.style.width = Length.Percent(healthPercent * 100);
    }

    private void UpdateStaminaBar()
    {
        RegenStamina();

        float staminaPercent = (float)playerData.Stamina / playerData.StaminaMax;
        staminaBar.style.width = Length.Percent(staminaPercent * 100);

    }

    private void RegenStamina()
    {
        if(playerData.Stamina < playerData.StaminaMax)
        {
            playerData.Stamina += (int)(playerData.StaminaRegen * Time.deltaTime);
        }
    }
    private void UpdateCurrency()
    {
        currencyLabel.text = $"${playerData.Currency}";
    }
}
