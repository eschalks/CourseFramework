using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using Math = Mogre.Math;

namespace CourseFramework
{
    class ThirdPersonCamera : ExtendedCamera
    {
        public float Distance { get; set; }
        public SceneNode Target { get; set; }
        public Vector3 TargetOffset { get; set; }

        private Radian yaw;
        private Radian pitch;

        public ThirdPersonCamera(SceneManager scene, string name) : base(scene, name)
        {
        }

        public override void Update(float timeSinceLastFrame)
        {

            base.Update(timeSinceLastFrame);
            if (Target == null)
                return;

            // Determine position
            var y = Math.Sin(pitch) * Distance;
            var dist = Math.Sqrt(Distance*Distance - y*y);
            var z = Math.Sin(yaw) * dist;
            var x = Math.Cos(yaw) * dist;
            Position = Target.Position + new Vector3(x, y, z) + TargetOffset;

            // Face camera towards target
            var q = new Quaternion();
            //var dz = Node.Position.z - Target.z;
            //var dy = Node.Position.y - Target.y;
            //var dx = Node.Position.x - Target.x;
            q.FromAngleAxis(Math.ATan2(x, z), Vector3.UNIT_Y);
            YawNode.Orientation = q;

            q = new Quaternion();
            q.FromAngleAxis(-pitch, Vector3.UNIT_X);
            PitchNode.Orientation = q;

        }
        public override void Yaw(Radian radians)
        {
            yaw += radians;
        }

        public override void Pitch(Radian radians)
        {
            pitch += radians;
            if (pitch >= Math.HALF_PI)
                pitch = Math.HALF_PI - .01f;
            //else if (pitch <= -Math.HALF_PI)
            //  pitch = -Math.HALF_PI - .01f;
            else if (pitch < 0)
                pitch = 0;
        }
    }
}
