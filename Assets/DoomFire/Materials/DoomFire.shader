Shader "Custom/DoomFire"
{
    Properties
    {
        [PerRendererData] _MainTex ("Fire Intensity (R8)", 2D) = "black" {}
        [PerRendererData] _PaletteTex ("Palette LUT (256x1)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment doom_fire_frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #include "UnitySprites.cginc"

            sampler2D _PaletteTex;

            fixed4 doom_fire_frag(v2f IN) : SV_Target
            {
                fixed intensity = tex2D(_MainTex, IN.texcoord).r;
                fixed4 col = tex2D(_PaletteTex, float2(intensity, 0.5)) * IN.color;
                return col;
            }
            ENDCG
        }
    }
}
