using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum ApartmentFeatures
{
    preFurnished = 1,
    kitchen = 2,
    laundry = 4,
    //airConditioning = 8,
    allowsPets = 16,
    balcony = 32,
    pool = 64
}

public class ApartmentData : MonoBehaviour
{
    public float rentPerWeek = 500;
    public int bedrooms = 1;
    public int bathrooms = 1;
    public int carSpaces = 1;
    public ApartmentFeatures features;
}
