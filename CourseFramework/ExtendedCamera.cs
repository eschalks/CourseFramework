using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using Math = Mogre.Math;

namespace CourseFramework
{
    class ExtendedCamera
    {
        protected SceneNode YawNode { get; private set; }
        protected SceneNode PitchNode { get; private set; }
        public Vector3 Position { get { return YawNode.Position; } set { YawNode.Position = value; } }

        public Camera Camera { get; private set; }

        public ExtendedCamera(SceneManager scene, string name)
        {
            // Create rotation scene nodes
            YawNode = scene.RootSceneNode.CreateChildSceneNode();
            PitchNode = YawNode.CreateChildSceneNode();

            Camera = scene.CreateCamera(name);
            Camera.NearClipDistance = 1;
            Camera.FarClipDistance = 5000;
            PitchNode.AttachObject(Camera);
        }

        public virtual void Yaw(Radian radians)
        {
            YawNode.Yaw(radians);
        }

        public virtual void Pitch(Radian radians)
        {
            PitchNode.Pitch(radians);
        }

        public virtual void Update(float timeSinceLastFrame)
        {
            
        }

        /// <summary>
        /// Determine velocity required to move an object into the direction the camera is facing.
        /// </summary>
        /// <param name="distance">The distance to move.</param>
        /// <param name="offset">An offset to apply to the current direction of the camera.</param>
        public Vector3 GetRelativeVelocity(float distance, Radian offset)
        {
            var angle = YawNode.Orientation.GetYaw() + offset;
            var dx = Math.Sin(angle);
            var dz = Math.Cos(angle);
            return new Vector3(dx, 0, dz);
        }
    }
}
