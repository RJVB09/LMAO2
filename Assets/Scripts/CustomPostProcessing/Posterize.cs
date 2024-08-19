using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(PosterizeRenderer), PostProcessEvent.AfterStack, "LMAO2/Posterize")]
public sealed class Posterize : PostProcessEffectSettings
{
    [Range(1, 256), Tooltip("The amount of levels a color should be posterized in")]
    public IntParameter posterizeLevels = new IntParameter { value = 256 };
    [Range(0f, 10f), Tooltip("A brightness modification")]
    public FloatParameter colorMultiplier = new FloatParameter { value = 1 };
}

public sealed class PosterizeRenderer : PostProcessEffectRenderer<Posterize>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/LMAO2/Posterize"));
        sheet.properties.SetInt("_Levels", settings.posterizeLevels);
        sheet.properties.SetFloat("_Multiplier", settings.colorMultiplier);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
