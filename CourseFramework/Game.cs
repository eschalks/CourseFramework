using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mogre;
using MOIS;
using Vector3 = Mogre.Vector3;
using Math = Mogre.Math;
using AwesomiumSharp;

namespace CourseFramework
{
    class Game : IDisposable
    {
        private Root root;
        private RenderWindow window;
        private SceneManager scene;
        private Viewport viewport;

        ExtendedCamera camera;

        private InputManager input;
        private Keyboard keyboard;
        private Mouse mouse;
        private JoyStick joyStick;

        private float elapsedTime;
        private const float TimeStep = 1/60.0f;

        private WebWindow gui;

        public void Start()
        {
            if (!InitializeOgre())
                return;

            InitializeInput();

            // Initialize Awesomium
            var webCoreCfg = new WebCoreConfig();
            webCoreCfg.DisableSameOriginPolicy = true;
            WebCore.Initialize(webCoreCfg);
            WebCore.BaseDirectory = "Media/gui";

            camera = new ThirdPersonCamera(scene, "PlayerCamera");
            //camera.Position = new Vector3(0, 0, 300);
            viewport = window.AddViewport(camera.Camera);

            CreateScene();

            gui = new WebWindow(0, 0, 200, 200);
            gui.LoadFile("hello.html");

            root.FrameRenderingQueued += OnFrameRenderingQueued;
            root.StartRendering();
        }

        private void InitializeInput()
        {
            int hWnd;
            window.GetCustomAttribute("WINDOW", out hWnd);
            input = InputManager.CreateInputSystem((uint) hWnd);
            mouse = (Mouse) input.CreateInputObject(MOIS.Type.OISMouse, false);
            keyboard = (Keyboard)input.CreateInputObject(MOIS.Type.OISKeyboard, false);

            if (input.GetNumberOfDevices(MOIS.Type.OISJoyStick) > 0)
            {
                joyStick = (JoyStick)input.CreateInputObject(MOIS.Type.OISJoyStick, false);
            }
        }

        /// <summary>
        /// Initialises OGRE and creates all important OGRE objects
        /// </summary>
        /// <returns>Whether the initialization was completed successfully.</returns>
        private bool InitializeOgre()
        {
            // Load all plugins in the Plugins folder
            root = new Root("", "SplitShooter.cfg", "SplitShooter.log");
            var dir = new DirectoryInfo("Plugins");
            foreach (var file in dir.EnumerateFiles("*.dll"))
            {
                root.LoadPlugin(file.FullName);
            }

            // Show/retrieve configuration
            if (!root.RestoreConfig() && // true if a config file was found and successfully parsed
                !root.ShowConfigDialog()) // true if the user presses OK in the configuration dialog)
            {
                // No configuration specified, end application
                return false;
            }

            // Initialise OGRE and create a window
            window = root.Initialise(true, "Ogre3D Game");
            scene = root.CreateSceneManager("OctreeSceneManager");

            // Load all resources that are in the Media folder
            var resMgr = ResourceGroupManager.Singleton;
            resMgr.AddResourceLocation("Media", "FileSystem", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, true);
            resMgr.InitialiseAllResourceGroups();

            return true;
        }



        /// <summary>Geen i
        /// Called every frame by OGRE.
        /// All game logic happens in here.
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        bool OnFrameRenderingQueued(FrameEvent evt)
        {
            // Update in fixed time steps - this is required by the physics engine
            elapsedTime += evt.timeSinceLastFrame;
            while (elapsedTime > TimeStep)
            {
                WebCore.Update();

                // Update all input devices
                mouse.Capture();
                keyboard.Capture();
                if (joyStick != null)
                    joyStick.Capture();

                var ms = mouse.MouseState;
                camera.Yaw(-ms.X.rel * TimeStep);
                camera.Pitch(-ms.Y.rel * TimeStep);

                var tp = camera as ThirdPersonCamera;
                if (tp != null)
                {
                    tp.Distance += -ms.Z.rel * TimeStep * 8;
                    if (tp.Distance > 600)
                        tp.Distance = 600;
                    if (tp.Distance < 100)
                        tp.Distance = 100;
                }

                camera.Update(TimeStep);

                elapsedTime -= TimeStep;
            }

            return !window.IsClosed && !keyboard.IsKeyDown(KeyCode.KC_ESCAPE);
        }

        void CreateScene()
        {
            var robot = scene.CreateEntity("Robot.mesh");
            var robotNode = scene.RootSceneNode.CreateChildSceneNode();
            robotNode.Yaw(Math.PI);
            robotNode.AttachObject(robot);

            var cam = camera as ThirdPersonCamera;
            if (cam != null)
            {
                cam.Distance = 300;
                cam.Target = robotNode;
                cam.TargetOffset = new Vector3(0, 50, 0);
                cam.Update(0);
            }
        }

        public void Dispose()
        {
            gui.Dispose();
            root.Shutdown();
            root.Dispose();
        }

        public static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Start();
            }
        }
    }
}
