using CS2_External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CS2_External_Cheat
{
    public class Entity
    {
        public IntPtr address { get; set; }
        public bool m_bOnGroundLastTick { get; set; }
        public int health { get; set; }
        public int teamNum { get; set; }
        public int jumpFlag { get; set; }
        public int m_iIDEntIndex { get; set; }
        public int dwGameRules { get; set; }
        public string m_iszPlayerName { get; set; }
        public float m_flFlashDuration { get; set; }
        public Vector3 origin { get; set; }
        public Vector3 abs { get; set; }
        public Vector3 viewOffset { get; set; }
        public Vector2 originScreenPosition { get; set; }
        public Vector2 absScreenPosition { get; set; }
    }
}
