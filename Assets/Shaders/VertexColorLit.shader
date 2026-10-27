// Простой lit-шейдер с vertex color — под палитровые модели Kenney.
Shader "Custom/VertexColorLit"
{
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.08
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input
        {
            float4 vertColor : COLOR;
        };

        fixed4 _Color;
        half _Glossiness;

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertColor = v.color;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = IN.vertColor.rgb * _Color.rgb;
            o.Smoothness = _Glossiness;
            o.Metallic = 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
