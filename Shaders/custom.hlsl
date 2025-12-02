//----------------------------------------------------------------------------------------------------------------------
// Macro

// Custom variables
#define LIL_CUSTOM_PROPERTIES \
    float _DecalCount; \
    float4 _Decal1Color; \
    float4 _Decal1Tex_ST; \
    float _Decal1TexAngle; \
    lilBool _Decal1ShouldCopy; \
    lilBool _Decal1ShouldFlipCopy; \
    uint _Decal1BlendMode; \
    uint _Decal1Tex_UVMode; \
    float4 _Decal2Color; \
    float4 _Decal2Tex_ST; \
    float _Decal2TexAngle; \
    lilBool _Decal2ShouldCopy; \
    lilBool _Decal2ShouldFlipCopy; \
    uint _Decal2BlendMode; \
    uint _Decal2Tex_UVMode; \
    float4 _Decal3Color; \
    float4 _Decal3Tex_ST; \
    float _Decal3TexAngle; \
    lilBool _Decal3ShouldCopy; \
    lilBool _Decal3ShouldFlipCopy; \
    uint _Decal3BlendMode; \
    uint _Decal3Tex_UVMode; \
    float4 _Decal4Color; \
    float4 _Decal4Tex_ST; \
    float _Decal4TexAngle; \
    lilBool _Decal4ShouldCopy; \
    lilBool _Decal4ShouldFlipCopy; \
    uint _Decal4BlendMode; \
    uint _Decal4Tex_UVMode; \
    float4 _Decal5Color; \
    float4 _Decal5Tex_ST; \
    float _Decal5TexAngle; \
    lilBool _Decal5ShouldCopy; \
    lilBool _Decal5ShouldFlipCopy; \
    uint _Decal5BlendMode; \
    uint _Decal5Tex_UVMode; \
    float4 _Decal6Color; \
    float4 _Decal6Tex_ST; \
    float _Decal6TexAngle; \
    lilBool _Decal6ShouldCopy; \
    lilBool _Decal6ShouldFlipCopy; \
    uint _Decal6BlendMode; \
    uint _Decal6Tex_UVMode; \
    float4 _Decal7Color; \
    float4 _Decal7Tex_ST; \
    float _Decal7TexAngle; \
    lilBool _Decal7ShouldCopy; \
    lilBool _Decal7ShouldFlipCopy; \
    uint _Decal7BlendMode; \
    uint _Decal7Tex_UVMode; \
    float4 _Decal8Color; \
    float4 _Decal8Tex_ST; \
    float _Decal8TexAngle; \
    lilBool _Decal8ShouldCopy; \
    lilBool _Decal8ShouldFlipCopy; \
    uint _Decal8BlendMode; \
    uint _Decal8Tex_UVMode; \
    float4 _Decal9Color; \
    float4 _Decal9Tex_ST; \
    float _Decal9TexAngle; \
    lilBool _Decal9ShouldCopy; \
    lilBool _Decal9ShouldFlipCopy; \
    uint _Decal9BlendMode; \
    uint _Decal9Tex_UVMode; \
    float4 _Decal10Color; \
    float4 _Decal10Tex_ST; \
    float _Decal10TexAngle; \
    lilBool _Decal10ShouldCopy; \
    lilBool _Decal10ShouldFlipCopy; \
    uint _Decal10BlendMode; \
    uint _Decal10Tex_UVMode; \
    lilBool _Decal1TexIsMSDF; \
    lilBool _Decal2TexIsMSDF; \
    lilBool _Decal3TexIsMSDF; \
    lilBool _Decal4TexIsMSDF; \
    lilBool _Decal5TexIsMSDF; \
    lilBool _Decal6TexIsMSDF; \
    lilBool _Decal7TexIsMSDF; \
    lilBool _Decal8TexIsMSDF; \
    lilBool _Decal9TexIsMSDF; \
    lilBool _Decal10TexIsMSDF; \
    lilBool _Decal1UseAnimation; \
    lilBool _Decal2UseAnimation; \
    lilBool _Decal3UseAnimation; \
    lilBool _Decal4UseAnimation; \
    lilBool _Decal5UseAnimation; \
    lilBool _Decal6UseAnimation; \
    lilBool _Decal7UseAnimation; \
    lilBool _Decal8UseAnimation; \
    lilBool _Decal9UseAnimation; \
    lilBool _Decal10UseAnimation; \
    float4 _Decal1TexDecalAnimation; \
    float4 _Decal2TexDecalAnimation; \
    float4 _Decal3TexDecalAnimation; \
    float4 _Decal4TexDecalAnimation; \
    float4 _Decal5TexDecalAnimation; \
    float4 _Decal6TexDecalAnimation; \
    float4 _Decal7TexDecalAnimation; \
    float4 _Decal8TexDecalAnimation; \
    float4 _Decal9TexDecalAnimation; \
    float4 _Decal10TexDecalAnimation;

// Custom textures
#define LIL_CUSTOM_TEXTURES \
    TEXTURE2D(_Decal1Tex); \
    TEXTURE2D(_Decal2Tex); \
    TEXTURE2D(_Decal3Tex); \
    TEXTURE2D(_Decal4Tex); \
    TEXTURE2D(_Decal5Tex); \
    TEXTURE2D(_Decal6Tex); \
    TEXTURE2D(_Decal7Tex); \
    TEXTURE2D(_Decal8Tex); \
    TEXTURE2D(_Decal9Tex); \
    TEXTURE2D(_Decal10Tex); \

// Add vertex shader input

// Add vertex shader output

// Add vertex copy
#define LIL_CUSTOM_VERT_COPY

// Map sampler alias for decals to an inline sampler from lil_common_input.hlsl
// Define it here (outside of LIL_CUSTOM_TEXTURES macro) so the preprocessor sees it directly.
#ifndef sampler_DecalTex
#define sampler_DecalTex sampler_linear_repeat
#endif
// Inserting a process into the vertex shader
//#define LIL_CUSTOM_VERTEX_OS
//#define LIL_CUSTOM_VERTEX_WS

// Inserting a process into pixel shader
//#define BEFORE_xx
//#define OVERRIDE_xx

//----------------------------------------------------------------------------------------------------------------------
// Information about variables
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
// Vertex shader inputs (appdata structure)
//
// Type     Name                    Description
// -------- ----------------------- --------------------------------------------------------------------
// float4   input.positionOS        POSITION
// float2   input.uv0               TEXCOORD0
// float2   input.uv1               TEXCOORD1
// float2   input.uv2               TEXCOORD2
// float2   input.uv3               TEXCOORD3
// float2   input.uv4               TEXCOORD4
// float2   input.uv5               TEXCOORD5
// float2   input.uv6               TEXCOORD6
// float2   input.uv7               TEXCOORD7
// float4   input.color             COLOR
// float3   input.normalOS          NORMAL
// float4   input.tangentOS         TANGENT
// uint     vertexID                SV_VertexID

//----------------------------------------------------------------------------------------------------------------------
// Vertex shader outputs or pixel shader inputs (v2f structure)
//
// The structure depends on the pass.
// Please check lil_pass_xx.hlsl for details.
//
// Type     Name                    Description
// -------- ----------------------- --------------------------------------------------------------------
// float4   output.positionCS       SV_POSITION
// float2   output.uv01             TEXCOORD0 TEXCOORD1
// float2   output.uv23             TEXCOORD2 TEXCOORD3
// float3   output.positionOS       object space position
// float3   output.positionWS       world space position
// float3   output.normalWS         world space normal
// float4   output.tangentWS        world space tangent

//----------------------------------------------------------------------------------------------------------------------
// Variables commonly used in the forward pass
//
// These are members of `lilFragData fd`
//
// Type     Name                    Description
// -------- ----------------------- --------------------------------------------------------------------
// float4   col                     lit color
// float3   albedo                  unlit color
// float3   emissionColor           color of emission
// -------- ----------------------- --------------------------------------------------------------------
// float3   lightColor              color of light
// float3   indLightColor           color of indirectional light
// float3   addLightColor           color of additional light
// float    attenuation             attenuation of light
// float3   invLighting             saturate((1.0 - lightColor) * sqrt(lightColor));
// -------- ----------------------- --------------------------------------------------------------------
// float2   uv0                     TEXCOORD0
// float2   uv1                     TEXCOORD1
// float2   uv2                     TEXCOORD2
// float2   uv3                     TEXCOORD3
// float2   uvMain                  Main UV
// float2   uvMat                   MatCap UV
// float2   uvRim                   Rim Light UV
// float2   uvPanorama              Panorama UV
// float2   uvScn                   Screen UV
// bool     isRightHand             input.tangentWS.w > 0.0;
// -------- ----------------------- --------------------------------------------------------------------
// float3   positionOS              object space position
// float3   positionWS              world space position
// float4   positionCS              clip space position
// float4   positionSS              screen space position
// float    depth                   distance from camera
// -------- ----------------------- --------------------------------------------------------------------
// float3x3 TBN                     tangent / bitangent / normal matrix
// float3   T                       tangent direction
// float3   B                       bitangent direction
// float3   N                       normal direction
// float3   V                       view direction
// float3   L                       light direction
// float3   origN                   normal direction without normal map
// float3   origL                   light direction without sh light
// float3   headV                   middle view direction of 2 cameras
// float3   reflectionN             normal direction for reflection
// float3   matcapN                 normal direction for reflection for MatCap
// float3   matcap2ndN              normal direction for reflection for MatCap 2nd
// float    facing                  VFACE
// -------- ----------------------- --------------------------------------------------------------------
// float    vl                      dot(viewDirection, lightDirection);
// float    hl                      dot(headDirection, lightDirection);
// float    ln                      dot(lightDirection, normalDirection);
// float    nv                      saturate(dot(normalDirection, viewDirection));
// float    nvabs                   abs(dot(normalDirection, viewDirection));
// -------- ----------------------- --------------------------------------------------------------------
// float4   triMask                 TriMask (for lite version)
// float3   parallaxViewDirection   mul(tbnWS, viewDirection);
// float2   parallaxOffset          parallaxViewDirection.xy / (parallaxViewDirection.z+0.5);
// float    anisotropy              strength of anisotropy
// float    smoothness              smoothness
// float    roughness               roughness
// float    perceptualRoughness     perceptual roughness
// float    shadowmix               this variable is 0 in the shadow area
// float    audioLinkValue          volume acquired by AudioLink
// -------- ----------------------- --------------------------------------------------------------------
// uint     renderingLayers         light layer of object (for URP / HDRP)
// uint     featureFlags            feature flags (for HDRP)
// uint2    tileIndex               tile index (for HDRP)