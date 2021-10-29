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
        public static float precision = 0.001f;
        public static Vector3 cameraPosition;
        public static float cameraRotation = 1.57079633f;

        public static Vector3 cameraForward = Vector3.Zero;
        public static Vector3 cameraRight = Vector3.Zero;
        
        public static float sunStrength = 20f;

        public static float average = 0f;

        static bool hasWarned = false;

        public static Vector3 sun = new Vector3(-10, -10, -10);

        public static List<Sphere> spheres = new List<Sphere>
        {
            new Sphere
            {
                posistion = new Vector3(11.5f, 0f, 0.5f),
                color = Color.RED,
                radius = 0.5f,
                shader = Shaders.Basic.shader,
                rnd = 0f
            }
            
        };

        public static List<Sphere> visibleSpheres = new List<Sphere>();

        static void Main(string[] args)
        {

            cameraForward = new Vector3(MathF.Sin(cameraRotation), 0, MathF.Cos(cameraRotation));
                    cameraRight = new Vector3(MathF.Sin(cameraRotation + 1.57079633f), 0, MathF.Cos(cameraRotation + 1.57079633f));

            Random random = new Random(DateTime.Now.Millisecond);
            bool debugMenu = false;

            Raylib.SetTraceLogLevel(TraceLogLevel.LOG_NONE);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.InitWindow(width, height, "Raymarcher");
            
            Image image = Raylib.GenImageColor(width, height, Color.BLANK);
            
            Console.WriteLine("Started!");
            Shaders.Basic shader = new Shaders.Basic();
            shader.roughness = 1f;
            spheres[0].shader = shader;

            while (!Raylib.WindowShouldClose())
            {

                Raytrace(ref image, out long mspf, out long mspd);
                //average += mspf;
                //average *= 0.5f;
                average = MathF.Max(average, 1000f / mspf);

                if(!hasWarned && 1000f / (mspd + mspf) <= 3f){
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Low performance! Is the resolution too high or can't the hardware keep up?");
                    Console.ResetColor();
                    hasWarned = true;
                } else if(1000f / (mspd + mspf) > 3f){
                    hasWarned = false;
                }

                Texture2D texture2D = Raylib.LoadTextureFromImage(image);

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);

                Raylib.DrawTexture(texture2D, 0, 0, Color.WHITE);

                if (debugMenu)
                {

                    Raylib.DrawText("REND: " + mspf + " ms", 5, 5, 10, Color.GREEN);
                    //Raylib.DrawText("HIGH: " + average, 5, 5, 10, Color.GREEN);
                    Raylib.DrawText("DRAW: " + mspd + " ms", 5, 15, 10, Color.GREEN);
                    Raylib.DrawText("FPS : " + 1000f / (mspd + mspf), 5,25, 10, Color.GREEN);

                    Raylib.DrawText("RESL: " + width + "x" + height, 5, height - 25, 10, Color.GREEN);
                    Raylib.DrawText("OBJC: " + spheres.Count, 5, height - 15, 10, Color.GREEN);

                    Raylib.DrawText($"X: {cameraPosition.X}, Y: {cameraPosition.Y}, Z: {cameraPosition.Z}", 5, height - 35, 10, Color.GREEN);
                }

                Raylib.EndDrawing();
                Raylib.UnloadTexture(texture2D);

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_F1))
                {
                    debugMenu = !debugMenu;
                }

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_Q))
                {

                    float size = (float)random.NextDouble() * 0.5f;

                    spheres.Add(new Sphere
                    {
                        //posistion = new Vector3(0f, (float)random.NextDouble() * 2 - 0.5f, (float)random.NextDouble() * 2),
                        posistion = cameraPosition + (cameraForward * (nearPlane + 1f)),
                        color = new Color(random.Next(256), random.Next(256),random.Next(256), 255),
                        radius = size,
                        shader = Shaders.Basic.shader,
                        rnd = (float)random.NextDouble() * 100f
                    });
                }
                float dt = mspd + mspf;
                if(Raylib.IsKeyDown(KeyboardKey.KEY_W)){
                    cameraPosition += cameraForward * dt * 0.001f;
                }
                if(Raylib.IsKeyDown(KeyboardKey.KEY_S)){
                    cameraPosition -= cameraForward * dt * 0.001f;
                }
                if(Raylib.IsKeyDown(KeyboardKey.KEY_D)){
                    cameraRotation += dt * 0.001f;
                    cameraForward = new Vector3(MathF.Sin(cameraRotation), 0, MathF.Cos(cameraRotation));
                    cameraRight = new Vector3(MathF.Sin(cameraRotation + 1.57079633f), 0, MathF.Cos(cameraRotation + 1.57079633f));
                }
                if(Raylib.IsKeyDown(KeyboardKey.KEY_A)){
                    cameraRotation -= dt * 0.001f;
                    cameraForward = new Vector3(MathF.Sin(cameraRotation), 0, MathF.Cos(cameraRotation));
                    cameraRight = new Vector3(MathF.Sin(cameraRotation + 1.57079633f), 0, MathF.Cos(cameraRotation + 1.57079633f));
                }

                if (Raylib.IsKeyPressed(KeyboardKey.KEY_E) && spheres.Count > 0)
                {
                    spheres.RemoveAt(random.Next(spheres.Count));
                }

                if (Raylib.IsWindowResized())
                {
                    width = Raylib.GetScreenWidth();
                    height = Raylib.GetScreenHeight();
                    ratio = (float)height / width;
                    image = Raylib.GenImageColor(width, height, Color.BLANK);
                }


                foreach (var sphere in spheres)
                {
                    //sphere.posistion.X = MathF.Sin((float)Raylib.GetTime() + sphere.rnd) + 3f;
                }
                
            }
        }

        static void Raytrace(ref Image image, out long mspf, out long mspd)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            visibleSpheres.Clear();
            foreach (var sphere in spheres)
            {
                if (sphere.DistanceToSurface(cameraPosition + cameraForward * nearPlane) <= farPlane)
                    visibleSpheres.Add(sphere);
            }


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

            Vector2 screenCoord = new Vector2(u / ((float)width * ratio), v / (float)height) - new Vector2(0.5f / ratio, 0.5f);
            Vector3 worldCoord = cameraForward * nearPlane + Vector3.UnitY * screenCoord.Y + screenCoord.X * cameraRight + cameraPosition;

            Vector3 direction = Vector3.Normalize(worldCoord - cameraPosition);

            while(distanceTraveled <= farPlane)
            {
                Vector3 pos = worldCoord + (direction * distanceTraveled);
                float lowestDst = float.MaxValue;

                for (int o = 0; o < visibleSpheres.Count; o++)
                {
                    float dst = visibleSpheres[o].DistanceToSurface(pos);
                    if (dst <= precision)
                    {
                        return visibleSpheres[o].shader.Fragment(new FragmentInput { position = pos, ray = direction, sphere = visibleSpheres[o], layer = 0 });
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
