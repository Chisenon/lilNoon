#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace lilToon
{
    public class MoreDecalInspector : lilToonInspector
    {
        // Decal Count
        MaterialProperty decalCount;
        
        // Decal properties arrays (max 10 decals)
        private const int MAX_DECALS = 10;
        MaterialProperty[] decalColor = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalTex = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalTex_ST = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalTexAngle = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalShouldCopy = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalShouldFlipCopy = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalBlendMode = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalTex_UVMode = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalTexIsMSDF = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalSyncScale = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalUseAnimation = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAnimation = new MaterialProperty[MAX_DECALS];

        private static bool[] isShowDecal = new bool[MAX_DECALS];
        private static bool isShowDecalCountControl = true;
        private const string shaderName = "ChiseNote/MoreDecal";

        protected override void LoadCustomProperties(MaterialProperty[] props, Material material)
        {

            isCustomShader = true;

            ReplaceToCustomShaders();
            isShowRenderMode = !material.shader.name.Contains("Optional");

            decalCount = FindProperty("_DecalCount", props);

            // Load all decal properties (1-10)
            for(int i = 0; i < MAX_DECALS; i++)
            {
                int num = i + 1;
                decalColor[i] = FindProperty($"_Decal{num}Color", props);
                decalTex[i] = FindProperty($"_Decal{num}Tex", props);
                decalTex_ST[i] = FindProperty($"_Decal{num}Tex_ST", props);
                decalTexAngle[i] = FindProperty($"_Decal{num}TexAngle", props);
                decalShouldCopy[i] = FindProperty($"_Decal{num}ShouldCopy", props);
                decalShouldFlipCopy[i] = FindProperty($"_Decal{num}ShouldFlipCopy", props);
                decalBlendMode[i] = FindProperty($"_Decal{num}BlendMode", props);
                decalTex_UVMode[i] = FindProperty($"_Decal{num}Tex_UVMode", props);
                decalTexIsMSDF[i] = FindProperty($"_Decal{num}TexIsMSDF", props);
                decalSyncScale[i] = FindProperty($"_Decal{num}SyncScale", props);
                decalUseAnimation[i] = FindProperty($"_Decal{num}UseAnimation", props);
                decalAnimation[i] = FindProperty($"_Decal{num}TexDecalAnimation", props);
            }
        }

        protected override void DrawCustomProperties(Material material)
        {
            // GUIStyles Name   Description
            // ---------------- ------------------------------------
            // boxOuter         outer box
            // boxInnerHalf     inner box
            // boxInner         inner box without label
            // customBox        box (similar to unity default box)
            // customToggleFont label for box

            // Decal Count Control at Top
            isShowDecalCountControl = Foldout("Decal Count Control", "Decal Count Control", isShowDecalCountControl);
            if(isShowDecalCountControl)
            {
                EditorGUILayout.BeginVertical(boxOuter);
                EditorGUILayout.BeginVertical(boxInnerHalf);
                
                EditorGUI.BeginChangeCheck();
                m_MaterialEditor.ShaderProperty(decalCount, "Add Decal (0-10)");
                if(EditorGUI.EndChangeCheck())
                {
                    decalCount.floatValue = Mathf.Clamp(decalCount.floatValue, 0, MAX_DECALS);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }

            // Get current decal count
            int currentDecalCount = Mathf.RoundToInt(decalCount.floatValue);

            // Draw decal sections dynamically based on count (only if count > 0)
            if(currentDecalCount > 0)
            {
                for(int i = 0; i < currentDecalCount; i++)
                {
                    int num = i + 1;
                    DrawDecalSection(material, i, num);
                }
            }
        }

        private void DrawDecalSection(Material material, int index, int displayNum)
        {
            string sectionName = $"Decal {displayNum}";
            isShowDecal[index] = Foldout(sectionName, sectionName, isShowDecal[index]);
            
            if(isShowDecal[index])
            {
                EditorGUILayout.BeginVertical(boxOuter);
                EditorGUILayout.LabelField(sectionName, customToggleFont);
                EditorGUILayout.BeginVertical(boxInnerHalf);

                EditorGUI.indentLevel++;
                    m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Texture"), decalTex[index], decalColor[index]);
                    lilEditorGUI.DrawColorAsAlpha(decalColor[index]);
                    lilEditorGUI.LocalizedProperty(m_MaterialEditor, decalTexIsMSDF[index]);
                    lilEditorGUI.LocalizedProperty(m_MaterialEditor, decalTex_UVMode[index]);
                    lilEditorGUI.LocalizedProperty(m_MaterialEditor, decalBlendMode[index]);
                    
                    DrawLine();

                    // Copy mode
                        int copyMode = 0;
                        if(decalShouldCopy[index].floatValue == 1.0f) copyMode = 1;
                        if(decalShouldFlipCopy[index].floatValue == 1.0f) copyMode = 2;

                        EditorGUI.BeginChangeCheck();
                        string cmlabel = Event.current.alt ? decalShouldCopy[index].name + ", " + decalShouldFlipCopy[index].name : lilLanguageManager.GetLoc("sCopyMode");
                        copyMode = lilEditorGUI.Popup(cmlabel, copyMode, new string[]{lilLanguageManager.GetLoc("sCopyModeNormal"), lilLanguageManager.GetLoc("sCopyModeSymmetry"), lilLanguageManager.GetLoc("sCopyModeFlip")});
                        if(EditorGUI.EndChangeCheck())
                        {
                            if(copyMode == 0)
                            {
                                decalShouldCopy[index].floatValue = 0.0f;
                                decalShouldFlipCopy[index].floatValue = 0.0f;
                            }
                            if(copyMode == 1)
                            {
                                decalShouldCopy[index].floatValue = 1.0f;
                                decalShouldFlipCopy[index].floatValue = 0.0f;
                            }
                            if(copyMode == 2)
                            {
                                decalShouldCopy[index].floatValue = 1.0f;
                                decalShouldFlipCopy[index].floatValue = 1.0f;
                            }
                        }

                    // Load scale & offset
                    float scaleX = decalTex[index].textureScaleAndOffset.x;
                    float scaleY = decalTex[index].textureScaleAndOffset.y;
                    float posX = decalTex[index].textureScaleAndOffset.z;
                    float posY = decalTex[index].textureScaleAndOffset.w;

                    if(scaleX==0.0f)
                    {
                        posX = 0.5f;
                        scaleX = 0.000001f;
                    }
                    else
                    {
                        // Convert UV_ST.z to Position: 0/0=bottom-left, 0.5/0.5=center, 1/1=top-right
                        posX = (0.5f - posX) / scaleX + 0.5f;
                        scaleX = 1.0f / scaleX;
                    }

                    if(scaleY==0.0f)
                    {
                        posY = 0.5f;
                        scaleY = 0.000001f;
                    }
                    else
                    {
                        // Convert UV_ST.w to Position: 0/0=bottom-left, 0.5/0.5=center, 1/1=top-right
                        posY = (0.5f - posY) / scaleY + 0.5f;
                        scaleY = 1.0f / scaleY;
                    }
                    scaleX = lilEditorGUI.RoundFloat1000000(scaleX);
                    scaleY = lilEditorGUI.RoundFloat1000000(scaleY);
                    posX = lilEditorGUI.RoundFloat1000000(posX);
                    posY = lilEditorGUI.RoundFloat1000000(posY);

                    EditorGUI.BeginChangeCheck();
                    if(copyMode > 0)
                    {
                        if(posX < 0.5f) posX = 1.0f - posX;
                        posX = EditorGUILayout.Slider(Event.current.alt ? decalTex[index].name + "_ST.z" : lilLanguageManager.GetLoc("sPositionX"), posX, 0.5f, 1.0f);
                    }
                    else
                    {
                        posX = EditorGUILayout.Slider(Event.current.alt ? decalTex[index].name + "_ST.z" : lilLanguageManager.GetLoc("sPositionX"), posX, 0.0f, 1.0f);
                    }

                    posY = EditorGUILayout.Slider(Event.current.alt ? decalTex[index].name + "_ST.w" : lilLanguageManager.GetLoc("sPositionY"), posY, 0.0f, 1.0f);
                    
                    // Sync Scale toggle
                    m_MaterialEditor.ShaderProperty(decalSyncScale[index], "Sync Scale");
                    
                    if(decalSyncScale[index].floatValue == 1.0f)
                    {
                        // Synchronized scale slider
                        float syncScale = scaleX;
                        syncScale = EditorGUILayout.Slider("X / Y Scale", syncScale, -1.0f, 1.0f);
                        scaleX = syncScale;
                        scaleY = syncScale;
                    }
                    else
                    {
                        // Separate scale sliders
                        scaleX = EditorGUILayout.Slider(Event.current.alt ? decalTex[index].name + "_ST.x" : lilLanguageManager.GetLoc("sScaleX"), scaleX, -1.0f, 1.0f);
                        scaleY = EditorGUILayout.Slider(Event.current.alt ? decalTex[index].name + "_ST.y" : lilLanguageManager.GetLoc("sScaleY"), scaleY, -1.0f, 1.0f);
                    }
                    if(EditorGUI.EndChangeCheck())
                    {
                        if(scaleX == 0.0f) scaleX = 0.000001f;
                        if(scaleY == 0.0f) scaleY = 0.000001f;

                        scaleX = 1.0f / scaleX;
                        scaleY = 1.0f / scaleY;
                        // Convert Position to UV_ST offset: 0/0=bottom-left, 0.5/0.5=center, 1/1=top-right
                        posX = (-(posX - 0.5f) * scaleX) + 0.5f;
                        posY = (-(posY - 0.5f) * scaleY) + 0.5f;

                        decalTex[index].textureScaleAndOffset = new Vector4(scaleX, scaleY, posX, posY);
                    }
                    
                    lilEditorGUI.LocalizedProperty(m_MaterialEditor, decalTexAngle[index]);
                    
                    DrawLine();
                    
                    // Animation toggle
                    m_MaterialEditor.ShaderProperty(decalUseAnimation[index], "Animation");
                    
                    if(decalUseAnimation[index].floatValue == 1.0f)
                    {
                        EditorGUI.indentLevel++;
                        
                        // Animation parameters
                        Vector4 animVec = decalAnimation[index].vectorValue;
                        int loopX = (int)animVec.x;
                        int loopY = (int)animVec.y;
                        int frames = (int)animVec.z;
                        float speed = animVec.w;
                        
                        EditorGUI.BeginChangeCheck();
                        
                        // X/Y frames in horizontal layout (like Scroll)
                        var positionVec2 = EditorGUILayout.GetControlRect();
                        float labelWidth = EditorGUIUtility.labelWidth;
                        var labelRect = new Rect(positionVec2.x, positionVec2.y, labelWidth, positionVec2.height);
                        EditorGUI.PrefixLabel(labelRect, new GUIContent(Event.current.alt ? decalAnimation[index].name + ".xy" : "X / Y Frames"));
                        
                        int indentBuf = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0;
                        
                        var vecRect = new Rect(positionVec2.x + labelWidth, positionVec2.y, positionVec2.width - labelWidth, positionVec2.height);
                        Vector2 framesVec = new Vector2(loopX, loopY);
                        framesVec = EditorGUI.Vector2Field(vecRect, GUIContent.none, framesVec);
                        loopX = (int)framesVec.x;
                        loopY = (int)framesVec.y;
                        
                        EditorGUI.indentLevel = indentBuf;
                        
                        frames = EditorGUI.IntField(EditorGUILayout.GetControlRect(), Event.current.alt ? decalAnimation[index].name + ".z" : "Total Frames", frames);
                        speed = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), Event.current.alt ? decalAnimation[index].name + ".w" : "FPS", speed);
                        
                        if(EditorGUI.EndChangeCheck())
                        {
                            decalAnimation[index].vectorValue = new Vector4(loopX, loopY, frames, speed);
                        }
                        
                        EditorGUI.indentLevel--;
                    }

                    DrawLine();

                    
                    
                    EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
        }

        protected override void ReplaceToCustomShaders()
        {
            lts         = Shader.Find(shaderName + "/lilToon");
            ltsc        = Shader.Find("Hidden/" + shaderName + "/Cutout");
            ltst        = Shader.Find("Hidden/" + shaderName + "/Transparent");
            ltsot       = Shader.Find("Hidden/" + shaderName + "/OnePassTransparent");
            ltstt       = Shader.Find("Hidden/" + shaderName + "/TwoPassTransparent");

            ltso        = Shader.Find("Hidden/" + shaderName + "/OpaqueOutline");
            ltsco       = Shader.Find("Hidden/" + shaderName + "/CutoutOutline");
            ltsto       = Shader.Find("Hidden/" + shaderName + "/TransparentOutline");
            ltsoto      = Shader.Find("Hidden/" + shaderName + "/OnePassTransparentOutline");
            ltstto      = Shader.Find("Hidden/" + shaderName + "/TwoPassTransparentOutline");

            ltsoo       = Shader.Find(shaderName + "/[Optional] OutlineOnly/Opaque");
            ltscoo      = Shader.Find(shaderName + "/[Optional] OutlineOnly/Cutout");
            ltstoo      = Shader.Find(shaderName + "/[Optional] OutlineOnly/Transparent");

            ltstess     = Shader.Find("Hidden/" + shaderName + "/Tessellation/Opaque");
            ltstessc    = Shader.Find("Hidden/" + shaderName + "/Tessellation/Cutout");
            ltstesst    = Shader.Find("Hidden/" + shaderName + "/Tessellation/Transparent");
            ltstessot   = Shader.Find("Hidden/" + shaderName + "/Tessellation/OnePassTransparent");
            ltstesstt   = Shader.Find("Hidden/" + shaderName + "/Tessellation/TwoPassTransparent");

            ltstesso    = Shader.Find("Hidden/" + shaderName + "/Tessellation/OpaqueOutline");
            ltstessco   = Shader.Find("Hidden/" + shaderName + "/Tessellation/CutoutOutline");
            ltstessto   = Shader.Find("Hidden/" + shaderName + "/Tessellation/TransparentOutline");
            ltstessoto  = Shader.Find("Hidden/" + shaderName + "/Tessellation/OnePassTransparentOutline");
            ltstesstto  = Shader.Find("Hidden/" + shaderName + "/Tessellation/TwoPassTransparentOutline");

            ltsl        = Shader.Find(shaderName + "/lilToonLite");
            ltslc       = Shader.Find("Hidden/" + shaderName + "/Lite/Cutout");
            ltslt       = Shader.Find("Hidden/" + shaderName + "/Lite/Transparent");
            ltslot      = Shader.Find("Hidden/" + shaderName + "/Lite/OnePassTransparent");
            ltsltt      = Shader.Find("Hidden/" + shaderName + "/Lite/TwoPassTransparent");

            ltslo       = Shader.Find("Hidden/" + shaderName + "/Lite/OpaqueOutline");
            ltslco      = Shader.Find("Hidden/" + shaderName + "/Lite/CutoutOutline");
            ltslto      = Shader.Find("Hidden/" + shaderName + "/Lite/TransparentOutline");
            ltsloto     = Shader.Find("Hidden/" + shaderName + "/Lite/OnePassTransparentOutline");
            ltsltto     = Shader.Find("Hidden/" + shaderName + "/Lite/TwoPassTransparentOutline");

            ltsref      = Shader.Find("Hidden/" + shaderName + "/Refraction");
            ltsrefb     = Shader.Find("Hidden/" + shaderName + "/RefractionBlur");
            ltsfur      = Shader.Find("Hidden/" + shaderName + "/Fur");
            ltsfurc     = Shader.Find("Hidden/" + shaderName + "/FurCutout");
            ltsfurtwo   = Shader.Find("Hidden/" + shaderName + "/FurTwoPass");
            ltsfuro     = Shader.Find(shaderName + "/[Optional] FurOnly/Transparent");
            ltsfuroc    = Shader.Find(shaderName + "/[Optional] FurOnly/Cutout");
            ltsfurotwo  = Shader.Find(shaderName + "/[Optional] FurOnly/TwoPass");
            ltsgem      = Shader.Find("Hidden/" + shaderName + "/Gem");
            ltsfs       = Shader.Find(shaderName + "/[Optional] FakeShadow");

            ltsover     = Shader.Find(shaderName + "/[Optional] Overlay");
            ltsoover    = Shader.Find(shaderName + "/[Optional] OverlayOnePass");
            ltslover    = Shader.Find(shaderName + "/[Optional] LiteOverlay");
            ltsloover   = Shader.Find(shaderName + "/[Optional] LiteOverlayOnePass");

            ltsm        = Shader.Find(shaderName + "/lilToonMulti");
            ltsmo       = Shader.Find("Hidden/" + shaderName + "/MultiOutline");
            ltsmref     = Shader.Find("Hidden/" + shaderName + "/MultiRefraction");
            ltsmfur     = Shader.Find("Hidden/" + shaderName + "/MultiFur");
            ltsmgem     = Shader.Find("Hidden/" + shaderName + "/MultiGem");
        }

        // You can create a menu like this
        /*
        [MenuItem("Assets/TemplateFull/Convert material to custom shader", false, 1100)]
        private static void ConvertMaterialToCustomShaderMenu()
        {
            if(Selection.objects.Length == 0) return;
            TemplateFullInspector inspector = new TemplateFullInspector();
            for(int i = 0; i < Selection.objects.Length; i++)
            {
                if(Selection.objects[i] is Material)
                {
                    inspector.ConvertMaterialToCustomShader((Material)Selection.objects[i]);
                }
            }
        }
        */
    }
}
#endif