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
        private static bool[] isShowDecalTransform = new bool[MAX_DECALS];
        private static bool[] isShowDecalAudioLink = new bool[MAX_DECALS];
        private static bool[] isShowDecalAnimation = new bool[MAX_DECALS];
        private static bool isShowDecalCountControl = true;
        private static bool decalCountLock = true;
        private const string shaderName = "ChiseNote/MoreDecal";

        protected override void LoadCustomProperties(MaterialProperty[] props, Material material)
        {
            isCustomShader = true;
            ReplaceToCustomShaders();
            isShowRenderMode = !material.shader.name.Contains("Optional");
            decalCount = FindProperty("_DecalCount", props);
            for(int i = 0; i < MAX_DECALS; i++)
            {
                int num = i + 1;
                decalColor[i] = FindProperty($"_Decal{num}Color", props);
                decalTex[i] = FindProperty($"_Decal{num}Tex", props);
                decalTex_ST[i] = FindProperty($"_Decal{num}Tex_ST", props);
                decalTexAngle[i] = FindProperty($"_Decal{num}TexAngle", props);
                decalUseAudioLink[i] = FindProperty($"_Decal{num}UseAudioLink", props, false);
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

        private bool FoldoutWithToggle(string label, bool foldout, MaterialProperty toggleProp, float indentMultiplier = 1f)
        {
            Rect rect = EditorGUILayout.GetControlRect(true, 22);
            float indent = EditorGUI.indentLevel * 15f * indentMultiplier;
            Rect boxRect = new Rect(rect.x + indent, rect.y, rect.width - indent, rect.height);
            GUI.Box(boxRect, "", customBox);
            
            Rect innerRect = new Rect(boxRect.x + 4, boxRect.y + 2, boxRect.width - 8, boxRect.height - 4);
            
            const float arrowWidth = 12f;
            const float checkboxWidth = 18f;
            const float spacing = 0f;
            const float visualOffset = 2.0f;
            const float arrowOffset = 12.0f;
            float effectiveCheckboxWidth = (toggleProp != null) ? checkboxWidth : 0f;

            Rect arrowRect = new Rect(innerRect.x + arrowOffset, innerRect.y, arrowWidth, innerRect.height);
            Rect drawRect = new Rect(innerRect.x + arrowWidth + visualOffset, innerRect.y, effectiveCheckboxWidth, effectiveCheckboxWidth);
            Rect labelRect = new Rect(drawRect.xMax + spacing, innerRect.y, innerRect.width - (arrowWidth + effectiveCheckboxWidth + spacing + visualOffset), innerRect.height);
            
            if(toggleProp != null)
            {
                bool toggleValue = toggleProp.floatValue == 1.0f;
                if (Event.current.type == EventType.MouseDown && drawRect.Contains(Event.current.mousePosition))
                {
                    toggleProp.floatValue = toggleValue ? 0.0f : 1.0f;
                    Event.current.Use();
                    GUI.changed = true;
                    toggleValue = !toggleValue;
                }
                int originalIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                EditorGUI.Toggle(drawRect, toggleValue);
                EditorGUI.indentLevel = originalIndent;
            }
            int originalIndentForFoldout = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            bool newFoldout = EditorGUI.Foldout(arrowRect, foldout, "", true, EditorStyles.foldout);
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);
            EditorGUI.indentLevel = originalIndentForFoldout;
            
            return newFoldout;
        }

        protected override void DrawCustomProperties(Material material)
        {
            isShowDecalCountControl = Foldout("Decal Count Control", "Decal Count Control", isShowDecalCountControl);
            if(isShowDecalCountControl)
            {
                EditorGUILayout.BeginVertical(boxOuter);
                EditorGUILayout.BeginVertical(boxInnerHalf);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                    int maxVal = decalCountLock ? MAX_DECALS : 100;
                    string rangeLabel = decalCountLock ? $"Add Decal (0-{MAX_DECALS})" : "Add Decal (0-100)";
                    EditorGUILayout.LabelField(rangeLabel, GUILayout.Width(140));
                    int currentVal = Mathf.RoundToInt(decalCount.floatValue);
                    int newVal = EditorGUILayout.IntSlider(currentVal, 0, maxVal);
                    decalCountLock = EditorGUILayout.Toggle(decalCountLock, GUILayout.Width(18));

                EditorGUILayout.EndHorizontal();

                if(EditorGUI.EndChangeCheck())
                {
                    decalCount.floatValue = Mathf.Clamp(newVal, 0, maxVal);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
            int currentDecalCount = Mathf.RoundToInt(decalCount.floatValue);
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

                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Texture"), decalTex[index], decalColor[index]);
                lilEditorGUI.DrawColorAsAlpha(decalColor[index]);
                lilEditorGUI.LocalizedProperty(m_MaterialEditor, decalTexIsMSDF[index]);
                lilEditorGUI.LocalizedProperty(m_MaterialEditor, decalTex_UVMode[index]);
                lilEditorGUI.LocalizedProperty(m_MaterialEditor, decalBlendMode[index]);
                
                DrawLine();
                EditorGUI.indentLevel++;
                isShowDecalTransform[index] = FoldoutWithToggle("Transform", isShowDecalTransform[index], null, 0f);
                if(isShowDecalTransform[index])
                {
                    EditorGUILayout.BeginVertical(boxInner);
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
                    m_MaterialEditor.ShaderProperty(decalSyncScale[index], "Sync Scale");
                    
                    if(decalSyncScale[index].floatValue == 1.0f)
                    {
                        float syncScale = scaleX;
                        syncScale = EditorGUILayout.Slider("X / Y Scale", syncScale, -1.0f, 1.0f);
                        scaleX = syncScale;
                        scaleY = syncScale;
                    }
                    else
                    {
                        scaleX = EditorGUILayout.Slider(Event.current.alt ? decalTex[index].name + "_ST.x" : lilLanguageManager.GetLoc("sScaleX"), scaleX, -1.0f, 1.0f);
                        scaleY = EditorGUILayout.Slider(Event.current.alt ? decalTex[index].name + "_ST.y" : lilLanguageManager.GetLoc("sScaleY"), scaleY, -1.0f, 1.0f);
                    }
                    if(EditorGUI.EndChangeCheck())
                    {
                        if(scaleX == 0.0f) scaleX = 0.000001f;
                        if(scaleY == 0.0f) scaleY = 0.000001f;

                        scaleX = 1.0f / scaleX;
                        scaleY = 1.0f / scaleY;
                        posX = (-(posX - 0.5f) * scaleX) + 0.5f;
                        posY = (-(posY - 0.5f) * scaleY) + 0.5f;

                        decalTex[index].textureScaleAndOffset = new Vector4(scaleX, scaleY, posX, posY);
                    }
                    
                    lilEditorGUI.LocalizedProperty(m_MaterialEditor, decalTexAngle[index]);
                    if(decalUseAudioLink[index] != null)
                    {
                        EditorGUILayout.Space(2);
                        isShowDecalAudioLink[index] = FoldoutWithToggle("AudioLink", isShowDecalAudioLink[index], decalUseAudioLink[index]);
                        
                        if(isShowDecalAudioLink[index])
                        {
                            EditorGUI.indentLevel++;

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
                                Vector4 scaleVec = decalAudioLinkScale[index].vectorValue;
                                EditorGUI.BeginChangeCheck();
                                var positionVec2X = EditorGUILayout.GetControlRect();
                                float labelWidth = EditorGUIUtility.labelWidth;
                                var labelRectX = new Rect(positionVec2X.x, positionVec2X.y, labelWidth, positionVec2X.height);
                                EditorGUI.PrefixLabel(labelRectX, new GUIContent(Event.current.alt ? decalAudioLinkScale[index].name + ".xz" : "Scale X (min / max)"));

                                var vecRectX = new Rect(positionVec2X.x + labelWidth, positionVec2X.y, positionVec2X.width - labelWidth, positionVec2X.height);
                                Vector2 audioScaleX = new Vector2(scaleVec.x, scaleVec.z);
                                int indentBufX = EditorGUI.indentLevel;
                                EditorGUI.indentLevel = 0;
                                GUIContent[] scaleXLabels = new GUIContent[] { new GUIContent("X Min"), new GUIContent("X Max") };
                                float[] scaleXVals = new float[] { audioScaleX.x, audioScaleX.y };
                                EditorGUI.MultiFloatField(vecRectX, GUIContent.none, scaleXLabels, scaleXVals);
                                audioScaleX = new Vector2(scaleXVals[0], scaleXVals[1]);
                                EditorGUI.indentLevel = indentBufX;
                                var positionVec2Y = EditorGUILayout.GetControlRect();
                                var labelRectY = new Rect(positionVec2Y.x, positionVec2Y.y, labelWidth, positionVec2Y.height);
                                EditorGUI.PrefixLabel(labelRectY, new GUIContent(Event.current.alt ? decalAudioLinkScale[index].name + ".yw" : "Scale Y (min / max)"));

                                var vecRectY = new Rect(positionVec2Y.x + labelWidth, positionVec2Y.y, positionVec2Y.width - labelWidth, positionVec2Y.height);
                                Vector2 audioScaleY = new Vector2(scaleVec.y, scaleVec.w);
                                int indentBufY = EditorGUI.indentLevel;
                                EditorGUI.indentLevel = 0;
                                GUIContent[] scaleYLabels = new GUIContent[] { new GUIContent("Y Min"), new GUIContent("Y Max") };
                                float[] scaleYVals = new float[] { audioScaleY.x, audioScaleY.y };
                                EditorGUI.MultiFloatField(vecRectY, GUIContent.none, scaleYLabels, scaleYVals);
                                audioScaleY = new Vector2(scaleYVals[0], scaleYVals[1]);
                                EditorGUI.indentLevel = indentBufY;

                                if(EditorGUI.EndChangeCheck())
                                {
                                    decalAudioLinkScale[index].vectorValue = new Vector4(audioScaleX.x, audioScaleY.x, audioScaleX.y, audioScaleY.y);
                                }
                            }
                            DrawLine();
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
                            GUIContent[] sideSubLabels = new GUIContent[] { new GUIContent("L"), new GUIContent("R"), new GUIContent("D"), new GUIContent("U") };
                            Vector4 minVec = Vector4.zero;
                            Vector4 maxVec = Vector4.zero;
                            if(decalAudioLinkSideMonMin[index] != null) minVec = decalAudioLinkSideMonMin[index].vectorValue;
                            if(decalAudioLinkSideMonMax[index] != null) maxVec = decalAudioLinkSideMonMax[index].vectorValue;

                            EditorGUI.BeginChangeCheck();
                            var posMin = EditorGUILayout.GetControlRect();
                            var labRectMin = new Rect(posMin.x, posMin.y, EditorGUIUtility.labelWidth, posMin.height);
                            EditorGUI.PrefixLabel(labRectMin, new GUIContent("Side Mod Min"));
                            var vRectMin = new Rect(posMin.x + EditorGUIUtility.labelWidth, posMin.y, posMin.width - EditorGUIUtility.labelWidth, posMin.height);
                            float[] minVals = new float[] { minVec.y, minVec.x, minVec.w, minVec.z };
                            int indentBufMin = EditorGUI.indentLevel;
                            EditorGUI.indentLevel = 0;
                            EditorGUI.MultiFloatField(vRectMin, GUIContent.none, sideSubLabels, minVals);
                            EditorGUI.indentLevel = indentBufMin;
                            var posMax = EditorGUILayout.GetControlRect();
                            var labRectMax = new Rect(posMax.x, posMax.y, EditorGUIUtility.labelWidth, posMax.height);
                            EditorGUI.PrefixLabel(labRectMax, new GUIContent("Side Mod Max"));
                            var vRectMax = new Rect(posMax.x + EditorGUIUtility.labelWidth, posMax.y, posMax.width - EditorGUIUtility.labelWidth, posMax.height);
                            float[] maxVals = new float[] { maxVec.y, maxVec.x, maxVec.w, maxVec.z };
                            int indentBufMax = EditorGUI.indentLevel;
                            EditorGUI.indentLevel = 0;
                            EditorGUI.MultiFloatField(vRectMax, GUIContent.none, sideSubLabels, maxVals);
                            EditorGUI.indentLevel = indentBufMax;

                            if(EditorGUI.EndChangeCheck())
                            {
                                if(decalAudioLinkSideMonMin[index] != null)
                                    decalAudioLinkSideMonMin[index].vectorValue = new Vector4(minVals[1], minVals[0], minVals[3], minVals[2]);
                                if(decalAudioLinkSideMonMax[index] != null)
                                    decalAudioLinkSideMonMax[index].vectorValue = new Vector4(maxVals[1], maxVals[0], maxVals[3], maxVals[2]);
                            }
                            DrawLine();
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
                                Vector4 rotVec = decalAudioLinkRotation[index].vectorValue;
                                EditorGUI.BeginChangeCheck();

                                var posRot = EditorGUILayout.GetControlRect();
                                float lwRot = EditorGUIUtility.labelWidth;
                                var labRectRot = new Rect(posRot.x, posRot.y, lwRot, posRot.height);
                                EditorGUI.PrefixLabel(labRectRot, new GUIContent(Event.current.alt ? decalAudioLinkRotation[index].name + ".xy" : "Rotation (min / max)"));

                                var vRectRot = new Rect(posRot.x + lwRot, posRot.y, posRot.width - lwRot, posRot.height);
                                GUIContent[] rotLabels = new GUIContent[] { new GUIContent("R Min"), new GUIContent("R Max") };
                                float[] rotValsArr = new float[] { rotVec.x, rotVec.y };
                                int indentBufRot = EditorGUI.indentLevel;
                                EditorGUI.indentLevel = 0;
                                EditorGUI.MultiFloatField(vRectRot, GUIContent.none, rotLabels, rotValsArr);
                                EditorGUI.indentLevel = indentBufRot;

                                if(EditorGUI.EndChangeCheck())
                                {
                                    decalAudioLinkRotation[index].vectorValue = new Vector4(rotValsArr[0], rotValsArr[1], rotVec.z, rotVec.w);
                                }
                            }
                            DrawLine();
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
                                string[] chronoOptions = new string[] {
                                    "Stop when band high (immediate)",
                                    "Stop when band high (smooth)",
                                    "Reverse when band high (immediate)",
                                    "Reverse when band high (smooth)",
                                    "Start on band high (immediate)",
                                    "Start on band high (smooth)"
                                };
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

                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
                
                DrawLine();
                EditorGUI.indentLevel++;
                isShowDecalAnimation[index] = FoldoutWithToggle("Animation", isShowDecalAnimation[index], decalUseAnimation[index], 0f);
                
                if(isShowDecalAnimation[index])
                {
                    EditorGUILayout.BeginVertical(boxInner);
                    Vector4 animVec = decalAnimation[index].vectorValue;
                    int loopX = (int)animVec.x;
                    int loopY = (int)animVec.y;
                    int frames = (int)animVec.z;
                    float speed = animVec.w;
                    
                    EditorGUI.BeginChangeCheck();
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
                    EditorGUILayout.EndVertical();
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