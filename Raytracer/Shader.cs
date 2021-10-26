using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Raytracer
{
    public interface IShader
    {
        public Color Fragment(FragmentInput args);
    }

    public struct FragmentInput
    {
        public Vector3 position;
        public Vector3 ray;
        public Sphere sphere;
    }
}
