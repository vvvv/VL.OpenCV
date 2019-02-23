// this is a very basic template. use it to start writing your own effects.
// if you want effects with lighting start from one of the GouraudXXXX or PhongXXXX effects

// --------------------------------------------------------------------------------------------------
// PARAMETERS:
// --------------------------------------------------------------------------------------------------
float4x4 tWVP: WORLDVIEWPROJECTION;
struct vs2ps
{
   float4 Pos: POSITION;
   float4 TexCd: TEXCOORD0;
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------
vs2ps VS_ZBuffer(float4 Pos: POSITION)
{
   vs2ps Out = (vs2ps) 0;
   Out.Pos = mul(Pos, tWVP);
   Out.TexCd = Out.Pos;
   return Out;
}


// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------
float4 PS_ZBuffer(vs2ps In) : COLOR
{
    float dist = In.TexCd.z;
    return float4(dist.xxx, 1);
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique DepthTex
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_ZBuffer();
        PixelShader  = compile ps_3_0 PS_ZBuffer();
    }
}
