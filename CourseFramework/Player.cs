using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;

namespace SplitShooter
{
    class Player : IDisposable
    {
        public Camera Camera { get; private set; }
        public Viewport Viewport { get; private set; }
        public Entity Entity { get; private set; }
        public int Number { get; private set; }
        public Vector3 Position { get { return node.Position; } set { node.Position = value; } }

        public IPlayerController Controller { get; set; }

        private SceneManager scene;
        private RenderWindow window;
        private SceneNode entityNode;
        private SceneNode cameraPitchNode;
        private SceneNode cameraYawNode;
        private Entity camEntity;
        private Bone pitchBone;
        private Vector3 velocity;
        private SceneNode node;


        private const float MaxPitch = Mogre.Math.PI/4; // 45 degrees
        private const float MinPitch = -MaxPitch;

        public Player(SceneManager scene, RenderWindow window, string meshName, int number)
        {
            this.scene = scene;
            this.window = window;
            Number = number;

            Camera = scene.CreateCamera(string.Format("Player{0}Camera", number));
            Camera.AutoAspectRatio = true;
            Camera.NearClipDistance = 1;
            Camera.FarClipDistance = 1000;
            window.PreViewportUpdate += OnPreViewportUpdate;
            window.PostViewportUpdate += OnPostViewportUpdate;

            Viewport = window.AddViewport(Camera, number);
            
            Entity = scene.CreateEntity(meshName);
            node = scene.RootSceneNode.CreateChildSceneNode();

            // Ninja notes
            // Joint11 = Sword/Hand
            // Joint12 = Sword/Hand
            // Joint7 = Head
            // Joint8 = Head
            // Joint16 = Left Arm
            // Joint21 = Right Foot

            Console.WriteLine("---{0}---", meshName);
            pitchBone = Entity.Skeleton.GetBone(6);
            pitchBone.SetManuallyControlled(true);
            Console.WriteLine("{0} = {1}", pitchBone.Handle, pitchBone.Name);

            camEntity = scene.CreateEntity(SceneManager.PrefabType.PT_SPHERE);

            entityNode = node.CreateChildSceneNode();
            entityNode.SetScale(0.3f, 0.3f, 0.3f);
            entityNode.AttachObject(Entity);
            

            cameraYawNode = node.CreateChildSceneNode();
            cameraPitchNode = cameraYawNode.CreateChildSceneNode();
            cameraPitchNode.AttachObject(Camera);
            //cameraYawNode.AttachObject(camEntity);
            // Position camera at eye height
            cameraYawNode.Position = new Vector3(0, 57, 0);
        }

        void OnPreViewportUpdate(RenderTargetViewportEvent_NativePtr evt)
        {
            if (evt.source == Viewport)
                entityNode.SetVisible(false);
        }
        private void OnPostViewportUpdate(RenderTargetViewportEvent_NativePtr evt)
        {
            if (evt.source == Viewport)
                entityNode.SetVisible(true);
        }


        public void Yaw(Radian radians)
        {
            entityNode.Yaw(radians);
            cameraYawNode.Yaw(radians);
        }

        public void Pitch(Radian radians)
        {
            
            var newPitch = cameraPitchNode.Orientation.Pitch + radians;
            var val = newPitch.ValueRadians;
            if (val > MaxPitch)
                newPitch = new Radian(MaxPitch);
            else if (val < MinPitch)
                newPitch = new Radian(MinPitch);


            radians = newPitch - cameraPitchNode.Orientation.Pitch;
            cameraPitchNode.Pitch(radians);
            pitchBone.Pitch(radians);

        }

        public void Update(float dt)
        {
            velocity = Vector3.ZERO;
            if (Controller != null)
                Controller.Update(this, dt);
            node.Translate(velocity * dt);
        }

        public void Move(Radian angle, float pressure)
        {
            if (pressure < 0)
                pressure = 0;
            else if (pressure > 1)
                pressure = 1;
            angle += cameraYawNode.Orientation.GetYaw();
            var z = Mogre.Math.Cos(angle) * pressure;
            var x = Mogre.Math.Sin(angle) * pressure;
            velocity = new Vector3(x * 150, 0, z * 150);
        }

        public void Dispose()
        {
            scene.DestroyEntity(Entity);

            node.RemoveAndDestroyAllChildren();
            scene.RootSceneNode.RemoveChild(node);
            scene.DestroySceneNode(node);

            window.RemoveViewport(Number);
            window.PreViewportUpdate -= OnPreViewportUpdate;
            window.PostViewportUpdate -= OnPostViewportUpdate;

            scene.DestroyCamera(Camera);
        }
    }
}
