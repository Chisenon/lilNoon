float2 lilMoreDecalRotate2D(float2 v, float angle)
{
    float s, c;
    sincos(angle, s, c);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float2 lilMoreDecalInvAffineTransform(float2 uv, float2 translate, float angle, float2 scale)
{
    scale.x = abs(scale.x) < 0.001 ? sign(scale.x) * 0.001 : scale.x;
    scale.y = abs(scale.y) < 0.001 ? sign(scale.y) * 0.001 : scale.y;
    return lilMoreDecalRotate2D(uv - 0.5 - translate, angle) / scale + 0.5;
}

float3 lilMoreDecalBlendColor(float3 baseColor, float3 decalColor, float alpha, uint blendMode)
{
    if(blendMode == 0) return lerp(baseColor, decalColor, alpha);
    if(blendMode == 1) return baseColor + decalColor * alpha;
    if(blendMode == 2) return lerp(baseColor, 1.0 - (1.0 - baseColor) * (1.0 - decalColor), alpha);
    if(blendMode == 3) return lerp(baseColor, baseColor * decalColor, alpha);
    return lerp(baseColor, decalColor, alpha);
}

float2 lilMoreDecalCalcUV(
    float2 uv,
    float4 uv_ST,
    float angle,
    bool shouldCopy,
    bool shouldFlipCopy)
{
    float2 outUV = uv;

    if(shouldCopy) outUV.x = abs(outUV.x - 0.5) + 0.5;

    float2 scale = float2(1.0, 1.0) / uv_ST.xy;
    float2 translate = (float2(0.5, 0.5) - uv_ST.zw) / uv_ST.xy;
    
    outUV = lilMoreDecalInvAffineTransform(outUV, translate, angle, scale);

    if(shouldFlipCopy && uv.x < 0.5) outUV.x = 1.0 - outUV.x;

    return outUV;
}

float lilMoreDecalSampleAudio(int band)
{
    #if defined(LIL_FEATURE_AUDIOLINK)
        if(!lilCheckAudioLink()) return 0.0;
        float bandY = (band * 0.25) + 0.125;
        bandY *= 0.0625;
        float2 audioUV = float2(0.0, bandY);
        float4 audioTex = LIL_SAMPLE_2D(_AudioTexture, lil_sampler_linear_clamp, audioUV);
        return saturate(audioTex.r);
    #else
        return 0.0;
    #endif
}

uint lilMoreDecalAudioLinkDecodeUInt(int px, int py)
{
    #if defined(LIL_FEATURE_AUDIOLINK)
        float2 uv = (float2(px, py) + 0.5) * _AudioTexture_TexelSize.xy;
        float4 s = LIL_SAMPLE_2D(_AudioTexture, lil_sampler_linear_clamp, uv);
        uint r = (uint)floor(s.r * 1023.0 + 0.5);
        uint g = (uint)floor(s.g * 1023.0 + 0.5);
        uint b = (uint)floor(s.b * 1023.0 + 0.5);
        uint a = (uint)floor(s.a * 1023.0 + 0.5);
        return r + (g << 10) + (b << 20) + (a << 30);
    #else
        return 0u;
    #endif
}

float lilMoreDecalSampleChronoRad(int offsetX, int band)
{
    #if defined(LIL_FEATURE_AUDIOLINK)
        if(!lilCheckAudioLink()) return 0.0;
        const int baseX = 16; // ALPASS_CHRONOTENSITY
        const int baseY = 28;
        int px = baseX + clamp(offsetX, 0, 7);
        int py = baseY + clamp(band, 0, 3);
        uint u = lilMoreDecalAudioLinkDecodeUInt(px, py);
        return (float(u) / 100000.0);
    #else
        return 0.0;
    #endif
}

void lilApplyDecal(
    inout lilFragData fd,
    float2 uv,
    float4 decalColor,
    TEXTURE2D(decalTex),
    float4 decalTex_ST,
    float decalTexAngle,
    bool decalShouldCopy,
    bool decalShouldFlipCopy,
    bool decalTexIsMSDF,
    uint decalBlendMode,
    bool decalUseAnimation,
    float4 decalAnimation,
    float decalAudioLinkScaleBand,
    float4 decalAudioLinkScale,
    float decalAudioLinkSideBand,
    float4 decalAudioLinkSideMonMin,
    float4 decalAudioLinkSideMonMax,
    float decalAudioLinkRotationBand,
    float2 decalAudioLinkRotation,
    float decalAudioLinkChronoRotationBand,
    float decalAudioLinkChronoMotionType,
    float decalAudioLinkChronoRotationSpeed
    LIL_SAMP_IN_FUNC(samp))
{
    float4 localTexST = decalTex_ST;
    #if defined(LIL_FEATURE_AUDIOLINK)
        if(any(decalAudioLinkScale != 0.0))
        {
            int band = (int)decalAudioLinkScaleBand;
            float audioVal = lilMoreDecalSampleAudio(band);
            float mulX = lerp(decalAudioLinkScale.x, decalAudioLinkScale.z, audioVal);
            float mulY = lerp(decalAudioLinkScale.y, decalAudioLinkScale.w, audioVal);
            if(!(mulX == 0.0 && mulY == 0.0))
            {
                mulX = (mulX <= 1e-5) ? 1.0 : mulX;
                mulY = (mulY <= 1e-5) ? 1.0 : mulY;
                float2 scaleRatio = float2(1.0 / mulX, 1.0 / mulY);
                localTexST.zw = localTexST.zw * scaleRatio + float2(0.5, 0.5) * (1.0 - scaleRatio);
                localTexST.xy = localTexST.xy / float2(mulX, mulY);
            }
        }
        
        if(any(decalAudioLinkSideMonMin != 0.0) || any(decalAudioLinkSideMonMax != 0.0))
        {
            int band = (int)decalAudioLinkSideBand;
            float audioVal = lilMoreDecalSampleAudio(band);
            float leftVal = lerp(decalAudioLinkSideMonMin.x, decalAudioLinkSideMonMax.x, audioVal);
            float rightVal = lerp(decalAudioLinkSideMonMin.y, decalAudioLinkSideMonMax.y, audioVal);
            float downVal = lerp(decalAudioLinkSideMonMin.z, decalAudioLinkSideMonMax.z, audioVal);
            float upVal = lerp(decalAudioLinkSideMonMin.w, decalAudioLinkSideMonMax.w, audioVal);
            float offsetX = rightVal - leftVal;
            float offsetY = upVal - downVal;
            localTexST.z += offsetX;
            localTexST.w += offsetY;
        }
        
        if(any(decalAudioLinkRotation != 0.0))
        {
            int band = (int)decalAudioLinkRotationBand;
            float audioVal = lilMoreDecalSampleAudio(band);
            float rotDeg = lerp(decalAudioLinkRotation.x, decalAudioLinkRotation.y, audioVal);
            const float DEG2RAD = 0.017453292519943295;
            float rotAngle = rotDeg * DEG2RAD;
            decalTexAngle += rotAngle;
        }
        
        if(decalAudioLinkChronoRotationSpeed != 0.0)
        {
            int band = (int)decalAudioLinkChronoRotationBand;
            int motionType = (int)decalAudioLinkChronoMotionType;
            float chronoRad = 0.0;
            #if defined(LIL_FEATURE_AUDIOLINK)
                chronoRad = lilMoreDecalSampleChronoRad(motionType, band);
            #endif

            const float CHRONO_SPEED_INPUT_SCALE = 0.001; // UI入力を0.001倍 (0.1 -> 0.0001)
            const float CHRONO_FALLBACK_SCALE = 0.1;

            if(chronoRad != 0.0)
            {
                const float DEG2RAD = 0.017453292519943295;
                float chronoTime = chronoRad;
                float deg = chronoTime * (decalAudioLinkChronoRotationSpeed * CHRONO_SPEED_INPUT_SCALE) * 360.0;
                decalTexAngle += deg * DEG2RAD;
            }
            else
            {
                float audioVal = lilMoreDecalSampleAudio(band);
                float deltaSec = unity_DeltaTime.w;
                float baseRotation = (decalAudioLinkChronoRotationSpeed * CHRONO_SPEED_INPUT_SCALE) * deltaSec * CHRONO_FALLBACK_SCALE;
                float totalRotation = baseRotation;
                const float TH = 0.5;
                if(motionType == 0)
                {
                    float active = (audioVal >= TH) ? 0.0 : 1.0;
                    totalRotation = baseRotation * active;
                }
                else if(motionType == 1)
                {
                    float smoothVal = smoothstep(TH - 0.1, TH, audioVal);
                    float factor = 1.0 - smoothVal;
                    totalRotation = baseRotation * factor;
                }
                else if(motionType == 2)
                {
                    float dir = (audioVal >= TH) ? -1.0 : 1.0;
                    totalRotation = baseRotation * dir;
                }
                else if(motionType == 3)
                {
                    float smoothVal = smoothstep(TH - 0.1, TH, audioVal);
                    float dir = lerp(1.0, -1.0, smoothVal);
                    totalRotation = baseRotation * dir;
                }
                else if(motionType == 4)
                {
                    float active = (audioVal >= TH) ? 1.0 : 0.0;
                    totalRotation = baseRotation * active;
                }
                else if(motionType == 5)
                {
                    float smoothVal = smoothstep(TH - 0.1, TH, audioVal);
                    totalRotation = baseRotation * smoothVal;
                }

                const float DEG2RAD = 0.017453292519943295;
                decalTexAngle += totalRotation * DEG2RAD;
            }
        }
    #endif

    float2 decalUV = lilMoreDecalCalcUV(
        uv,
        localTexST,
        decalTexAngle,
        decalShouldCopy,
        decalShouldFlipCopy);
    
    float mask = saturate(0.5 - abs(decalUV.x - 0.5));
    mask *= saturate(0.5 - abs(decalUV.y - 0.5));
    mask = saturate(mask / clamp(fwidth(mask), 0.0001, saturate(fd.nv - 0.05)));
    
    if(decalUseAnimation)
    {
        float4 decalSubParam = float4(1.0, 1.0, 0.0, 1.0);
        decalUV = lilCalcAtlasAnimation(decalUV, decalAnimation, decalSubParam);
    }
    
    float4 decalSample = LIL_SAMPLE_2D(decalTex, samp, decalUV);
    
    if(decalTexIsMSDF) decalSample = float4(1.0, 1.0, 1.0, lilMSDF(decalSample.rgb));
    
    decalSample.a *= mask;
    
    float3 decalCol = decalSample.rgb * decalColor.rgb;
    float decalAlpha = decalSample.a * decalColor.a;
    
    if(decalAlpha > 0.001)
    {
        fd.col.rgb = lilMoreDecalBlendColor(fd.col.rgb, decalCol, decalAlpha, decalBlendMode);
    }
}

#if !defined(BEFORE_MAIN3RD)
    #define BEFORE_MAIN3RD \
        if(_DecalCount >= 1) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal1Tex_UVMode == 1) decalUV = fd.uv1; \
            else if(_Decal1Tex_UVMode == 2) decalUV = fd.uv2; \
            else if(_Decal1Tex_UVMode == 3) decalUV = fd.uv3; \
            lilApplyDecal( \
                fd, \
                decalUV, \
                _Decal1Color, \
                _Decal1Tex, \
                _Decal1Tex_ST, \
                _Decal1TexAngle, \
                _Decal1ShouldCopy, \
                _Decal1ShouldFlipCopy, \
                _Decal1TexIsMSDF, \
                _Decal1BlendMode, \
                _Decal1UseAnimation, \
                _Decal1TexDecalAnimation, \
                _AudioLinkDecal1ScaleBand, \
                _AudioLinkDecal1Scale, \
                _AudioLinkDecal1SideBand, \
                _AudioLinkDecal1SideMonMin, \
                _AudioLinkDecal1SideMonMax, \
                _AudioLinkDecal1RotationBand, \
                _AudioLinkDecal1Rotation, \
                _AudioLinkDecal1ChronoRotationBand, \
                _AudioLinkDecal1ChronoMotionType, \
                _AudioLinkDecal1ChronoRotationSpeed \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 2) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal2Tex_UVMode == 1) decalUV = fd.uv1; \
            else if(_Decal2Tex_UVMode == 2) decalUV = fd.uv2; \
            else if(_Decal2Tex_UVMode == 3) decalUV = fd.uv3; \
            lilApplyDecal( \
                fd, \
                decalUV, \
                _Decal2Color, \
                _Decal2Tex, \
                _Decal2Tex_ST, \
                _Decal2TexAngle, \
                _Decal2ShouldCopy, \
                _Decal2ShouldFlipCopy, \
                _Decal2TexIsMSDF, \
                _Decal2BlendMode, \
                _Decal2UseAnimation, \
                _Decal2TexDecalAnimation, \
                _AudioLinkDecal2ScaleBand, \
                _AudioLinkDecal2Scale, \
                _AudioLinkDecal2SideBand, \
                _AudioLinkDecal2SideMonMin, \
                _AudioLinkDecal2SideMonMax, \
                _AudioLinkDecal2RotationBand, \
                _AudioLinkDecal2Rotation, \
                _AudioLinkDecal2ChronoRotationBand, \
                _AudioLinkDecal2ChronoMotionType, \
                _AudioLinkDecal2ChronoRotationSpeed \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 3) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal3Tex_UVMode == 1) decalUV = fd.uv1; \
            else if(_Decal3Tex_UVMode == 2) decalUV = fd.uv2; \
            else if(_Decal3Tex_UVMode == 3) decalUV = fd.uv3; \
            lilApplyDecal( \
                fd, \
                decalUV, \
                _Decal3Color, \
                _Decal3Tex, \
                _Decal3Tex_ST, \
                _Decal3TexAngle, \
                _Decal3ShouldCopy, \
                _Decal3ShouldFlipCopy, \
                _Decal3TexIsMSDF, \
                _Decal3BlendMode, \
                _Decal3UseAnimation, \
                _Decal3TexDecalAnimation, \
                _AudioLinkDecal3ScaleBand, \
                _AudioLinkDecal3Scale, \
                _AudioLinkDecal3SideBand, \
                _AudioLinkDecal3SideMonMin, \
                _AudioLinkDecal3SideMonMax, \
                _AudioLinkDecal3RotationBand, \
                _AudioLinkDecal3Rotation, \
                _AudioLinkDecal3ChronoRotationBand, \
                _AudioLinkDecal3ChronoMotionType, \
                _AudioLinkDecal3ChronoRotationSpeed \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 4) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal4Tex_UVMode == 1) decalUV = fd.uv1; \
            else if(_Decal4Tex_UVMode == 2) decalUV = fd.uv2; \
            else if(_Decal4Tex_UVMode == 3) decalUV = fd.uv3; \
            lilApplyDecal( \
                fd, \
                decalUV, \
                _Decal4Color, \
                _Decal4Tex, \
                _Decal4Tex_ST, \
                _Decal4TexAngle, \
                _Decal4ShouldCopy, \
                _Decal4ShouldFlipCopy, \
                _Decal4TexIsMSDF, \
                _Decal4BlendMode, \
                _Decal4UseAnimation, \
                _Decal4TexDecalAnimation, \
                _AudioLinkDecal4ScaleBand, \
                _AudioLinkDecal4Scale, \
                _AudioLinkDecal4SideBand, \
                _AudioLinkDecal4SideMonMin, \
                _AudioLinkDecal4SideMonMax, \
                _AudioLinkDecal4RotationBand, \
                _AudioLinkDecal4Rotation, \
                _AudioLinkDecal4ChronoRotationBand, \
                _AudioLinkDecal4ChronoMotionType, \
                _AudioLinkDecal4ChronoRotationSpeed \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 5) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal5Tex_UVMode == 1) decalUV = fd.uv1; \
            else if(_Decal5Tex_UVMode == 2) decalUV = fd.uv2; \
            else if(_Decal5Tex_UVMode == 3) decalUV = fd.uv3; \
            lilApplyDecal( \
                fd, \
                decalUV, \
                _Decal5Color, \
                _Decal5Tex, \
                _Decal5Tex_ST, \
                _Decal5TexAngle, \
                _Decal5ShouldCopy, \
                _Decal5ShouldFlipCopy, \
                _Decal5TexIsMSDF, \
                _Decal5BlendMode, \
                _Decal5UseAnimation, \
                _Decal5TexDecalAnimation, \
                _AudioLinkDecal5ScaleBand, \
                _AudioLinkDecal5Scale, \
                _AudioLinkDecal5SideBand, \
                _AudioLinkDecal5SideMonMin, \
                _AudioLinkDecal5SideMonMax, \
                _AudioLinkDecal5RotationBand, \
                _AudioLinkDecal5Rotation, \
                _AudioLinkDecal5ChronoRotationBand, \
                _AudioLinkDecal5ChronoMotionType, \
                _AudioLinkDecal5ChronoRotationSpeed \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 6) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal6Tex_UVMode == 1) decalUV = fd.uv1; \
            else if(_Decal6Tex_UVMode == 2) decalUV = fd.uv2; \
            else if(_Decal6Tex_UVMode == 3) decalUV = fd.uv3; \
            lilApplyDecal( \
                fd, \
                decalUV, \
                _Decal6Color, \
                _Decal6Tex, \
                _Decal6Tex_ST, \
                _Decal6TexAngle, \
                _Decal6ShouldCopy, \
                _Decal6ShouldFlipCopy, \
                _Decal6TexIsMSDF, \
                _Decal6BlendMode, \
                _Decal6UseAnimation, \
                _Decal6TexDecalAnimation, \
                _AudioLinkDecal6ScaleBand, \
                _AudioLinkDecal6Scale, \
                _AudioLinkDecal6SideBand, \
                _AudioLinkDecal6SideMonMin, \
                _AudioLinkDecal6SideMonMax, \
                _AudioLinkDecal6RotationBand, \
                _AudioLinkDecal6Rotation, \
                _AudioLinkDecal6ChronoRotationBand, \
                _AudioLinkDecal6ChronoMotionType, \
                _AudioLinkDecal6ChronoRotationSpeed \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 7) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal7Tex_UVMode == 1) decalUV = fd.uv1; \
            else if(_Decal7Tex_UVMode == 2) decalUV = fd.uv2; \
            else if(_Decal7Tex_UVMode == 3) decalUV = fd.uv3; \
            lilApplyDecal( \
                fd, \
                decalUV, \
                _Decal7Color, \
                _Decal7Tex, \
                _Decal7Tex_ST, \
                _Decal7TexAngle, \
                _Decal7ShouldCopy, \
                _Decal7ShouldFlipCopy, \
                _Decal7TexIsMSDF, \
                _Decal7BlendMode, \
                _Decal7UseAnimation, \
                _Decal7TexDecalAnimation, \
                _AudioLinkDecal7ScaleBand, \
                _AudioLinkDecal7Scale, \
                _AudioLinkDecal7SideBand, \
                _AudioLinkDecal7SideMonMin, \
                _AudioLinkDecal7SideMonMax, \
                _AudioLinkDecal7RotationBand, \
                _AudioLinkDecal7Rotation, \
                _AudioLinkDecal7ChronoRotationBand, \
                _AudioLinkDecal7ChronoMotionType, \
                _AudioLinkDecal7ChronoRotationSpeed \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 8) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal8Tex_UVMode == 1) decalUV = fd.uv1; \
            else if(_Decal8Tex_UVMode == 2) decalUV = fd.uv2; \
            else if(_Decal8Tex_UVMode == 3) decalUV = fd.uv3; \
            lilApplyDecal( \
                fd, \
                decalUV, \
                _Decal8Color, \
                _Decal8Tex, \
                _Decal8Tex_ST, \
                _Decal8TexAngle, \
                _Decal8ShouldCopy, \
                _Decal8ShouldFlipCopy, \
                _Decal8TexIsMSDF, \
                _Decal8BlendMode, \
                _Decal8UseAnimation, \
                _Decal8TexDecalAnimation, \
                _AudioLinkDecal8ScaleBand, \
                _AudioLinkDecal8Scale, \
                _AudioLinkDecal8SideBand, \
                _AudioLinkDecal8SideMonMin, \
                _AudioLinkDecal8SideMonMax, \
                _AudioLinkDecal8RotationBand, \
                _AudioLinkDecal8Rotation, \
                _AudioLinkDecal8ChronoRotationBand, \
                _AudioLinkDecal8ChronoMotionType, \
                _AudioLinkDecal8ChronoRotationSpeed \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 9) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal9Tex_UVMode == 1) decalUV = fd.uv1; \
            else if(_Decal9Tex_UVMode == 2) decalUV = fd.uv2; \
            else if(_Decal9Tex_UVMode == 3) decalUV = fd.uv3; \
            lilApplyDecal( \
                fd, \
                decalUV, \
                _Decal9Color, \
                _Decal9Tex, \
                _Decal9Tex_ST, \
                _Decal9TexAngle, \
                _Decal9ShouldCopy, \
                _Decal9ShouldFlipCopy, \
                _Decal9TexIsMSDF, \
                _Decal9BlendMode, \
                _Decal9UseAnimation, \
                _Decal9TexDecalAnimation, \
                _AudioLinkDecal9ScaleBand, \
                _AudioLinkDecal9Scale, \
                _AudioLinkDecal9SideBand, \
                _AudioLinkDecal9SideMonMin, \
                _AudioLinkDecal9SideMonMax, \
                _AudioLinkDecal9RotationBand, \
                _AudioLinkDecal9Rotation, \
                _AudioLinkDecal9ChronoRotationBand, \
                _AudioLinkDecal9ChronoMotionType, \
                _AudioLinkDecal9ChronoRotationSpeed \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 10) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal10Tex_UVMode == 1) decalUV = fd.uv1; \
            else if(_Decal10Tex_UVMode == 2) decalUV = fd.uv2; \
            else if(_Decal10Tex_UVMode == 3) decalUV = fd.uv3; \
            lilApplyDecal( \
                fd, \
                decalUV, \
                _Decal10Color, \
                _Decal10Tex, \
                _Decal10Tex_ST, \
                _Decal10TexAngle, \
                _Decal10ShouldCopy, \
                _Decal10ShouldFlipCopy, \
                _Decal10TexIsMSDF, \
                _Decal10BlendMode, \
                _Decal10UseAnimation, \
                _Decal10TexDecalAnimation, \
                _AudioLinkDecal10ScaleBand, \
                _AudioLinkDecal10Scale, \
                _AudioLinkDecal10SideBand, \
                _AudioLinkDecal10SideMonMin, \
                _AudioLinkDecal10SideMonMax, \
                _AudioLinkDecal10RotationBand, \
                _AudioLinkDecal10Rotation, \
                _AudioLinkDecal10ChronoRotationBand, \
                _AudioLinkDecal10ChronoMotionType, \
                _AudioLinkDecal10ChronoRotationSpeed \
                LIL_SAMP_IN(sampler_DecalTex)); \
        }
#endif