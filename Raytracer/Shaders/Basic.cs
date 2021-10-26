﻿using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Raymarcher.Shaders
{
    public class Basic : IShader
    {
        public static IShader shader = new Basic();

        // Change for a lightshow
        public static bool basicSun = true;

        public Color Fragment(FragmentInput args)
        {
            
            float light = CalculateLightning(args);

            Color c = MultC(args.sphere.color, light);
            return c;
        }

        public float CalculateLightning(FragmentInput args)
        {
            Vector3 b = Vector3.Reflect(args.sphere.CalculateNormal(args.position), args.ray);

            if(basicSun)
                return SimpleSun(b) / MathF.PI;
            else
                return Ray(args.position, args.ray, Program.sunStrength, args.sphere) / Program.sunStrength;
        }

        public static float Ray(Vector3 origin, Vector3 direction, float maxLen, Sphere sphere)
        {
            float distanceTraveled = Program.precision + float.Epsilon;
            while (distanceTraveled <= maxLen)
            {
                Vector3 pos = origin + (direction * distanceTraveled);
                float lowestDst = float.MaxValue;

                for (int o = 0; o < Program.spheres.Count; o++)
                {
                    float dst = Program.spheres[o].DistanceToSurface(pos);
                    if (dst <= Program.precision)
                    {
                        return maxLen;
                    }

                    lowestDst = MathF.Min(lowestDst, dst);
                }

                distanceTraveled += lowestDst;
            }

            return distanceTraveled - 10;
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
