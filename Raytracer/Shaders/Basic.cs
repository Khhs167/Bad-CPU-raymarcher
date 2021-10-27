using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Raymarcher.Shaders
{

    public enum LightningType
    {
        Basic, Sun, None
    }

    public class Basic : IShader
    {
        public static IShader shader = new Basic();

        public LightningType lightning = LightningType.Sun;

        public float roughness = 1f;

        public Color Fragment(FragmentInput args)
        {

            float light = CalculateLightning(args);
            //args.sphere.color
            Color c = args.sphere.color;

            if (roughness < 1 && args.layer < 5)
                c = LerpC(GetReflection(args), c, roughness);

            c = MultC(c, Math.Clamp(light / MathF.Min(roughness, 0.000001f), 0f, 1f));
            return c;
        }

        public float CalculateLightning(FragmentInput args)
        {
            Vector3 b = Vector3.Reflect(args.ray, args.sphere.CalculateNormal(args.position));

            if (lightning == LightningType.Basic)
                return SimpleSun(b) / MathF.PI;
            else if (lightning == LightningType.Sun)
                return SunRay(args.position, Program.sunStrength, args.sphere);
            else if (lightning == LightningType.None)
                return 1;

            throw new Exception("No lightning alivalable for fragment shader!");
        }

        public static float SunRay(Vector3 origin, float maxLen, Sphere sphere)
        {
            float distanceTraveled = 0;

            Vector3 direction = Vector3.Normalize(Program.sun);

           
            origin -= sphere.CalculateNormal(origin) * Program.precision * 2;

            while (distanceTraveled <= maxLen)

            {
                Vector3 pos = origin + (direction * distanceTraveled);
                float lowestDst = float.MaxValue;

                for (int o = 0; o < Program.spheres.Count; o++)
                {
                    float dst = Program.spheres[o].DistanceToSurface(pos);

                    //if (Program.spheres[o] == sphere)
                    //    continue;

                    if (dst <= Program.precision)
                    {
                        return 0;
                    }

                    lowestDst = MathF.Min(lowestDst, dst);
                }

                distanceTraveled += lowestDst;
            }

            return 1;
        }

        public static Color GetReflection(FragmentInput args)
        {
            float distanceTraveled = 0;

            Vector3 direction = Vector3.Reflect(args.ray, args.sphere.CalculateNormal(args.position));

            //origin += sphere.CalculateNormal(origin) * Program.precision;
            while (distanceTraveled <= Program.farPlane)
            {
                Vector3 pos = args.position + (direction * distanceTraveled);
                float lowestDst = float.MaxValue;

                for (int o = 0; o < Program.spheres.Count; o++)
                {

                    if (Program.spheres[o] == args.sphere)
                        continue;

                    float dst = Program.spheres[o].DistanceToSurface(pos);

                    if (dst <= Program.precision)
                    {
                        return Program.spheres[o].shader.Fragment(new FragmentInput { position = pos, ray = direction, sphere = Program.spheres[o], layer = args.layer + 1 });
                    }

                    lowestDst = MathF.Min(lowestDst, dst);
                }

                distanceTraveled += lowestDst;
            }
            
            return Color.BLUE;
        }

        public static Color MultC(Color a, Color b)
        {
            return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        }
        public static Color MultC(Color a, float b)
        {
            return new Color((byte)(a.r * b), (byte)(a.g * b), (byte)(a.b * b), a.a);
        }

        public static Color LerpC(Color a, Color b, float x)
        {
            Vector4 av = new Vector4(a.r, a.g, a.b, a.a);
            Vector4 bv = new Vector4(b.r, b.g, b.b, b.a);
            Vector4 o = Vector4.Lerp(av, bv, x);
            return new Color((int)o.X, (int)o.Y, (int)o.Z, (int)o.W);
        }

        public static float SimpleSun(Vector3 n)
        {
            return MathF.Acos(Vector3.Dot(n, Program.sun) / (n.Length() * Program.sun.Length()));
        }
    }
}
