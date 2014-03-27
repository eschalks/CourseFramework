using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using Math = Mogre.Math;

namespace CourseFramework
{
    class FirstPersonCamera : ExtendedCamera
    {
        private readonly SceneNode rightNode;
        private readonly SceneNode leftNode;


        public FirstPersonCamera(SceneManager scene, string name) : base(scene, name)
        {
            // Create weapon nodes
            rightNode = PitchNode.CreateChildSceneNode();
            leftNode = PitchNode.CreateChildSceneNode();
        }


    }
}
