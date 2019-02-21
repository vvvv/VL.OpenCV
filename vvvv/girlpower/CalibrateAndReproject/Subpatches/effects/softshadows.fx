// Soft Shadow map example using floating point depth texture performing percentage-closer filtering
// Source: NVIDIA

//transforms
float4x4 tW: WORLD;
float4x4 tWVP: WORLDVIEWPROJECTION;
float4x4 tWITXf : WorldInverseTranspose;
float4x4 tVIXf : ViewInverse;

float4x4 LampViewXf;
float4x4 LampProjXf;

#define PCF_SAMPS 24      //<<< SAMPLE COUNT
#define PCFH ((PCF_SAMPS-1)/2)
#define PCFT (PCF_SAMPS*PCF_SAMPS)

//textures
texture shadowTex <string uiname="shadowTexture";>;
sampler ShadSampler = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (shadowTex);          //apply a texture to the sampler
    MipFilter = none;         //sampler states
    MinFilter = point;
    MagFilter = point;
    AddressU  = CLAMP;
    AddressV = CLAMP;
};

texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state    //sampler for doing the texture-lookup
{
    Texture   = (Tex);          //apply a texture to the sampler
    MipFilter = linear;         //sampler states
    MinFilter = linear;
    MagFilter = linear;
};

//texture transformation marked with semantic TEXTUREMATRIX to achieve symmetric transformations
float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;

/////////////////////////PARAMETERS////////////////////////////////////////////

float3 SpotLightPos : POSITION <string UIName = "Lamp Posistion";> = {-1.0f, 1.0f, 0.0f};
float PCFSize <float UIMin = 0.100;string UIName = "PCF Filter Width";> = 0.01;
float SpotLightCone <string UIName = "Cone Angle";float uimin=0.0;> = 60.0;
float4 lAmb  : COLOR <String uiname="Ambient Color";>  = {0.15, 0.15, 0.15, 1};
float4 lDiff : COLOR <String uiname="Diffuse Color";>  = {0.85, 0.85, 0.85, 1};
float4 lSpec : COLOR <String uiname="Specular Color";> = {0.35, 0.35, 0.35, 1};
float lPower <String uiname="Power"; float uimin=0.0;> = 25.0;

////////////////////////////////////////////////////////////////////////////////

struct vs2ps
{
   float4 Position    : POSITION;
   float2 TexCd           : TEXCOORD0;
   float3 LightVec     : TEXCOORD1;
   float3 WNormal      : TEXCOORD2;
   float3 WView        : TEXCOORD3;
   float4 LP           : TEXCOORD4;
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------
vs2ps VS(
   float3 Position     : POSITION,
   float4 TexCd           : TEXCOORD0,
   float4 Normal       : NORMAL)
{
    //inititalize all fields of output struct with 0
    vs2ps Out = (vs2ps)0;

    //transform position
    Out.WNormal = mul(Normal,tWITXf).xyz;
    float4 Po = float4(Position.xyz,(float)1.0);     // "P" in object coordinates
    float4 Pw = mul(Po,tW);                        // "P" in world coordinates
    float4x4 ShadowViewProjXf = mul(LampViewXf,LampProjXf);    // light viewprojection
    float4 Pl = mul(Pw,ShadowViewProjXf);  // "P" in light coords

    Out.LP = Pl;
    Out.WView = normalize(tVIXf[3].xyz - Pw.xyz);     // world coords
    Out.Position = mul(Po,tWVP);    // screen clipspace coords
    Out.TexCd = TexCd.xy;
    Out.LightVec = SpotLightPos - Pw.xyz;

    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

// Utility function for pixel shaders to use this shadow map
float shadow_calc(float4 LP,	// current shaded point in light-projected coordinates
	uniform sampler ShadowMapSampler)
{
        float totalShad=0;
	int i, j;
	float offSize = PCFSize / (float)PCF_SAMPS;
	for (i = -PCFH; i<= PCFH; i += 1) {
		for (j = -PCFH; j<= PCFH; j += 1) {
			float2 offset = float2(offSize*i,offSize*j);
			float2 nuv = float2(.5,-.5)*(LP.xy+offset)/LP.w + float2(.5,.5);
			float shadMapDepth = tex2D(ShadowMapSampler,nuv).x;
			float shad = 1-(shadMapDepth<LP.z);
			totalShad += shad;
		}
	}
	return totalShad/(float)PCFT;
}

float4 useShadowPS(vs2ps In): COLOR
{
    float CosSpotAng = cos(radians(SpotLightCone));
    float3 Nn = normalize(In.WNormal);
    float3 Vn = normalize(In.WView);
    float3 Ln = normalize(In.LightVec);
    float3 Hn = normalize(Vn + Ln);
    float hdn = dot(Hn,Nn);
    float ldn = dot(Ln,Nn);
    float4 litVec = lit(ldn,hdn,lPower);
    ldn = litVec.y;

    float dl = normalize(In.LP.xyz).z;
    dl = max((float)0,((dl-CosSpotAng)/(((float)1.0)-CosSpotAng)));
    float3 diffContrib = lDiff*ldn;
    float3 specContrib = ldn * litVec.z * lSpec;
    float3 result = diffContrib + specContrib;

    float shadowed = shadow_calc(In.LP,ShadSampler);

    float4 col = tex2D(Samp, In.TexCd);

    col *= float4((dl*shadowed*result)+lAmb,1);

    //return float4(shadowed.xxx,1);
    return col;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TSimpleShader
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 useShadowPS();
    }
}
