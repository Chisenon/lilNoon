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
        MaterialProperty[] decalUseAudioLink = new MaterialProperty[MAX_DECALS];

        // AudioLink properties for decals
        MaterialProperty[] decalAudioLinkScaleBand = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAudioLinkScale = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAudioLinkSideBand = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAudioLinkSideMon = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAudioLinkSideMonMin = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAudioLinkSideMonMax = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAudioLinkRotationBand = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAudioLinkRotation = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAudioLinkChronoRotationBand = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAudioLinkChronoMotionType = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAudioLinkChronoRotationSpeed = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalShouldCopy = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalShouldFlipCopy = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalBlendMode = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalTex_UVMode = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalTexIsMSDF = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalSyncScale = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalUseAnimation = new MaterialProperty[MAX_DECALS];
        MaterialProperty[] decalAnimation = new MaterialProperty[MAX_DECALS];

        private static bool[] isShowDecal = new bool[MAX_DECALS];
        private static bool[] isShowDecalAudioLink = new bool[MAX_DECALS];
        
        // Toggle to show hitbox debug overlays (arrow, checkbox, label)
        private static bool showHitboxDebug = true;
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
                decalUseAudioLink[i] = FindProperty($"_Decal{num}UseAudioLink", props, false);
                
                // Load AudioLink-related properties (ScaleBand and Scale)
                decalAudioLinkScaleBand[i] = FindProperty($"_AudioLinkDecal{num}ScaleBand", props, false);
                decalAudioLinkScale[i] = FindProperty($"_AudioLinkDecal{num}Scale", props, false);
                decalAudioLinkSideBand[i] = FindProperty($"_AudioLinkDecal{num}SideBand", props, false);
                decalAudioLinkSideMon[i] = FindProperty($"_AudioLinkDecal{num}SideMon", props, false);
                decalAudioLinkSideMonMin[i] = FindProperty($"_AudioLinkDecal{num}SideMonMin", props, false);
                decalAudioLinkSideMonMax[i] = FindProperty($"_AudioLinkDecal{num}SideMonMax", props, false);
                decalAudioLinkRotationBand[i] = FindProperty($"_AudioLinkDecal{num}RotationBand", props, false);
                decalAudioLinkRotation[i] = FindProperty($"_AudioLinkDecal{num}Rotation", props, false);
                decalAudioLinkChronoRotationBand[i] = FindProperty($"_AudioLinkDecal{num}ChronoRotationBand", props, false);
                decalAudioLinkChronoMotionType[i] = FindProperty($"_AudioLinkDecal{num}ChronoMotionType", props, false);
                decalAudioLinkChronoRotationSpeed[i] = FindProperty($"_AudioLinkDecal{num}ChronoRotationSpeed", props, false);
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

        private bool FoldoutWithToggle(string label, bool foldout, MaterialProperty toggleProp)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 22);
            GUI.Box(rect, "", customBox);
            
            Rect innerRect = new Rect(rect.x + 4, rect.y + 2, rect.width - 8, rect.height - 4);
            
            const float arrowWidth = 14f;
            const float checkboxWidth = 18f;
            const float spacing = 2f;
            const float visualOffset = 2.0f;

            Rect drawRect = new Rect(innerRect.x + arrowWidth + visualOffset, innerRect.y, checkboxWidth, checkboxWidth);
            Rect labelRect = new Rect(drawRect.xMax + spacing, innerRect.y, innerRect.width - (arrowWidth + checkboxWidth + spacing + visualOffset), innerRect.height);
            
            if(toggleProp != null)
            {
                bool toggleValue = toggleProp.floatValue == 1.0f;
                
                // Handle toggle click
                if (Event.current.type == EventType.MouseDown && drawRect.Contains(Event.current.mousePosition))
                {
                    toggleProp.floatValue = toggleValue ? 0.0f : 1.0f;
                    Event.current.Use();
                    GUI.changed = true;
                    toggleValue = !toggleValue;
                }

                // Draw debug overlays and toggle
                int originalIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                
                if (showHitboxDebug)
                {
                    EditorGUI.DrawRect(new Rect(innerRect.x, innerRect.y, arrowWidth, innerRect.height), new Color(1f, 0f, 0f, 0.16f));
                    EditorGUI.DrawRect(drawRect, new Color(1f, 1f, 0f, 0.18f));
                    EditorGUI.DrawRect(labelRect, new Color(0f, 0f, 1f, 0.08f));
                }

                EditorGUI.Toggle(drawRect, toggleValue);
                EditorGUI.indentLevel = originalIndent;
            }
            
            // Draw foldout arrow and label
            bool newFoldout = EditorGUI.Foldout(rect, foldout, "", true, EditorStyles.foldout);
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);
            
            return newFoldout;
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

                    // AudioLink controls with Foldout+Toggle header (Poiyomi-style)
                    if(decalUseAudioLink[index] != null)
                    {
                        EditorGUILayout.Space(2);
                        // Modified function used here
                        isShowDecalAudioLink[index] = FoldoutWithToggle("AudioLink", isShowDecalAudioLink[index], decalUseAudioLink[index]);
                        
                        if(isShowDecalAudioLink[index])
                        {
                            EditorGUILayout.BeginVertical(boxOuter);
                            EditorGUILayout.BeginVertical(boxInnerHalf);

                            int originalIndent = EditorGUI.indentLevel;
                            EditorGUI.indentLevel = 0;

                            if(decalAudioLinkScaleBand[index] != null)
                            {
                                int scaleBand = (int)decalAudioLinkScaleBand[index].floatValue;
                                string[] bandOptions = new string[] {"Bass","Low Mid","High Mid","Treble","Volume"};
                                EditorGUI.BeginChangeCheck();
                                scaleBand = EditorGUILayout.Popup("Scale Band", scaleBand, bandOptions);
                                if(EditorGUI.EndChangeCheck())
                                {
                                    decalAudioLinkScaleBand[index].floatValue = scaleBand;
                                }
                            }
                            if(decalAudioLinkScale[index] != null)
                            {
                                // Split Vector4 into two Vector2 fields (minX/maxX, minY/maxY)
                                Vector4 scaleVec = decalAudioLinkScale[index].vectorValue;

                                EditorGUI.BeginChangeCheck();

                                // X Scale (minX, maxX)
                                var positionVec2X = EditorGUILayout.GetControlRect();
                                float labelWidth = EditorGUIUtility.labelWidth;
                                var labelRectX = new Rect(positionVec2X.x, positionVec2X.y, labelWidth, positionVec2X.height);
                                EditorGUI.PrefixLabel(labelRectX, new GUIContent(Event.current.alt ? decalAudioLinkScale[index].name + ".xz" : "Scale X (min / max)"));

                                var vecRectX = new Rect(positionVec2X.x + labelWidth, positionVec2X.y, positionVec2X.width - labelWidth, positionVec2X.height);
                                Vector2 audioScaleX = new Vector2(scaleVec.x, scaleVec.z);
                                audioScaleX = EditorGUI.Vector2Field(vecRectX, GUIContent.none, audioScaleX);

                                // Y Scale (minY, maxY)
                                var positionVec2Y = EditorGUILayout.GetControlRect();
                                var labelRectY = new Rect(positionVec2Y.x, positionVec2Y.y, labelWidth, positionVec2Y.height);
                                EditorGUI.PrefixLabel(labelRectY, new GUIContent(Event.current.alt ? decalAudioLinkScale[index].name + ".yw" : "Scale Y (min / max)"));

                                var vecRectY = new Rect(positionVec2Y.x + labelWidth, positionVec2Y.y, positionVec2Y.width - labelWidth, positionVec2Y.height);
                                Vector2 audioScaleY = new Vector2(scaleVec.y, scaleVec.w);
                                audioScaleY = EditorGUI.Vector2Field(vecRectY, GUIContent.none, audioScaleY);

                                if(EditorGUI.EndChangeCheck())
                                {
                                    decalAudioLinkScale[index].vectorValue = new Vector4(audioScaleX.x, audioScaleY.x, audioScaleX.y, audioScaleY.y);
                                }
                            }

                            DrawLine();


                            // --- Side controls (Side Band + Side Monitor min/max) ---
                            if(decalAudioLinkSideBand[index] != null)
                            {
                                int sideBand = (int)decalAudioLinkSideBand[index].floatValue;
                                string[] bandOptions = new string[] {"Bass","Low Mid","High Mid","Treble","Volume"};
                                EditorGUI.BeginChangeCheck();
                                sideBand = EditorGUILayout.Popup("Side Band", sideBand, bandOptions);
                                if(EditorGUI.EndChangeCheck())
                                {
                                    decalAudioLinkSideBand[index].floatValue = sideBand;
                                }
                            }

                            // Side Mod Min/Max (L / R / D / U)
                            GUIContent[] sideSubLabels = new GUIContent[] { new GUIContent("L"), new GUIContent("R"), new GUIContent("D"), new GUIContent("U") };
                            Vector4 minVec = Vector4.zero;
                            Vector4 maxVec = Vector4.zero;
                            if(decalAudioLinkSideMonMin[index] != null) minVec = decalAudioLinkSideMonMin[index].vectorValue;
                            if(decalAudioLinkSideMonMax[index] != null) maxVec = decalAudioLinkSideMonMax[index].vectorValue;

                            EditorGUI.BeginChangeCheck();

                            // Min row
                            var posMin = EditorGUILayout.GetControlRect();
                            var labRectMin = new Rect(posMin.x, posMin.y, EditorGUIUtility.labelWidth, posMin.height);
                            EditorGUI.PrefixLabel(labRectMin, new GUIContent("Side Mod Min"));
                            var vRectMin = new Rect(posMin.x + EditorGUIUtility.labelWidth, posMin.y, posMin.width - EditorGUIUtility.labelWidth, posMin.height);
                            float[] minVals = new float[] { minVec.x, minVec.y, minVec.z, minVec.w };
                            EditorGUI.MultiFloatField(vRectMin, GUIContent.none, sideSubLabels, minVals);

                            // Max row
                            var posMax = EditorGUILayout.GetControlRect();
                            var labRectMax = new Rect(posMax.x, posMax.y, EditorGUIUtility.labelWidth, posMax.height);
                            EditorGUI.PrefixLabel(labRectMax, new GUIContent("Side Mod Max"));
                            var vRectMax = new Rect(posMax.x + EditorGUIUtility.labelWidth, posMax.y, posMax.width - EditorGUIUtility.labelWidth, posMax.height);
                            float[] maxVals = new float[] { maxVec.x, maxVec.y, maxVec.z, maxVec.w };
                            EditorGUI.MultiFloatField(vRectMax, GUIContent.none, sideSubLabels, maxVals);

                            if(EditorGUI.EndChangeCheck())
                            {
                                if(decalAudioLinkSideMonMin[index] != null)
                                    decalAudioLinkSideMonMin[index].vectorValue = new Vector4(minVals[0], minVals[1], minVals[2], minVals[3]);
                                if(decalAudioLinkSideMonMax[index] != null)
                                    decalAudioLinkSideMonMax[index].vectorValue = new Vector4(maxVals[0], maxVals[1], maxVals[2], maxVals[3]);
                            }

                            DrawLine();

                            // Rotation controls (Rotation Band + Rotation min/max)
                            if(decalAudioLinkRotationBand[index] != null)
                            {
                                int rotationBand = (int)decalAudioLinkRotationBand[index].floatValue;
                                string[] bandOptions = new string[] {"Bass","Low Mid","High Mid","Treble","Volume"};
                                EditorGUI.BeginChangeCheck();
                                rotationBand = EditorGUILayout.Popup("Rotation Band", rotationBand, bandOptions);
                                if(EditorGUI.EndChangeCheck())
                                {
                                    decalAudioLinkRotationBand[index].floatValue = rotationBand;
                                }
                            }
                            if(decalAudioLinkRotation[index] != null)
                            {
                                Vector4 rotVec = decalAudioLinkRotation[index].vectorValue; // x=min, y=max
                                EditorGUI.BeginChangeCheck();

                                var posRot = EditorGUILayout.GetControlRect();
                                float lwRot = EditorGUIUtility.labelWidth;
                                var labRectRot = new Rect(posRot.x, posRot.y, lwRot, posRot.height);
                                EditorGUI.PrefixLabel(labRectRot, new GUIContent(Event.current.alt ? decalAudioLinkRotation[index].name + ".xy" : "Rotation (min / max)"));

                                var vRectRot = new Rect(posRot.x + lwRot, posRot.y, posRot.width - lwRot, posRot.height);
                                Vector2 rotVals = new Vector2(rotVec.x, rotVec.y);
                                rotVals = EditorGUI.Vector2Field(vRectRot, GUIContent.none, rotVals);

                                if(EditorGUI.EndChangeCheck())
                                {
                                    decalAudioLinkRotation[index].vectorValue = new Vector4(rotVals.x, rotVals.y, rotVec.z, rotVec.w);
                                }
                            }

                            DrawLine();

                            // Chrono Rotation controls (band / motion type / speed)
                            if(decalAudioLinkChronoRotationBand[index] != null)
                            {
                                int chronoRotationBand = (int)decalAudioLinkChronoRotationBand[index].floatValue;
                                string[] bandOptions = new string[] {"Bass","Low Mid","High Mid","Treble","Volume"};
                                EditorGUI.BeginChangeCheck();
                                chronoRotationBand = EditorGUILayout.Popup("Chrono Rotation Band", chronoRotationBand, bandOptions);
                                if(EditorGUI.EndChangeCheck())
                                {
                                    decalAudioLinkChronoRotationBand[index].floatValue = chronoRotationBand;
                                }
                            }

                            if(decalAudioLinkChronoMotionType[index] != null)
                            {
                                int curType = (int)decalAudioLinkChronoMotionType[index].floatValue;
                                string[] chronoOptions = new string[] {"None","Sine","Step","PingPong","Random"};
                                EditorGUI.BeginChangeCheck();
                                curType = EditorGUILayout.Popup(Event.current.alt ? decalAudioLinkChronoMotionType[index].name + ".x" : "Chrono motion type", curType, chronoOptions);
                                if(EditorGUI.EndChangeCheck())
                                {
                                    decalAudioLinkChronoMotionType[index].floatValue = curType;
                                }
                            }

                            if(decalAudioLinkChronoRotationSpeed[index] != null)
                            {
                                float speed = decalAudioLinkChronoRotationSpeed[index].floatValue;
                                EditorGUI.BeginChangeCheck();
                                speed = EditorGUILayout.FloatField(Event.current.alt ? decalAudioLinkChronoRotationSpeed[index].name + ".x" : "Chrono Rotation Speed", speed);
                                if(EditorGUI.EndChangeCheck())
                                {
                                    decalAudioLinkChronoRotationSpeed[index].floatValue = speed;
                                }
                            }

                            EditorGUI.indentLevel = originalIndent;

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();
                        }
                    }
                    
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
                        DrawLine();
                        
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
    }
}
#endif