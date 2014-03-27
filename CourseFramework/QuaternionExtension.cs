using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;

namespace CourseFramework
{
    static class QuaternionExtension
    {
        /// <summary>
        /// MOGRE's version of GetYaw() appears to be bugged.
        /// This method contains a direct port of the original C++ version.
        /// </summary>
        public static Radian GetYaw(this Quaternion self)
        {
            var fTx = 2.0f * self.x;
            var fTy = 2.0f * self.y;
            var fTz = 2.0f * self.z;
            var fTwy = fTy * self.w;
            var fTxx = fTx * self.x;
            var fTxz = fTz * self.x;
            var fTyy = fTy * self.y;
            return new Radian(Mogre.Math.ATan2(fTxz + fTwy, 1.0f - (fTxx + fTyy)));
        }
    }
}
