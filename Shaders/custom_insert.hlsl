//----------------------------------------------------------------------------------------------------------------------
// Decal Processing
//----------------------------------------------------------------------------------------------------------------------

// Helper function to rotate 2D vector
float2 lilMoreDecalRotate2D(float2 v, float angle)
{
    float s, c;
    sincos(angle, s, c);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

// Inverse affine transform: applies translate, rotate, scale in reverse order
// This allows rotation center to follow position while preserving aspect ratio
float2 lilMoreDecalInvAffineTransform(float2 uv, float2 translate, float angle, float2 scale)
{
    scale = max(scale, float2(0.001, 0.001));
    return lilMoreDecalRotate2D(uv - 0.5 - translate, angle) / scale + 0.5;
}

// Helper function to blend colors based on blend mode
float3 lilMoreDecalBlendColor(float3 baseColor, float3 decalColor, float alpha, uint blendMode)
{
    // 0:Normal 1:Add 2:Screen 3:Multiply
    if(blendMode == 0) return lerp(baseColor, decalColor, alpha);
    if(blendMode == 1) return baseColor + decalColor * alpha;
    if(blendMode == 2) return lerp(baseColor, 1.0 - (1.0 - baseColor) * (1.0 - decalColor), alpha);
    if(blendMode == 3) return lerp(baseColor, baseColor * decalColor, alpha);
    return lerp(baseColor, decalColor, alpha);
}

// Custom decal UV calculation using inverse affine transform
// This approach allows rotation center to follow Position X/Y while preserving aspect ratio
float2 lilMoreDecalCalcUV(
    float2 uv,
    float4 uv_ST,
    float angle,
    bool shouldCopy,
    bool shouldFlipCopy)
{
    float2 outUV = uv;

    // Copy (same as lilCalcDecalUV)
    if(shouldCopy) outUV.x = abs(outUV.x - 0.5) + 0.5;

    // Convert uv_ST to translate and scale
    // uv_ST.xy = scale (inverted: 1/displayScale)
    // uv_ST.zw = offset (calculated: -posX*scaleX+0.5)
    // We need to extract: translate (position) and scale (1/uv_ST.xy)
    float2 scale = float2(1.0, 1.0) / max(abs(uv_ST.xy), float2(0.001, 0.001));
    float2 translate = (float2(0.5, 0.5) - uv_ST.zw) / uv_ST.xy;
    
    // Apply inverse affine transform: translate → rotate → scale
    outUV = lilMoreDecalInvAffineTransform(outUV, translate, angle, scale);

    // Flip (same as lilCalcDecalUV)
    if(shouldFlipCopy && uv.x < 0.5) outUV.x = 1.0 - outUV.x;

    return outUV;
}

// Apply decal to color
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
    float4 decalAnimation
    LIL_SAMP_IN_FUNC(samp))
{
    // Calculate UV with custom rotation (around texture center)
    float2 decalUV = lilMoreDecalCalcUV(
        uv,
        decalTex_ST,
        decalTexAngle,
        decalShouldCopy,
        decalShouldFlipCopy);
    
    // Apply decal masking BEFORE atlas animation (always enabled)
    // Check if UV is in 0-1 range (with small tolerance based on normal view)
    float mask = saturate(0.5 - abs(decalUV.x - 0.5));
    mask *= saturate(0.5 - abs(decalUV.y - 0.5));
    mask = saturate(mask / clamp(fwidth(mask), 0.0001, saturate(fd.nv - 0.05)));
    
    // Apply atlas animation (sprite sheet) only if enabled
    if(decalUseAnimation)
    {
        float4 decalSubParam = float4(1.0, 1.0, 0.0, 1.0);
        decalUV = lilCalcAtlasAnimation(decalUV, decalAnimation, decalSubParam);
    }
    
    // Sample texture
    float4 decalSample = LIL_SAMPLE_2D(decalTex, samp, decalUV);
    
    // Apply MSDF if enabled
    if(decalTexIsMSDF) decalSample = float4(1.0, 1.0, 1.0, lilMSDF(decalSample.rgb));
    
    // Apply mask to alpha
    decalSample.a *= mask;
    
    float3 decalCol = decalSample.rgb * decalColor.rgb;
    float decalAlpha = decalSample.a * decalColor.a;
    
    if(decalAlpha > 0.001)
    {
        fd.col.rgb = lilMoreDecalBlendColor(fd.col.rgb, decalCol, decalAlpha, decalBlendMode);
    }
}

// Insert decal processing after Main3rd (similar to DecalHeartRate)
#if !defined(BEFORE_MAIN3RD)
    #define BEFORE_MAIN3RD \
        if(_DecalCount >= 1) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal1Tex_UVMode == 1) decalUV = fd.uv1; \
            if(_Decal1Tex_UVMode == 2) decalUV = fd.uv2; \
            if(_Decal1Tex_UVMode == 3) decalUV = fd.uv3; \
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
                _Decal1TexDecalAnimation \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 2) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal2Tex_UVMode == 1) decalUV = fd.uv1; \
            if(_Decal2Tex_UVMode == 2) decalUV = fd.uv2; \
            if(_Decal2Tex_UVMode == 3) decalUV = fd.uv3; \
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
                _Decal2TexDecalAnimation \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 3) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal3Tex_UVMode == 1) decalUV = fd.uv1; \
            if(_Decal3Tex_UVMode == 2) decalUV = fd.uv2; \
            if(_Decal3Tex_UVMode == 3) decalUV = fd.uv3; \
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
                _Decal3TexDecalAnimation \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 4) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal4Tex_UVMode == 1) decalUV = fd.uv1; \
            if(_Decal4Tex_UVMode == 2) decalUV = fd.uv2; \
            if(_Decal4Tex_UVMode == 3) decalUV = fd.uv3; \
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
                _Decal4TexDecalAnimation \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 5) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal5Tex_UVMode == 1) decalUV = fd.uv1; \
            if(_Decal5Tex_UVMode == 2) decalUV = fd.uv2; \
            if(_Decal5Tex_UVMode == 3) decalUV = fd.uv3; \
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
                _Decal5TexDecalAnimation \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 6) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal6Tex_UVMode == 1) decalUV = fd.uv1; \
            if(_Decal6Tex_UVMode == 2) decalUV = fd.uv2; \
            if(_Decal6Tex_UVMode == 3) decalUV = fd.uv3; \
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
                _Decal6TexDecalAnimation \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 7) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal7Tex_UVMode == 1) decalUV = fd.uv1; \
            if(_Decal7Tex_UVMode == 2) decalUV = fd.uv2; \
            if(_Decal7Tex_UVMode == 3) decalUV = fd.uv3; \
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
                _Decal7TexDecalAnimation \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 8) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal8Tex_UVMode == 1) decalUV = fd.uv1; \
            if(_Decal8Tex_UVMode == 2) decalUV = fd.uv2; \
            if(_Decal8Tex_UVMode == 3) decalUV = fd.uv3; \
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
                _Decal8TexDecalAnimation \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 9) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal9Tex_UVMode == 1) decalUV = fd.uv1; \
            if(_Decal9Tex_UVMode == 2) decalUV = fd.uv2; \
            if(_Decal9Tex_UVMode == 3) decalUV = fd.uv3; \
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
                _Decal9TexDecalAnimation \
                LIL_SAMP_IN(sampler_DecalTex)); \
        } \
        if(_DecalCount >= 10) \
        { \
            float2 decalUV = fd.uv0; \
            if(_Decal10Tex_UVMode == 1) decalUV = fd.uv1; \
            if(_Decal10Tex_UVMode == 2) decalUV = fd.uv2; \
            if(_Decal10Tex_UVMode == 3) decalUV = fd.uv3; \
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
                _Decal10TexDecalAnimation \
                LIL_SAMP_IN(sampler_DecalTex)); \
        }
#endif