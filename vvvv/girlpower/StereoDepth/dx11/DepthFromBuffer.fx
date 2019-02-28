//@author: vux
//@help: template for standard shaders
//@tags: template
//@credits: 

Texture2D texture2d <string uiname="Texture";>;

cbuffer cbTextureData : register(b2)
{
	float4x4 tTex <string uiname="Texture Transform"; bool uvspace=true; >;
};

SamplerState linearSampler : IMMUTABLE
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};
 
cbuffer cbPerDraw : register( b0 )
{
	float4x4 tVP : LAYERVIEWPROJECTION;	
};

cbuffer cbPerObj : register( b1 )
{
	float4x4 tW : WORLD;
	float4 cAmb <bool color=true;String uiname="Color";> = { 1.0f,1.0f,1.0f,1.0f };
};

StructuredBuffer<float> MyBuffer;

struct VS_IN
{
	float4 PosO : POSITION;
	float4 TexCd : TEXCOORD0;
	uint vi : SV_VertexID;
};

struct vs2ps
{
    float4 PosWVP: SV_Position;
	float Brightness: TEXCOORD1;
    float4 TexCd: TEXCOORD0;
};

vs2ps VS(VS_IN input)
{
    vs2ps output;
	uint vi = input.vi;
	float offset = MyBuffer[vi];
	output.Brightness = offset;
    output.PosWVP  = mul(input.PosO + float4(0, 0, offset, 0), mul(tW,tVP));
    output.TexCd = mul(input.TexCd, tTex);
    return output;
}


float4 PS(vs2ps In): SV_Target
{
    float4 col = texture2d.Sample(linearSampler,In.TexCd.xy) * cAmb;
	col.rgb = col.rgb * (In.Brightness + 0.1);
    return col;
}

technique10 Constant
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}




