Shader "Protocolo404/BordaInspecao"
{
    Properties
    {
        [PerRendererData] _MainTex ("Textura UI", 2D) = "white" {}
        _Color ("Cor da borda", Color) = (0.5, 1, 0.85, 1)
        _Espessura ("Espessura", Range(0.002, 0.05)) = 0.012
        _Velocidade ("Pulsos por segundo", Range(0.2, 3)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Espessura;
            float _Velocidade;

            struct Entrada
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct Saida
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            Saida vert(Entrada entrada)
            {
                Saida saida;
                saida.vertex = UnityObjectToClipPos(entrada.vertex);
                saida.uv = entrada.uv;
                saida.color = entrada.color;
                return saida;
            }

            fixed4 frag(Saida entrada) : SV_Target
            {
                float2 distancia = min(entrada.uv, 1.0 - entrada.uv);
                float borda = 1.0 - step(
                    _Espessura,
                    min(distancia.x, distancia.y)
                );

                float pulso = 0.65 + 0.35 *
                    sin(_Time.y * _Velocidade * 6.283185);

                fixed4 cor = _Color * entrada.color;
                cor.a *= borda * pulso;
                return cor;
            }
            ENDCG
        }
    }
}