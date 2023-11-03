using CS2_External;
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
        public int ViewMatrix = 0x1899070;
        public int ViewAngle = 0x18F8088;
        public int localPlayer = 0x16B6320;
        public int entityList = 0x16C2FC0;
        public int dwForceJump = 0x169F400;
        public int m_iIDEntIndex = 0x1534;
        public int m_bOnGroundLastTick = 0x2290;
        public int current_time = 0x2C;

        // entity attributes
        public int teamNum = 0x3BF;
        public int jumpFlag = 0x3c8;
        public int health = 0x32c;
        public int origin = 0xCD8;
        public int groundFlag = 0x3F8;

        // client.dll
        public int dwEntityList = 0x17AA8E8;
        public int dwForceForward = 0x16AF4D0; // for you  @SilencedCaosk
        public int dwGameRules = 0x1806F48;
        public int dwGlobalVars = 0x16AB2D0;
        public int dwLocalPlayerController = 0x17F9C08;
        public int dwLocalPlayerPawn = 0x16B6320;
        public int dwPlantedC4 = 0x189DAF8;
        public int dwViewAngles = 0x18F8088;
        public int dwViewMatrix = 0x1899070;

        // inputsystem.dll
        public int dwInputSystem = 0x35770;

        // engine2.dll
        public int dwBuildNumber = 0x488514;

        // Entity
        public int m_pClippingWeapon = 0x12A0;
        public int m_pItemServices = 0x10B0;
        public int m_AttributeManager = 0x1040;
        public int m_hPlayerPawn = 0x7BC;
        public int m_hPawn = 0x5DC;
        public int m_lifeState = 0x330;
        public int m_pGameSceneNode = 0x310;
        public int m_iItemDefinitionIndex = 0x1BA;
        public int m_vecAbsOrigin = 0xC8;
        public int m_Item = 0x50;

        // EntityPawn
        public int m_iHealth = 0x32C;
        public int m_iTeamNum = 0x3BF;
        public int m_vOldOrigin = 0x1224;
        public int m_bIsScoped = 0x1398;
        public int m_bIsDefusing = 0x13A0;
        public int m_bIsGrabbingHostage = 0x13A1;
        public int m_flFlashDuration = 0x1460;
        public int m_ArmorValue = 0x1500;
        public int m_angEyeAngles = 0x1508;

        // EntityController
        public int m_iszPlayerName = 0x610;

        // EntityItems
        public int m_bHasDefuser = 0x40;
        public int m_bHasHelmet = 0x41;

        // CSGameRules
        public int m_bFreezePeriod = 0x30;
        public int m_bWarmupPeriod = 0x31;
        public int m_bHasMatchStarted = 0xA4;
        public int m_bBombPlanted = 0x9DD;

        // PlantedC4
        public int m_nBombSite = 0xE84;
        public int m_flC4Blow = 0xEB0;
    }
}
