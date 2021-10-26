using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Raymarcher.Shaders
{
    public class Basic : IShader
    {
        public static IShader shader = new Basic();


        public Color Fragment(FragmentInput args)
        {
            Vector3 b = Vector3.Reflect(args.sphere.CalculateNormal(args.position), args.ray);
            float light = SimpleSun(b) / MathF.PI;
            //return b.Y < 0.5f ? spheres[o].color : Color.BLACK;
            Color c = MultC(args.sphere.color, light);
            //c.a -= (byte)(precision / (precision - dst) * 255);
            return c;
            //return new Color((byte)(light * 255), (byte)(light * 255), (byte)(light * 255), (byte)255);
            //return new Color((byte)(b.X * 255), (byte)(b.Y * 255), (byte)(b.Z * 255), (byte)255);
        }


        public static Color MultC(Color a, Color b)
        {
            return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        }
        public static Color MultC(Color a, float b)
        {
            return new Color((byte)(a.r * b), (byte)(a.g * b), (byte)(a.b * b), a.a);
        }

        public static float SimpleSun(Vector3 n)
        {
            return MathF.Acos(Vector3.Dot(n, Program.sun) / (n.Length() * Program.sun.Length()));
        }
    }
}
