using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApartmentViewer : MonoBehaviour
{
    /// <summary>
    /// Contains a list of GUI variables for each type of apartment feature, to ensure they can all be shown properly.
    /// </summary>
    [System.Serializable]
    public class ApartmentFeatureGUIData
    {
        [HideInInspector] public string name;
        public string visibleName;
        public Sprite icon;
        public Image graphic;
        public Image legendGraphic;
        public Text legendText;

        public ApartmentFeatureGUIData(string newName)
        {
            name = newName;
        }
    }

    [Header("GUI elements")]
    public RectTransform viewWindow;
    public Text apartmentName;
    public Text rent;
    public Text bedrooms;
    public Text bathrooms;
    public Text carSpaces;
    public List<ApartmentFeatureGUIData> featureElements;

    void OnValidate()
    {
        // Validates array of features against enum types to ensure the correct ones are present.
        string[] names = System.Enum.GetNames(typeof(ApartmentFeatures));
        for (int i = 0; i < names.Length; i++)
        {
            // For each enum value, check if existing graphic data is present.
            ApartmentFeatureGUIData value = featureElements.Find((v) => v.name == names[i]);
            if (value != null)
            {
                // If so, shift it to the appropriate value in the graph
                featureElements.Remove(value);
                featureElements.Insert(i, value);
            }
            else // If not, add a new one
            {
                featureElements.Insert(i, new ApartmentFeatureGUIData(names[i]));
            }
        }

        // Removes excess entries
        if (featureElements.Count > names.Length)
        {
            featureElements.RemoveRange(names.Length, featureElements.Count - names.Length);
        }
    }
    void Awake()
    {
        foreach(ApartmentFeatureGUIData feature in featureElements)
        {
            feature.legendGraphic.sprite = feature.icon;
            feature.legendText.text = feature.visibleName;
        }
        
        viewWindow.gameObject.SetActive(false);
    }

    /// <summary>
    /// Checks for ApartmentData in a particular gameobject hit by a raycast, e.g. if the player's mouse cursor is hovering over it.
    /// </summary>
    /// <param name="rh"></param>
    public void PopulateWindow(RaycastHit rh)
    {
        ApartmentData apartment = rh.collider?.GetComponentInParent<ApartmentData>();
        if (rh.collider == null && apartment == null)
        {
            viewWindow.gameObject.SetActive(false);
            return;
        }
        PopulateWindow(apartment);
    }
    /// <summary>
    /// Updates window to show details for a specific apartment.
    /// </summary>
    /// <param name="apartment"></param>
    public void PopulateWindow(ApartmentData apartment)
    {
        apartmentName.text = apartment.name;
        rent.text = "$" + apartment.rentPerWeek;
        bedrooms.text = apartment.bedrooms.ToString();
        bathrooms.text = apartment.bathrooms.ToString();
        carSpaces.text = apartment.carSpaces.ToString();

        var allFeatures = System.Enum.GetValues(typeof(ApartmentFeatures));
        int nextGraphic = 0;

        // Cycle through all ApartmentFeatures enum values
        // If apartment.features has one as a flag, populate the next graphic.
        for (int i = 0; i < allFeatures.Length; i++)
        {
            if (apartment.features.HasFlag((System.Enum)allFeatures.GetValue(i)))
            {
                featureElements[nextGraphic].graphic.gameObject.SetActive(true);
                featureElements[nextGraphic].graphic.sprite = featureElements[i].icon;
                nextGraphic++;
            }
        }

        // Disable all remaining icons, if less features are shown than the amount of icon graphics
        for (nextGraphic = nextGraphic; nextGraphic < allFeatures.Length; nextGraphic++)
        {
            featureElements[nextGraphic].graphic.gameObject.SetActive(false);
        }

        viewWindow.gameObject.SetActive(true);
    }
}
