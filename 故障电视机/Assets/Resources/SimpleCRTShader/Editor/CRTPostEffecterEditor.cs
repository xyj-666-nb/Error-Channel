using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

[CustomEditor(typeof(CRTPostEffecter))]
public class CRTPostEffecterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CRTPostEffecter effect = target as CRTPostEffecter;
        if (effect == null) return;

        // 核心材质
        effect.material = (Material)EditorGUILayout.ObjectField(
            "Effect Material", effect.material, typeof(Material), false
        );

        // 白噪音设置
        using (new HorizontalScope(GUI.skin.box))
        {
            effect.whiteNoiseFrequency = EditorGUILayout.IntField(
                "White Noise Freaquency (x/1000)", effect.whiteNoiseFrequency
            );
            effect.whiteNoiseLength = EditorGUILayout.FloatField(
                "White Noise Time Left (sec)", effect.whiteNoiseLength
            );
        }

        // 屏幕跳动设置
        using (new VerticalScope(GUI.skin.box))
        {
            effect.screenJumpFrequency = EditorGUILayout.IntField(
                "Screen Jump Freaquency (x/1000)", effect.screenJumpFrequency
            );
            effect.screenJumpLength = EditorGUILayout.FloatField(
                "Screen Jump Length", effect.screenJumpLength
            );
            using (new HorizontalScope())
            {
                effect.screenJumpMinLevel = EditorGUILayout.FloatField("min", effect.screenJumpMinLevel);
                effect.screenJumpMaxLevel = EditorGUILayout.FloatField("max", effect.screenJumpMaxLevel);
            }
        }

        // 扫描线
        using (new HorizontalScope(GUI.skin.box))
        {
            effect.isScanline = EditorGUILayout.Toggle("Scanline On / Off", effect.isScanline);
        }

        // 单色模式
        using (new HorizontalScope(GUI.skin.box))
        {
            effect.isMonochrome = EditorGUILayout.Toggle("Monochrome On / Off", effect.isMonochrome);
        }

        // 闪烁效果
        using (new HorizontalScope(GUI.skin.box))
        {
            effect.flickeringStrength = EditorGUILayout.FloatField(
                "Flickering Strength", effect.flickeringStrength
            );
            effect.flickeringCycle = EditorGUILayout.FloatField(
                "Flickering Cycle", effect.flickeringCycle
            );
        }

        // 滑动效果
        using (new VerticalScope(GUI.skin.box))
        {
            effect.isSlippage = EditorGUILayout.Toggle("Slippage On / Off", effect.isSlippage);
            effect.isSlippageNoise = EditorGUILayout.Toggle("Slippage Noise", effect.isSlippageNoise);
            effect.slippageStrength = EditorGUILayout.FloatField("Slippage Strength", effect.slippageStrength);
            effect.slippageInterval = EditorGUILayout.FloatField("Slippage Interval", effect.slippageInterval);
            effect.slippageScrollSpeed = EditorGUILayout.FloatField("Slippage Scroll Speed", effect.slippageScrollSpeed);
            effect.slippageSize = EditorGUILayout.FloatField("Slippage Size", effect.slippageSize);
        }

        // 色差效果
        using (new VerticalScope(GUI.skin.box))
        {
            effect.isChromaticAberration = EditorGUILayout.Toggle(
                "Chromatic Aberration On / Off", effect.isChromaticAberration
            );
            effect.chromaticAberrationStrength = EditorGUILayout.FloatField(
                "Chromatic Aberration Strength", effect.chromaticAberrationStrength
            );
        }

        // 多重鬼影
        using (new VerticalScope(GUI.skin.box))
        {
            effect.isMultipleGhost = EditorGUILayout.Toggle("Multiple Ghost On / Off", effect.isMultipleGhost);
            effect.multipleGhostStrength = EditorGUILayout.FloatField(
                "Multiple Ghost Strength", effect.multipleGhostStrength
            );
        }

        // LetterBox
        using (new VerticalScope(GUI.skin.box))
        {
            effect.isLetterBox = EditorGUILayout.Toggle("Letter Box On / Off", effect.isLetterBox);
            effect.letterBoxType = (CRTPostEffecter.LeterBoxType)EditorGUILayout.EnumPopup(
                "Letter Box Type", effect.letterBoxType
            );
            effect.isLetterBoxEdgeBlur = EditorGUILayout.Toggle("Letter Box Edge Blur", effect.isLetterBoxEdgeBlur);
        }

        // 贴图水印
        using (new VerticalScope(GUI.skin.box))
        {
            effect.isDecalTex = EditorGUILayout.Toggle("Decal Tex On / Off", effect.isDecalTex);
            effect.decalTex = (Texture2D)EditorGUILayout.ObjectField(
                "Decal Tex", effect.decalTex, typeof(Texture2D), false
            );
            effect.decalTexPos = EditorGUILayout.Vector2Field("Decal Tex Position", effect.decalTexPos);
            effect.decalTexScale = EditorGUILayout.Vector2Field("Decal Tex Scale", effect.decalTexScale);
        }

        // 低分辨率
        using (new VerticalScope(GUI.skin.box))
        {
            effect.isLowResolution = EditorGUILayout.Toggle("Low Resolution", effect.isLowResolution);
            effect.resolutions = EditorGUILayout.Vector2IntField("Resolutions", effect.resolutions);
        }

        // 胶片污渍
        using (new VerticalScope(GUI.skin.box))
        {
            effect.isFilmDirt = EditorGUILayout.Toggle("Film Dirt", effect.isFilmDirt);
            effect.filmDirtTex = (Texture2D)EditorGUILayout.ObjectField(
                "Film Dirt Tex", effect.filmDirtTex, typeof(Texture2D), false
            );
        }

        // UV范围设置
        using (new VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("渲染范围（UV 0-1）", EditorStyles.boldLabel);
            using (new HorizontalScope())
            {
                EditorGUILayout.LabelField("起点 (左, 下)", GUILayout.Width(80));
                effect.effectRange.x = EditorGUILayout.Slider(effect.effectRange.x, 0, 1);
                effect.effectRange.y = EditorGUILayout.Slider(effect.effectRange.y, 0, 1);
            }
            using (new HorizontalScope())
            {
                EditorGUILayout.LabelField("大小 (宽, 高)", GUILayout.Width(80));
                effect.effectRange.width = EditorGUILayout.Slider(
                    effect.effectRange.width, 0, 1 - effect.effectRange.x
                );
                effect.effectRange.height = EditorGUILayout.Slider(
                    effect.effectRange.height, 0, 1 - effect.effectRange.y
                );
            }
        }

        // 调试设置
        using (new VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("调试设置", EditorStyles.boldLabel);
            effect.showDebugRange = EditorGUILayout.Toggle("显示UV范围调试框", effect.showDebugRange);
            if (effect.showDebugRange)
            {
                effect.debugRangeColor = EditorGUILayout.ColorField("调试框颜色", effect.debugRangeColor);
            }
        }

        // 交互设置
        using (new VerticalScope(GUI.skin.box))
        {
            EditorGUILayout.LabelField("玩家交互设置", EditorStyles.boldLabel);
            effect.allowPlayerAdjust = EditorGUILayout.Toggle("允许玩家调整范围", effect.allowPlayerAdjust);
            if (effect.allowPlayerAdjust)
            {
                effect.edgeDetectDistance = EditorGUILayout.FloatField(
                    "边缘检测距离（像素）", effect.edgeDetectDistance
                );
            }
        }

        EditorUtility.SetDirty(target);
    }
}