using System;
using Raylib_cs;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;

namespace Raymarcher
{
    class Program
    {
        public static int width = 320, height = 180;
        public static float ratio = (float)height / width;
        public static float nearPlane = 1f, farPlane = 10f;

        public static float precision = 0.01f;

        public static float average = 0f;

        public static Vector3 sun = new Vector3(-1, -1, 0);

        public static List<Sphere> spheres = new List<Sphere>
        {
            new Sphere
            {
                posistion = new Vector3(11.5f, 0f, 0.5f),
                color = Color.RED,
                radius = 1f,
                shader = Shaders.Basic.shader,
                rnd = 0f
            }
            
        };

        static void Main(string[] args)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            bool debugMenu = false;

            Raylib.SetTraceLogLevel(TraceLogLevel.LOG_NONE);
            Raylib.InitWindow(width, height, "Raymarcher");
            Image image = Raylib.GenImageColor(width, height, Color.BLANK);
            
            Console.WriteLine("Started!");

            while (!Raylib.WindowShouldClose())
            {
                Raytrace(ref image, out long mspf, out long mspd);
                //average += mspf;
                //average *= 0.5f;
                average = MathF.Max(average, 1000f / mspf);

                Texture2D texture2D = Raylib.LoadTextureFromImage(image);

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);

                Raylib.DrawTexture(texture2D, 0, 0, Color.WHITE);

                if (debugMenu)
                {

                    Raylib.DrawText("REND: " + mspf + " ms", 5, 5, 10, Color.GREEN);
                    //Raylib.DrawText("HIGH: " + average, 5, 5, 10, Color.GREEN);
                    Raylib.DrawText("DRAW: " + mspd + " ms", 5, 15, 10, Color.GREEN);
                    Raylib.DrawText("FPS : " + 1000 / (mspd + mspf), 5,25, 10, Color.GREEN);

                    Raylib.DrawText("RESL: " + width + "x" + height, 5, height - 25, 10, Color.GREEN);
                    Raylib.DrawText("OBJC: " + spheres.Count, 5, height - 15, 10, Color.GREEN);
                }

                Raylib.EndDrawing();
                Raylib.UnloadTexture(texture2D);

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_F1))
                {
                    debugMenu = !debugMenu;
                }

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_Q))
                {
                    spheres.Add(new Sphere
                    {
                        posistion = new Vector3(0f, (float)random.NextDouble() * 2 - 0.5f, (float)random.NextDouble() * 2),
                        color = new Color(random.Next(256), random.Next(256),random.Next(256), 255),
                        radius = (float)random.NextDouble() * 0.5f,
                        shader = Shaders.Basic.shader,
                        rnd = (float)random.NextDouble() * 100f
                    });
                }

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_E) && spheres.Count > 0)
                {
                    spheres.RemoveAt(random.Next(spheres.Count));
                }


                foreach (var sphere in spheres)
                {
                    sphere.posistion.X = MathF.Sin((float)Raylib.GetTime() + sphere.rnd) + 3f;
                }
                
            }
        }

        static void Raytrace(ref Image image, out long mspf, out long mspd)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var screen = new Color[width, height];

            Raylib.ImageClearBackground(ref image, Color.RED);

            for (int u = 0; u < width; u++)
            {
                for (int v = 0; v < height; v++)
                {
                    screen[u, v] = RenderPixel(u, v);
                }
            }
            stopwatch.Stop();
            mspf = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            for (int u = 0; u < width; u++)
            {
                for (int v = 0; v < height; v++)
                {
                    Raylib.ImageDrawPixel(ref image, u, v, screen[u, v]);
                }
            }
            stopwatch.Stop();
            mspd = stopwatch.ElapsedMilliseconds;

            //Console.WriteLine("Frame took " + stopwatch.ElapsedMilliseconds + " ms to complete");
        }

        static Color RenderPixel(int u, int v)
        {
            
            float distanceTraveled = 0;

            Vector2 screenCoord = new Vector2(u / (float)width / ratio, v / (float)height) - Vector2.One * 0.5f;
            Vector3 worldCoord = new Vector3(nearPlane, screenCoord.Y, screenCoord.X);

            Vector3 direction = worldCoord / worldCoord.Length();

            while(distanceTraveled <= farPlane)
            {
                Vector3 pos = worldCoord + (direction * distanceTraveled);
                float lowestDst = float.MaxValue;

                for (int o = 0; o < spheres.Count; o++)
                {
                    float dst = spheres[o].DistanceToSurface(pos);
                    if (dst <= precision)
                    {
                        return spheres[o].shader.Fragment(new FragmentInput { position = pos, ray = direction, sphere = spheres[o] });
                    }

                    lowestDst = MathF.Min(lowestDst, dst);
                }

                distanceTraveled += lowestDst;
            }

            return Color.BLUE;
        }
    }

    public class Sphere
    {
        public float radius;
        public Color color;
        public Vector3 posistion;
        public IShader shader;
        public float rnd;

        public bool IsInside(Vector3 pos)
        {
            return (posistion - pos).Length() < radius;
        }

        public float DistanceToSurface(Vector3 pos)
        {
            return (posistion - pos).Length() - radius;
        }

        public Vector3 CalculateNormal(Vector3 pos)
        {
            Vector3 diffVec = posistion - pos;
            float len = diffVec.Length();
            Vector3 n = diffVec / len;
            return n;
        }
    }
}
