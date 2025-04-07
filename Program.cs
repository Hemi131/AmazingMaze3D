using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
    float lastFPStime = 0f;
    int fps = 0;


    public Program(string[] args) : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize=new OpenTK.Mathematics.Vector2i(600,600) }) { this.args = args; }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
        GL.DebugMessageCallback(DebugCallback, IntPtr.Zero);
        GL.Enable(EnableCap.DepthTest);

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
        shader = new Shader("basic.vert", "basic2.frag");
        mainCamera = new Camera(mainViewport);
        light = new Light(new Vector3(25, -1, 25), 0f, new Vector3(0.2f, 0.1f, 0.1f));
        flashlight = new Flashlight();

        WindowState = WindowState.Fullscreen;

        if (args.Length > 0) {
            mapName = args[0];
        }
        loadModels();
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

        foreach (var model in modelList)
        {
            model.Draw(mainCamera, flashlight, light);
        }

        SwapBuffers();
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

        fps++;
        lastFPStime += (float)args.Time;
        if (lastFPStime > 1f)
        {
            Title = $"FPS: {fps}";
            fps = 0;
            lastFPStime = 0f;
        }
    }

    Vector2 mouseDelta;
    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        mouseDelta += new Vector2(e.DeltaX, e.DeltaY);
    }
    private void loadModels()
    {
        SimpleObj wall1 = SimpleObj.LoadObj("wall1.obj");
        SimpleObj freeSpace = SimpleObj.LoadObj("freeSpace.obj");

        Console.WriteLine("Loading map: " + mapName);
        string[] lines = File.ReadAllLines(mapName);

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

                Model block;
                switch (curr)
                {
                    case 'x':
                        block = new Model(new Vector3(x * blockWidth, 0, y * blockDepth), wall1, shader,
                        new Vector3(0.01f, 0.1f, 0.8f),
                        new Vector3(0.8f),
                        32.0f);

                        wallHere[x, y] = true;
                        modelList.Add(block);
                        break;
                    case ' ':
                        block = new Model(new Vector3(x * blockWidth, 0, y * blockDepth), freeSpace, shader,
                        new Vector3(0.01f, 0.8f, 0.01f),
                        new Vector3(0.8f),
                        32.0f);

                        modelList.Add(block);
                        break;

                    case '@':
                        mainCamera.pos = new Vector3(x * blockWidth, 1.7f, y * blockDepth);

                        block = new Model(new Vector3(x * blockWidth, 0, y * blockDepth), freeSpace, shader,
                        new Vector3(0.01f, 0.8f, 0.01f),
                        new Vector3(0.8f),
                        32.0f);

                        modelList.Add(block);
                        break;

                    default:
                        block = new Model(new Vector3(x * blockWidth, 0, y * blockDepth), freeSpace, shader,
                        new Vector3(0.01f, 0.8f, 0.01f),
                        new Vector3(0.8f),
                        32.0f);

                        modelList.Add(block);
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

}