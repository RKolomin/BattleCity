using SlimDX.Direct3D9;
using SlimDX;

namespace BattleCity.Video
{
    /// <summary>
    /// Describes a custom vertex format structure that contains transformed vertices
    /// and color information.
    /// </summary>
    public struct TransformedColored
    {
        /// <summary>
        /// Retrieves the VertexFormats for the current custom vertex.
        /// </summary>
        public const VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Diffuse;

        /// <summary>
        /// Retrieves or sets the x component of the position.
        /// </summary>
        public float X;

        /// <summary>
        /// Retrieves or sets the y component of the position.
        /// </summary>
        public float Y;

        /// <summary>
        /// Retrieves or sets the z component of the position.
        /// </summary>
        public float Z;

        /// <summary>
        /// Retrieves or sets the reciprocal homogeneous w (RHW) component of the position.
        /// </summary>
        public float Rhw;

        /// <summary>
        /// Retrieves or sets the vertex color.
        /// </summary>
        public int Color;

        /// <summary>
        /// Initializes a new instance of the CustomVertex.TransformedColored
        /// </summary>
        /// <param name="position">A Vector4 object that contains the position.</param>
        /// <param name="color">Integer that represents the vertex color value.</param>
        public TransformedColored(Vector4 position, int color)
        {
            X = position.X;
            Y = position.Y;
            Z = position.Z;
            Rhw = position.W;
            Color = color;
        }

        //
        // Summary:
        //     Initializes a new instance of the CustomVertex.TransformedColored
        //     class.
        //
        // Parameters:
        //   xvalue:
        //     Floating-point value that represents the x coordinate of the position.
        //
        //   yvalue:
        //     Floating-point value that represents the y coordinate of the position.
        //
        //   zvalue:
        //     Floating-point value that represents the z coordinate of the position.
        //
        //   c:
        //     Integer that represents the vertex color value.
        public TransformedColored(float xvalue, float yvalue, float zvalue, int c)
        {
            X = xvalue;
            Y = yvalue;
            Z = zvalue;
            Rhw = 1;
            Color = c;
        }
        //
        // Summary:
        //     Initializes a new instance of the CustomVertex.TransformedColored
        //     class.
        //
        // Parameters:
        //   xvalue:
        //     Floating-point value that represents the x coordinate of the position.
        //
        //   yvalue:
        //     Floating-point value that represents the y coordinate of the position.
        //
        //   zvalue:
        //     Floating-point value that represents the z coordinate of the position.
        //
        //   rhwvalue:
        //     Floating-point value that represents the reciprocal homogeneous w (RHW) component
        //     of the transformed vertex.
        //
        //   c:
        //     Integer that represents the vertex color value.
        public TransformedColored(float xvalue, float yvalue, float zvalue, float rhwvalue, int c)
        {
            X = xvalue;
            Y = yvalue;
            Z = zvalue;
            Rhw = rhwvalue;
            Color = c;
        }

        //
        // Summary:
        //     Initializes a new instance of the CustomVertex.TransformedColored
        //     class.
        //
        // Parameters:
        //   xvalue:
        //     Floating-point value that represents the x coordinate of the position.
        //
        //   yvalue:
        //     Floating-point value that represents the y coordinate of the position.
        //
        //   zvalue:
        //     Floating-point value that represents the z coordinate of the position.
        //
        //   rhwvalue:
        //     Floating-point value that represents the reciprocal homogeneous w (RHW) component
        //     of the transformed vertex.
        //
        //   c:
        //     Integer that represents the vertex color value.
        public TransformedColored(float xvalue, float yvalue, float zvalue, float rhwvalue, Color4 c)
        {
            X = xvalue;
            Y = yvalue;
            Z = zvalue;
            Rhw = rhwvalue;
            Color = c.ToArgb();
        }

        /// <summary>
        /// Retrieves or sets the transformed position.
        /// </summary>
        public Vector4 Position
        {
            get { return new Vector4(X, Y, Z, Rhw); }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
                Rhw = value.W;
            }
        }

        /// <summary>
        /// Retrieves the size of the CustomVertex.TransformedColored structure.
        /// </summary>
        public static int StrideSize { get { return 5 * 4; } }

        /// <summary>
        /// Obtains a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString() { return Position.ToString() + " " + Color.ToString(); }
    };

    /// <summary>
    /// Describes a custom vertex format structure that contains transformed vertices
    /// and one set of texture coordinates.
    /// </summary>
    public struct TransformedTextured
    {
        // Summary:
        //     Retrieves the VertexFormats for the current custom
        //     vertex.
        public const VertexFormat Format = (VertexFormat)260;

        public static VertexElement[] Elements =
        {
            new VertexElement(0, 0,  DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
            new VertexElement(0, 16, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
            VertexElement.VertexDeclarationEnd
        };

        //
        // Summary:
        //     Retrieves or sets the x component of the position.
        public float X;
        //
        // Summary:
        //     Retrieves or sets the y component of the position.
        public float Y;
        //
        // Summary:
        //     Retrieves or sets the z component of the position.
        public float Z;

        // Summary:
        //     Retrieves or sets the reciprocal homogeneous w (RHW) component of the position.
        public float Rhw;
        //
        // Summary:
        //     Retrieves or sets the u component of the texture coordinate.
        public float Tu;
        //
        // Summary:
        //     Retrieves or sets the v component of the texture coordinate.
        public float Tv;

        //
        // Summary:
        //     Initializes a new instance of the CustomVertex.TransformedTextured
        //     class.
        //
        // Parameters:
        //   value:
        //     A Vector4 object that contains the position.
        //
        //   u:
        //     Floating-point value that represents the CustomVertex.TransformedTextured.#ctor()
        //     component of the texture coordinate.
        //
        //   v:
        //     Floating-point value that represents the CustomVertex.TransformedTextured.#ctor()
        //     component of the texture coordinate.
        public TransformedTextured(Vector4 value, float u, float v)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            Rhw = value.W;
            Tu = u;
            Tv = v;
        }
        //
        // Summary:
        //     Initializes a new instance of the CustomVertex.TransformedTextured
        //     class.
        //
        // Parameters:
        //   xvalue:
        //     Floating-point value that represents the x coordinate of the position.
        //
        //   yvalue:
        //     Floating-point value that represents the y coordinate of the position.
        //
        //   zvalue:
        //     Floating-point value that represents the z coordinate of the position.
        //
        //   rhwvalue:
        //     Floating-point value that represents the reciprocal homogeneous w (RHW) component
        //     of the transformed vertex.
        //
        //   u:
        //     Floating-point value that represents the CustomVertex.TransformedTextured.#ctor()
        //     component of the texture coordinate.
        //
        //   v:
        //     Floating-point value that represents the CustomVertex.TransformedTextured.#ctor()
        //     component of the texture coordinate.
        public TransformedTextured(float xvalue, float yvalue, float zvalue, float rhwvalue, float u, float v)
        {
            X = xvalue;
            Y = yvalue;
            Z = zvalue;
            Rhw = rhwvalue;
            Tu = u;
            Tv = v;
        }

        // Summary:
        //     Retrieves or sets the transformed position.
        public Vector4 Position
        {
            get { return new Vector4(X, Y, Z, Rhw); }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
                Rhw = value.W;
            }
        }

        public void SetXY(float x, float y)
        {
            X = x;
            Y = y;
        }

        //
        // Summary:
        //     Retrieves the size of the CustomVertex.TransformedTextured
        //     structure.
        public static int StrideSize { get { return 6 * 4; } }

        // Summary:
        //     Obtains a string representation of the current instance.
        //
        // Returns:
        //     String that represents the object.
        public override string ToString() { return Position.ToString() + new Vector2(Tu, Tv); }
    };

    /// <summary>
    /// Describes a custom vertex format structure that contains transformed vertices.
    /// </summary>
    public struct PositionW
    {
        /// <summary>
        /// Retrieves the VertexFormats for the current custom vertex.
        /// </summary>
        public const VertexFormat Format = VertexFormat.PositionW;

        /// <summary>
        /// Retrieves the array of VertexElement for the current custom vertex.
        /// </summary>
        public static VertexElement[] Elements =
        {
            new VertexElement(0, 0,  DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
            VertexElement.VertexDeclarationEnd
        };

        /// <summary>
        /// Retrieves or sets the x component of the position.
        /// </summary>
        public float X;

        /// <summary>
        /// Retrieves or sets the y component of the position.
        /// </summary>
        public float Y;

        /// <summary>
        /// Retrieves or sets the z component of the position.
        /// </summary>
        public float Z;

        /// <summary>
        /// Retrieves or sets the reciprocal homogeneous w (RHW) component of the position.
        /// </summary>
        public float W;

        /// <summary>
        /// Initializes a new instance of the CustomVertex.Transformed
        /// </summary>
        /// <param name="value">A Vector4 object that contains the position.</param>
        public PositionW(Vector4 value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = value.W;
        }

        /// <summary>
        /// Initializes a new instance of the CustomVertex.Transformed
        /// </summary>
        /// <param name="value">A Vector3 object that contains the position.</param>
        public PositionW(Vector3 value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = 1;
        }

        /// <summary>
        /// Initializes a new instance of the CustomVertex.Transformed
        /// </summary>
        /// <param name="xvalue">Floating-point value that represents the x coordinate of the position.</param>
        /// <param name="yvalue">Floating-point value that represents the y coordinate of the position.</param>
        /// <param name="zvalue">Floating-point value that represents the z coordinate of the position.</param>
        /// <param name="wvalue">Floating-point value that represents the reciprocal homogeneous w (W) component
        /// of the transformed vertex.</param>
        public PositionW(float xvalue, float yvalue, float zvalue, float wvalue)
        {
            X = xvalue;
            Y = yvalue;
            Z = zvalue;
            W = wvalue;
        }

        /// <summary>
        /// Retrieves or sets the transformed position.
        /// </summary>
        public Vector4 Position
        {
            get { return new Vector4(X, Y, Z, W); }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
                W = value.W;
            }
        }

        /// <summary>
        /// Retrieves the size of the CustomVertex.Transformed structure.
        /// </summary>
        public static int StrideSize { get { return 4 * 4; } }
  
        /// <summary>
        /// Obtains a string representation of the current instance.
        /// </summary>
        /// <returns>String that represents the object.</returns>
        public override string ToString() { return Position.ToString(); }
    }

    /// <summary>
    /// Describes a custom vertex format structure that contains transformed vertices,
    /// color, and one set of texture coordinates.
    /// </summary>
    public struct TransformedColoredTextured
    {
        // Summary:
        //     Retrieves the VertexFormats for the current custom
        //     vertex.
        public const VertexFormat Format = VertexFormat.Texture1 | VertexFormat.Diffuse | VertexFormat.PositionRhw;

        public static readonly VertexElement[] Elements =
        {
            new VertexElement(0, 0,  DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0),
            new VertexElement(0, 16, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
            new VertexElement(0, 20, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
            VertexElement.VertexDeclarationEnd
        };

        //
        // Summary:
        //     Retrieves or sets the x component of the position.
        public float X;
        //
        // Summary:
        //     Retrieves or sets the y component of the position.
        public float Y;
        //
        // Summary:
        //     Retrieves or sets the z component of the position.
        public float Z;
        //
        // Summary:
        //     Retrieves or sets the reciprocal homogeneous w (RHW) component of the position.
        public float Rhw;
        // Summary:
        //     Retrieves or sets the vertex color.
        public int Color;
        //
        // Summary:
        //     Retrieves or sets the u component of the texture coordinate.
        public float Tu;
        //
        // Summary:
        //     Retrieves or sets the v component of the texture coordinate.
        public float Tv;

        public void SetXY(float x, float y)
        {
            X = x;
            Y = y;
        }

        public TransformedColoredTextured(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
            Rhw = 1;
            Color = 0;
            Tu = 0;
            Tv = 0;
        }

        public TransformedColoredTextured(float x, float y, float z, int c)
        {
            X = x;
            Y = y;
            Z = z;
            Rhw = 1;
            Color = c;
            Tu = 0;
            Tv = 0;
        }

        //
        // Summary:
        //     Initializes a new instance of the CustomVertex.TransformedColoredTextured
        //     class.
        //
        // Parameters:
        //   value:
        //     A Vector4 object that contains the position.
        //
        //   c:
        //     Integer that represents the vertex color value.
        //
        //   u:
        //     Floating-point value that represents the CustomVertex.TransformedColoredTextured.#ctor()
        //     component of the texture coordinate.
        //
        //   v:
        //     Floating-point value that represents the CustomVertex.TransformedColoredTextured.#ctor()
        //     component of the texture coordinate.
        public TransformedColoredTextured(Vector4 value, int c, float u, float v)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            Rhw = value.W;
            Color = c;
            Tu = u;
            Tv = v;
        }
        //
        // Summary:
        //     Initializes a new instance of the CustomVertex.TransformedColoredTextured
        //     class.
        //
        // Parameters:
        //   xvalue:
        //     Floating-point value that represents the x coordinate of the position.
        //
        //   yvalue:
        //     Floating-point value that represents the y coordinate of the position.
        //
        //   zvalue:
        //     Floating-point value that represents the z coordinate of the position.
        //
        //   rhwvalue:
        //     Floating-point value that represents the reciprocal homogeneous w (RHW) component
        //     of the transformed vertex.
        //
        //   c:
        //     Integer that represents the vertex color value.
        //
        //   u:
        //     Floating-point value that represents the CustomVertex.TransformedColoredTextured.#ctor()
        //     component of the texture coordinate.
        //
        //   v:
        //     Floating-point value that represents the CustomVertex.TransformedColoredTextured.#ctor()
        //     component of the texture coordinate.
        public TransformedColoredTextured(float xvalue, float yvalue, float zvalue, float rhwvalue, int c, float u, float v)
        {
            X = xvalue;
            Y = yvalue;
            Z = zvalue;
            Rhw = rhwvalue;
            Tu = u;
            Tv = v;
            Color = c;
        }

        // Summary:
        //     Retrieves or sets the transformed position.
        public Vector4 Position
        {
            get { return new Vector4(X, Y, Z, Rhw); }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
                Rhw = value.W;
            }
        }
        //
        // Summary:
        //     Retrieves the size of the CustomVertex.TransformedColoredTextured
        //     structure.
        public static int StrideSize { get { return 7 * 4; } }

        // Summary:
        //     Obtains a string representation of the current instance.
        //
        // Returns:
        //     String that represents the object.
        public override string ToString() { return Position.ToString() + " " + new Vector2(Tu, Tv).ToString() + " " + Color; }
    }
}
