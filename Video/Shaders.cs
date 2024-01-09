namespace BattleCity.Video
{
    /// <summary>
    /// Шейдеры
    /// </summary>
    public sealed class Shaders
    {
        /// <summary>
        /// Шейдер отрисовки кирипичной стены
        /// </summary>
        public static readonly string BrickWall = @"
uniform float2 zoom = float2(80.0, 30.0);
uniform bool singleColor = true;
uniform float3 brickColor = float3(0.537,0.212,0.161);
uniform float3 lineColor = float3(0.845, 0.845, 0.845);
uniform float edgePos = 1.5;


float sincosbundle(float val)
{
    return sin(cos(2.*val) + sin(4.*val)- cos(5.*val) + sin(3.*val))*0.05;
}

float3 color(in float2 uv)
{
    //grid and coord inside each cell
    float2 coord = floor(uv);
    float2 gv = frac(uv);
    
    float movingValue = -sincosbundle(coord.y)*2.;

    float offset = floor(fmod(uv.y,2.0))*(edgePos);
    float verticalEdge = abs(cos(uv.x + offset));
    
    //color of the bricks
    float3 brick = brickColor - (singleColor ? 0 : movingValue);
        
    bool vrtEdge = step( 1. - 0.01, verticalEdge) == 1.;
    bool hrtEdge = gv.y > (0.9) || gv.y < (0.1);
    
    if(hrtEdge || vrtEdge)  
        return lineColor;
    return brick;
}

void mainImage( out float4 fragColor : COLOR0, in float2 fragCoord : TEXCOORD0 )
{
    float2 uv = fragCoord * zoom;
    fragColor = float4(color(uv),1.0);
}

technique deftech
{
    pass tpass
    {
        PixelShader = compile ps_3_0 mainImage();    
    }
}";

        /// <summary>
        /// Шейдер отрисовки игровых объектов
        /// </summary>
        public static readonly string GameObject = @"
sampler2D iChannel0 = sampler_state
{
    Filter  = NONE;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

struct ps_in
{
    float4 Position     : POSITIONT;
    float4 Color        : COLOR0;
    float2 TexCoord		: TEXCOORD0;
};

float4 mainImage( in ps_in data ) : COLOR0
{
    float4 pixel = tex2D(iChannel0, data.TexCoord);
    float checkSum = data.Color.r + data.Color.g + data.Color.b;

    if (checkSum == 3.0)
    {
        pixel.a *= data.Color.a;
    }
    else
    {
        float gray = dot(pixel.rgb, float3(0.2126, 0.7152, 0.0722));
        pixel.rgb = gray * gray * 2 * data.Color.rgb;
        pixel.a *= data.Color.a;
    }
    return pixel;
}

technique tech1
{
    pass tpass
    {
        PixelShader = compile ps_3_0 mainImage();
    }
}";

        /// <summary>
        /// Шейдер отрисовки шахматки
        /// </summary>
        public static readonly string ChessBoard = @"
uniform float     iTime;                 // shader playback time (in seconds)
uniform float2    iVPSize = float2(1,1); // screen viewport resolution (in pixels)

uniform float cols = 10.0;
uniform float rows = 8.0;
uniform float3 color1 = float3(0.0,0.0,0.0);
//uniform float3 color2 = float3(1.0,1.0,1.0);
uniform float3 color2 = float3(0.04, 0.04, 0.04);

void mainImage( out float4 fragColor : COLOR0, in float2 fragCoord : TEXCOORD0 )
{
    // Normalized pixel coordinates (from 0 to 1)
    float2 uv = fragCoord;

    uv.x *= cols; // change number of columns
    uv.y *= rows; // change number of rows
    
    // determine whether uv position is even or odd and store it in a variable
    float3 blackOrWhite = fmod(floor(uv.x) + floor(uv.y),2.0);
    
    // frac uv to draw multiple tiles
    uv = frac(uv);

       // background color
    float3 color = color1 * blackOrWhite + color2 * (1.0 - blackOrWhite);
    
    // Output to screen
    fragColor = float4(color,1.0);
}

technique deftech
{
    pass tpass
    {
        PixelShader = compile ps_3_0 mainImage();    
    }
}
";

        /// <summary>
        /// Шейдер эффекта Scanlines
        /// </summary>
        public static readonly string Scanlines = @"
uniform float     iTime;
uniform float     amount = 0.25;
uniform float     aspectY = 1;

sampler2D iChannel0 = sampler_state
{
    Filter = MIN_MAG_LINEAR_MIP_POINT;  //NONE, POINT, LINEAR, MIN_MAG_LINEAR_MIP_POINT, ANISOTROPIC
    AddressU = MIRROR;                  //MIRROR, CLAMP, BORDER
    AddressV = MIRROR;                  //MIRROR, CLAMP, BORDER
};

void mainImage( out float4 fragColor : COLOR0, in float2 fragCoord : TEXCOORD0 )
{
    // Normalized pixel coordinates (from 0 to 1)
    float2 uv = fragCoord;

    // Create a scanline effect
    //float scanline = (uv.y * 1000) % 10;// abs(cos(uv.y * 800.));
    float scanline = abs(cos(uv.y * 800.));
    scanline = smoothstep(0.0, 2.0, scanline * aspectY);

    float3 f = tex2D(iChannel0, uv).rgb - (amount * scanline);

    // Output to screen
    fragColor = float4(f, 1.0);
}

technique deftech
{
    pass tpass
    {
        PixelShader = compile ps_3_0 mainImage();    
    }
}

";

        /// <summary>
        /// Шейдер сетки
        /// </summary>
        public static readonly string GridLines = @"
uniform float2    iVPSize = float2(1,1);
uniform float4    color = 1;
uniform float     lineThickness = 1;
uniform float     gridIncrement = .1;

void mainImage( out float4 fragColor : COLOR0, in float2 fragCoord : TEXCOORD0 )
{
    float2 uv = fragCoord.xy;
    uv.x *= iVPSize.x / iVPSize.y;
    float gridLineThickness = lineThickness / iVPSize.y;
    uv =  step( fmod(uv, gridIncrement), float2(gridLineThickness, gridLineThickness) );    
    fragColor = color * (uv.x+uv.y);
}

technique deftech
{
    pass tpass
    {
        PixelShader = compile ps_3_0 mainImage();    
    }
}";
    }
}
