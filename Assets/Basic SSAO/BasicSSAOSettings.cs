using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicSSAOSettings", menuName = "Basic SSAO/BasicSSAOSettings", order = 0)]
public class BasicSSAOSettings : ScriptableObject
{
    [Header("Assets")]
    
    [Tooltip("Please attach there texture called BasicSSAORandomTexture, you can find it in BasicSSAO folder")]
    public Texture RandomTexture;

    [Header("SSAO normals reconstruction")]

    [Tooltip("Setting deciding whether SSAO will reconstruct normals using simple method or more complex one, complex one solves artifacts seen on borders of geometry but affects performance. If these artifacts are ok for you keep this option disabled.")]
    public bool UseComplexNormalsReconstruction = false;

    //======== UNIFORMS SECTION ========//

    [Header("SSAO main properties")]

    [Tooltip("Is effect enabled?")]
    public bool EffectEnabled = true;

    [Tooltip("Color of ambient occlusion")]
    public Color SSAOColor = Color.black;

    [Tooltip("Amount of samples taken per one pixel, higher value equals better result and significiant performance hit.")]
    [Range(1, 64)]
    public int SamplesCount = 8;
  
    [Tooltip("Radius of ao effect")]
    [Range(0, 0.3f)]
    public float Radius = 0.1f;

    [Tooltip("Parameter to help reducing self shadowing effect, high value means less self shadowing but also less detail of objects on scene, you have to find balance.")]
    [Range(0, 1)]
    public float SelfShadowingReduction = 0.08f;
  
    [Tooltip("Changes the maximum acceptable depth difference between pixels")]
    [Range(0, 1.5f)]
    public float AcceptableDepthDifference = 0.013f;

    [Tooltip("Intensity of AO factor")]
    [Range(0.001f, 20f)]
    public float OcclusionFactor = 5f;

    [Tooltip("Downsample AO texture to achieve better performance at cost of visuals, Downsampling equal to 1 means no downsampling.")]
    [Range(1, 4)]
    public int Downsampling = 1;

    [Header("SSAO depth cutoff")]

    [Tooltip("Distance after which there should no be depth occlusion on objects, used to remove artifacts on objects far away. 1 = camera far plane")]
    [Range(0, 1)]
    public float DepthCutoff = 0.083f;

    [Tooltip("Parameter used to achieve nice transition effect instead of hard cutoff (if you want hard cutoff set it to 0), higher value means longer AO fading distance")]
    [Range(0, 0.5f)]
    public float DepthCutoffSmoothTransitionRange = 0.01f;

    [Header("SSAO Blur Settings")]

    [Tooltip("Is blurring AO enabled?")]
    public bool BlurEnabled = true;
     
    [Range(1, 6)]
    [Tooltip("How many times AO texture will be blurred, bigger value = better blur but worse performance, value between 2 and 4 should give good results.")]
    public int BlurIterations = 2;

    [Range(0, 0.008f)]
    [Tooltip("Radius of blur.")]
    public float BlurRadius = 0.003f;

    [Tooltip("Low values remove visible AO bleeding on edges of geometry. Value used by biliteral blur, determines if neightbour pixels are close enough on z axis to be blurred.")]
    public float BlurDepthFalloff = 0.1f;

    [Header("Blending SSAO With Scene Settings")]

    [Tooltip("Value used at combining AO with scene stage. Exponientaly increases ao factor (or decreases depends on ao value per pixel), basically strong black points will be even stronger and weak grey points will be weaker.")]
    [Range(1f, 20f)]
    public float AOIntensity = 3.0f;

    [Header("Debug")]
    public bool ShowAOTextureOnly = false;
}
