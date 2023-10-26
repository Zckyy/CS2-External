using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS2_External_Cheat
{
    public class Offsets
    {
        // client
        public int ViewMatrix = 0x1887730;
        public int ViewAngle = 0x18E6770;
        public int localPlayer = 0x1886C48;
        public int entityList = 0x16B28A0;
        public int dwForceJump = 0x169E360;

        // entity attributes
        public int teamNum = 0x3BF;
        public int jumpFlag = 0x3c8;
        public int health = 0x32c;
        public int origin = 0xCD8;
        public int groundFlag = 0x3F8;
    }
}
