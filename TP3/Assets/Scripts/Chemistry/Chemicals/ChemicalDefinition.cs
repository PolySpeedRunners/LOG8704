using UnityEngine;
using LiquidVolumeFX;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
public enum ChemicalType
{
    Water,
    PVA,
    PVA_Decomposed,
    PVA_Black,
}

[CreateAssetMenu(menuName = "Chemistry/Chemical Definition")]
public class ChemicalDefinition : ScriptableObject
{
    public ChemicalType type;

    [Header("Basic Visual")]
    public Color color;

    //[Header("Liquid Volume FX Settings")]
    //public float alpha = 1f;
    //public bool allowViewFromInside = true;
    //public bool autoCloseMesh = true;
    //public float backDepthBias = 0f;
    //public float blurIntensity = 0f;

    //[Header("Bubbles")]
    //public float bubblesAmount = 0f;
    //public float bubblesBrightness = 1f;
    //public float bubblesScale = 1f;
    //public int bubblesSeed = 0;
    //public float bubblesSizeMax = 0.1f;
    //public float bubblesSizeMin = 0.01f;
    //public float bubblesVerticalSpeed = 0.1f;

    //[Header("Turbulence")]
    //public float turbulence1 = 0f;
    //public float turbulence2 = 0f;

    //[Header("Sparkling")]
    //public float sparklingAmount = 0f;
    //public float sparklingIntensity = 0f;
    //public float speed = 0.1f;

    //[Header("Foam")]
    //public Color foamColor = Color.white;
    //public float foamDensity = 0f;
    //public float foamScale = 1f;
    //public float foamThickness = 0.1f;
    //public float foamTurbulence = 0.1f;
    //public bool foamVisibleFromBottom = true;
    //public float foamWeight = 1f;
}
