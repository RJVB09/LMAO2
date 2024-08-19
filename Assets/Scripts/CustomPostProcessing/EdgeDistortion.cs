using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(EdgeDistortionRenderer), PostProcessEvent.BeforeStack, "LMAO2/EdgeDistortion")]
public sealed class EdgeDistortion : PostProcessEffectSettings
{
    [Range(0f, 10f), Tooltip("Noise radial frequency")]
    public FloatParameter radialFrequency = new FloatParameter { value = 10f };
    [Range(0f, 1000f), Tooltip("Noise angular frequency")]
    public FloatParameter angularFrequency = new FloatParameter { value = 262f };
    [Range(0f, 100f), Tooltip("TimeEvolution")]
    public FloatParameter timeEvolution = new FloatParameter { value = 100f };
    [Range(0f, 1f), Tooltip("Strength of the effect")]
    public FloatParameter strength = new FloatParameter { value = 0.038f };
    [Range(0f, 10f), Tooltip("Strength of the effect")]
    public FloatParameter falloff = new FloatParameter { value = 8f };
}

public sealed class EdgeDistortionRenderer : PostProcessEffectRenderer<EdgeDistortion>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/LMAO2/EdgeDistortion"));
        sheet.properties.SetFloat("_RadialFreq", settings.radialFrequency);
        sheet.properties.SetFloat("_AngularFreq", settings.angularFrequency);
        sheet.properties.SetFloat("_TimeEvolution", settings.timeEvolution);
        sheet.properties.SetFloat("_Strength", settings.strength);
        sheet.properties.SetFloat("_Falloff", settings.falloff);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
