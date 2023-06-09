using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    [Header("Default Active Button")]
    [Tooltip("Should the default button be active on pannel active?")]
    public bool activateDefaultButtonColor = true;
    [Tooltip("Reference to the default button for the default panel")]
    public Button defaultButton;

    [Header("Default Panel")]
    [Tooltip("Should the script activate the panel under this check when activating self?")]
    public bool activateDefaultPanel = true;
    [Tooltip("Reference to the default shown panel")]
    public GameObject defaultPanelReference;

    [Header("Panels References")]
    [Tooltip("Put all the panels to switch between")]
    public List<GameObject> panelsList;

    private void OnEnable()
    {
        if (activateDefaultPanel && defaultPanelReference != null)
        {
            defaultPanelReference.SetActive(true);
        }
        if (activateDefaultButtonColor)
        {
            defaultButton.Select();
        }
    }

    private void OnDisable()
    {
        if (activateDefaultPanel && defaultPanelReference != null)
        {
            SwitchTo(defaultPanelReference);
        }
        else
        {
            SwitchTo(null);
        }

        if (activateDefaultButtonColor)
        {
            defaultButton.Select();
        }
    }

    public void SwitchTo(GameObject panel)
    {
        foreach (var item in panelsList)
        {
            if (item.activeSelf == true)
            {
                item.SetActive(false);
            }
        }

        if (panel != null)
        {
            GameObject clickedPanel = panelsList.FirstOrDefault(x => x.name == panel.name);
            clickedPanel.SetActive(true);
        }
    }
}
