using ClickableTransparentOverlay;
using CS2_External_Cheat;
using Swed64;
using System.Numerics;
using ImGuiNET;
using System.Runtime.InteropServices;

namespace CS2EXTERNAL
{
    class Program : Overlay
    {
        // imports and struct
        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        public RECT GetWindowRect(IntPtr hWnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hWnd, out rect);
            return rect;
        }

        // important variables

        Swed swed = new Swed("cs2");
        Offsets offsets = new Offsets();
        ImDrawListPtr ImDrawListPtr;

        Entity localPlayer = new Entity();
        List<Entity> entityList = new List<Entity>();
        List<Entity> enemyTeam = new List<Entity>();
        List<Entity> playerTeam = new List<Entity>();

        IntPtr client;

        // ImGui stuff
        // global colors

        Vector4 teamColour = new Vector4(0, 0, 1, 1); // RGBA, Blue teammates
        Vector4 enemyColour = new Vector4(1, 0, 0, 1); // RGBA, Red enemies
        Vector4 healthBarColour = new Vector4(0, 1, 0, 1); // RGBA, Green healthbar
        Vector4 healthBarTextColour = new Vector4(0, 0, 0, 1); // RGBA, black text

        // Screen variables, we update these later

        Vector2 windowLocation = new Vector2(0, 0);
        Vector2 windowSize = new Vector2(1920, 1080);
        Vector2 lineOrigin = new Vector2(1920 / 2, 1080);
        Vector2 windowCenter = new Vector2(1920 / 2, 1080 / 2);

        // ImGui checkboxes and stuff

        bool enableESP = true;

        bool enableTeamLine = true;
        bool enableTeamBox = true;
        bool enableTeamDot = true;
        bool enableTeamHealthBar = true;

        bool enableEnemyLine = true;
        bool enableEnemyBox = true;
        bool enableEnemyDot = true;
        bool enableHealthBar = true;


        protected override void Render()
        {
            // only render stuff here
            ImGui.SetNextWindowSize(new Vector2(400, 300));
            ImGui.Begin("CS2 External Cheat");
            DrawMenu();
        }

        void DrawMenu()
        {
            ImGui.Begin("CS2 External Cheat - https://github.com/Zckyy");

            if (ImGui.BeginTabBar("Tabs"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    ImGui.Checkbox("Enable ESP", ref enableESP);

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Colours"))
                {
                    // team colours
                    ImGui.ColorPicker4("Team Colour", ref teamColour);
                    ImGui.Checkbox("Team Snap Line", ref enableTeamLine);
                    ImGui.Checkbox("Team Box", ref enableTeamBox);
                    ImGui.Checkbox("Team Dot", ref enableTeamDot);
                    ImGui.Checkbox("Team Health Bar", ref enableTeamHealthBar);

                    // enemy colours
                    ImGui.ColorPicker4("Enemy Colour", ref enemyColour);
                    ImGui.Checkbox("Enemy Snap Line", ref enableEnemyLine);
                    ImGui.Checkbox("Enemy Box", ref enableEnemyBox);
                    ImGui.Checkbox("Enemy Dot", ref enableEnemyDot);
                    ImGui.Checkbox("Enemy Health Bar", ref enableHealthBar);

                    ImGui.EndTabItem();

                }

                // End the tab bar.
                ImGui.EndTabBar();
            }

            ImGui.End();
        }

        void MainLogic()
        {
            // Calculate window size and location so we can place overlay on top
            var window = GetWindowRect(swed.GetProcess().MainWindowHandle);
            windowLocation = new Vector2(window.left, window.top);
            windowSize = new Vector2(window.right - window.left, window.bottom - window.top);
            lineOrigin = new Vector2(windowLocation.X + windowLocation.X / 2, window.bottom);
            windowCenter = new Vector2(lineOrigin.X, window.bottom - windowSize.Y / 2);

            client = swed.GetModuleBase("client.dll");

            while (true) // Always run
            {
                ReloadEntityList();
                foreach (var entity in entityList) 
                {
                    Console.WriteLine(entity.origin);
                    Console.WriteLine(entity.health);
                }

                Thread.Sleep(2000);
            }
        }

        void ReloadEntityList()
        {
            entityList.Clear();

            localPlayer.address = swed.ReadPointer(client, offsets.localPlayer); // set the address so we can update it later
            UpdateEntity(localPlayer);
            updateEntityList();
        }   

        void updateEntityList() // handle all other entities here
        {
            for (int i = 0; i < 64; i++) // normall les than 64 entities
            {
                IntPtr tempEntityAddress = swed.ReadPointer(client, offsets.entityList + i * 0x8); // get the address of the entity

                if (tempEntityAddress == IntPtr.Zero)
                    continue;

                Entity entity = new Entity(); // create a new entity
                entity.address = tempEntityAddress;

                UpdateEntity(entity); // update the entity

                if (entity.health < 1 || entity.health > 100) // checking if entity is dead
                    continue;

                // Check if duplicate of the entity since we use goofy entity list
                if (!entityList.Any(x => x.origin.X == entity.origin.X))
                {
                    entityList.Add(entity);

                    // adding entity to team's lists
                    if (entity.teamNum == localPlayer.teamNum)
                    {
                        playerTeam.Add(entity);
                    }

                    else
                    {
                        enemyTeam.Add(entity);
                    }
                }
            }
        }

        void UpdateEntity(Entity entity)
        {
            entity.health = swed.ReadInt(entity.address, offsets.health);
            entity.origin = swed.ReadVec(entity.address, offsets.origin);
            entity.teamNum = swed.ReadInt(entity.address, offsets.teamNum);
        }

        static void Main(string[] args)
        {
            // run logic methods and more

            Program program = new Program();
            program.Start().Wait();

            Thread mainLogicThread = new Thread(program.MainLogic) { IsBackground = true };
            mainLogicThread.Start();
        }
    }
}