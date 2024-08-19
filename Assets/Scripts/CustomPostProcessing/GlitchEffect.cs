using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(GlitchEffectRenderer), PostProcessEvent.AfterStack, "LMAO2/GlitchEffect")]
public sealed class GlitchEffect : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("")]
    public FloatParameter amount = new FloatParameter { value = 0f };
    [Range(0f, 1f), Tooltip("")]
    public FloatParameter pixelSize = new FloatParameter { value = 0f };
}

public sealed class GlitchEffectRenderer : PostProcessEffectRenderer<GlitchEffect>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/LMAO2/GlitchEffect"));
        sheet.properties.SetFloat("_Amount", settings.amount);
        sheet.properties.SetFloat("_PixelSize", settings.pixelSize);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
