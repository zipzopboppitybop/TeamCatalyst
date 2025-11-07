using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private Catalyst.Player.PlayerData playerData;

    private VisualElement root;
    private VisualElement healthBar;
    private Label healthBarLabel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(uIDocument == null)  
            uIDocument = GetComponent<UIDocument>();

        root = uIDocument.rootVisualElement;

        healthBar = root.Q<VisualElement>("HealthBarGREEN");

        UpdateHealthBar();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        float healthPercent = playerData.Health / playerData.HealthMax;
        healthBar.style.width = Length.Percent(healthPercent * 100);
    }
}
