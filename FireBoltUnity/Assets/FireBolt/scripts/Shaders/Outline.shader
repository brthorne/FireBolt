Shader "Hidden/Outline"
{
	Properties
	{
		_MainTex("", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
	}

		SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

	sampler2D _CameraDepthTexture;

	fixed4 _Color;
	uniform float4 _MainTex_TexelSize;


	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv[5] : TEXCOORD0;
	};

	v2f vert(appdata_img v) 
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		float2 uv = v.texcoord.xy;
		o.uv[0] = uv;

#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			uv.y = 1 - uv.y;
#endif

		o.uv[1] = uv + _MainTex_TexelSize.xy * half2(1,1) * 1;
		o.uv[2] = uv + _MainTex_TexelSize.xy * half2(-1,-1) * 1;
		o.uv[3] = uv + _MainTex_TexelSize.xy * half2(-1,1) * 1;
		o.uv[4] = uv + _MainTex_TexelSize.xy * half2(1,-1) * 1;

		return o;
	}

	sampler2D _MainTex;
	float4 _HighlightDirection;

	half4 frag(v2f i) : SV_TARGET
	{
		float sample1 = Linear01Depth(tex2D(_CameraDepthTexture, i.uv[1].xy).r);
		float sample2 = Linear01Depth(tex2D(_CameraDepthTexture, i.uv[2].xy).r);
		float sample3 = Linear01Depth(tex2D(_CameraDepthTexture, i.uv[3].xy).r);
		float sample4 = Linear01Depth(tex2D(_CameraDepthTexture, i.uv[4].xy).r);

		half edge = 1.0;

		edge *= (sample1 == 1) == (sample2 == 1);
		edge *= (sample1 == 1) == (sample4 == 1);

		if (edge > 0.99 )
		{
			float4 ret = tex2D(_MainTex, i.uv[0]);
			ret.a = 0;
			return ret;
		}
		else
			return _Color;
	}
		ENDCG
	}
	}
		FallBack "Diffuse"
}