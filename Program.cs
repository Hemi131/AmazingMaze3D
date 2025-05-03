using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace AmazingMaze3D;

public class Program : GameWindow
{
    public static void Main(string[] args) => new Program(args).Run();

    string[] args;
    string mapName = "map1.txt";
    Viewport mainViewport;
    Camera mainCamera;
    Light light;
    Flashlight flashlight;
    Shader shader;
    List<Model> modelList;
    bool[,] wallHere;

    float lastToggle = 0f;
    int fps = 0;
    private List<double> frameTimes = new List<double>();
    private Stopwatch stopwatch = new Stopwatch();

    bool wireModel = false;

    Teleport teleport;
    Enemy enemy;


    public Program(string[] args) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize=new OpenTK.Mathematics.Vector2i(600,600), DepthBits = 24 }) { this.args = args; }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
        GL.DebugMessageCallback(DebugCallback, IntPtr.Zero);
        GL.Enable(EnableCap.DepthTest);
        GL.LineWidth(10);

        CursorState = CursorState.Grabbed;
        
        mainViewport = new Viewport()
        {
            Left = 0,
            Top = 0,
            Width = 1,
            Height = 1,
            window = this
        };

        modelList = new List<Model>();
        shader = new Shader("shaders/basic.vert", "shaders/basic2.frag");
        mainCamera = new Camera(mainViewport);
        light = new Light(new Vector3(25, -1, 25), 0f, new Vector3(0.2f, 0.1f, 0.1f), new Vector3(0f));
        flashlight = new Flashlight();
        teleport = new Teleport("t", light, mainCamera);

        WindowState = WindowState.Fullscreen;

        if (args.Length > 0) {
            mapName = args[0];
        }
        loadModels();

        stopwatch.Start();
    }

    private static void DebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
    {
        string msg = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length);
        Console.WriteLine($"OpenGL Debug Message: {msg}\nSource: {source}, Type: {type}, Severity: {severity}, ID: {id}");
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        mainViewport.Set();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (wireModel)
        {
            var view = mainCamera.View;
            var direction = new Vector3(view[0, 2], view[1, 2], view[2, 2]);

            direction = direction / new Vector3(Math.Abs(direction.X), Math.Abs(direction.Y), Math.Abs(direction.X)) * 0.01f;

            mainCamera.pos.X += direction.X;
            mainCamera.pos.Z += direction.Z;
            mainCamera.pos.Y += direction.Y;

            foreach (var model in modelList)
            {
                model.Draw(mainCamera, flashlight, light, false);
            }

            GL.Clear(ClearBufferMask.ColorBufferBit);

            mainCamera.pos.X -= direction.X;
            mainCamera.pos.Z -= direction.Z;
            mainCamera.pos.Y -= direction.Y;

            foreach (var model in modelList)
            {
                model.Draw(mainCamera, flashlight, light, true);
            }

        } else {
            foreach (var model in modelList)
            {
                model.Draw(mainCamera, flashlight, light, false);
            }

        }

        enemy.Draw(mainCamera, flashlight, light, false);

        SwapBuffers();

        double currentTime = stopwatch.Elapsed.TotalSeconds;
        frameTimes.Add(currentTime);

        while (frameTimes.Count > 0 && currentTime - frameTimes[0] > 1.0)
        {
            frameTimes.RemoveAt(0);
        }

        fps = frameTimes.Count;

        Title = $"FPS: {fps}";
    }

    float timetotal  = 0;
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        Vector2 direction = new Vector2(0, 0);
        float running = 1f;
        if (lastToggle > 0) { lastToggle -= (float)args.Time; }

        if (KeyboardState.IsKeyDown(Keys.W)) direction += Vector2.UnitY;
        if (KeyboardState.IsKeyDown(Keys.S)) direction -= Vector2.UnitY;
        if (KeyboardState.IsKeyDown(Keys.A)) direction -= Vector2.UnitX;
        if (KeyboardState.IsKeyDown(Keys.D)) direction += Vector2.UnitX;
        if (KeyboardState.IsKeyDown(Keys.LeftShift)) running = 2;
        if (KeyboardState.IsKeyDown(Keys.F) && lastToggle <= 0f) { flashlight.Toggle(); lastToggle = 0.2f; }
        if (KeyboardState.IsKeyDown(Keys.F11) && lastToggle <= 0f) { WindowState = WindowState == WindowState.Fullscreen ? WindowState.Maximized : WindowState.Fullscreen; lastToggle = 0.2f; }
        if (KeyboardState.IsKeyDown(Keys.F10) && lastToggle <= 0f) { lastToggle = 0.2f; if (wireModel) { light.lightOverdose = new Vector3(0f); wireModel = false; } else { light.lightOverdose = new Vector3(1f); wireModel = true; } }
        if (KeyboardState.IsKeyDown(Keys.E)) { teleport.teleport(); }

        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();


        //Console.WriteLine(light.position);

        var len = direction.Length;
        if (len > 0)
        {
            direction *= (float)args.Time/len*running;
            mainCamera.Move(direction.X, direction.Y, wallHere);
        }

        if(mouseDelta.LengthSquared > 0.00001)
        {
            mainCamera.RotateX(mouseDelta.Y*(float)args.Time);
            mainCamera.RotateY(mouseDelta.X*(float)args.Time);
            mouseDelta = Vector2.Zero;
        }

        flashlight.UpdatePositionAndDirection(mainCamera);
        teleport.teleportAnimation((float)args.Time);
        enemy.Animate(wallHere, (float)args.Time, mainCamera);
    }

    Vector2 mouseDelta;
    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        mouseDelta += new Vector2(e.DeltaX, e.DeltaY);
    }
    private void loadModels()
    {
        MyObjects myObjects = new MyObjects(shader);

        enemy = myObjects.getGreenGhost(new Vector3(7f, 0f, 29f));

        Console.WriteLine("Loading map: " + mapName);
        string[] lines = File.ReadAllLines("maps/" + mapName);

        string[] rozmery = lines[0].Split("x");
        int rozmer1 = int.Parse(rozmery[0]);
        int rozmer2 = int.Parse(rozmery[1]);

        wallHere = new bool[rozmer1+2, rozmer2+2];
        wallHere.Initialize();
        for (int i = 0; i < rozmer1 + 2; i++)
        {
            wallHere[i, 0] = true;
            wallHere[i, rozmer2 + 1] = true;
        }
        for (int i = 0; i < rozmer2 + 2; i++)
        {
            wallHere[0, i] = true;
            wallHere[rozmer1 + 1, i] = true;
        }

        // Rozměry bloku
        float blockWidth = 2.0f;
        float blockHeight = 3.0f;
        float blockDepth = 2.0f;

        char curr;
        for (int y = 1; y < lines.Length; y++)
        {
            for (int x = 1; x < lines[y].Length + 1; x++)
            {
                curr = lines[y][x - 1];

                Vector3 blockPos = new Vector3(x * blockWidth, 0, y * blockDepth);
                switch (curr)
                {
                    case 'x':
                        wallHere[x, y] = true;

                        modelList.Add(myObjects.getWall1(blockPos));
                        break;
                    case 'y':
                        wallHere[x, y] = true;
                        modelList.Add(myObjects.getWall1Blood(blockPos));
                        break;
                    case ' ':
                        modelList.Add(myObjects.getFreeSpaceTop1(blockPos));
                        modelList.Add(myObjects.getFreeSpaceBottom1(blockPos));
                        break;

                    case '@':
                        mainCamera.pos = new Vector3(x * blockWidth, 1.7f, y * blockDepth);

                        modelList.Add(myObjects.getFreeSpaceTop1(blockPos));
                        modelList.Add(myObjects.getFreeSpaceBottom1(blockPos));
                        break;
                    case 's':
                        wallHere[x, y] = true;
                        modelList.Add(myObjects.getWallPic1L(blockPos));
                        break;
                    case 't':
                        wallHere[x, y] = true;
                        modelList.Add(myObjects.getWallPic1R(blockPos));
                        break;
                    case 'N':
                        modelList.Add(myObjects.getFreeSpaceTop1(blockPos));
                        modelList.Add(myObjects.getFloorTeleport(blockPos));

                        teleport.setTeleportPos(new Vector3(x * blockWidth, 1.7f, y * blockDepth));

                        break;

                    default:
                        modelList.Add(myObjects.getFreeSpaceTop1(blockPos));
                        modelList.Add(myObjects.getFreeSpaceBottom1(blockPos));
                        break;

                }
            }
        }

        //for (int i = 0; i < rozmer2 + 2; i++)
        //{
        //    for (int j = 0; j < rozmer1 + 2; j++)
        //    {
        //        Console.Write(wallHere[j, i] ? "x" : " ");
        //    }
        //    Console.WriteLine();
        //}
    }

    private static HashSet<string> _extensions;

    public static bool IsExtensionSupported(string extensionName)
    {
        if (_extensions == null)
        {
            _extensions = new HashSet<string>();
            GL.GetInteger(GetPName.NumExtensions, out int extensionCount);

            for (int i = 0; i < extensionCount; i++)
            {
                string ext = GL.GetString(StringNameIndexed.Extensions, i);
                if (ext != null)
                    _extensions.Add(ext);
            }
        }

        return _extensions.Contains(extensionName);
    }

}

