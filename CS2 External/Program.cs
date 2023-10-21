using ClickableTransparentOverlay;
using CS2_External_Cheat;
using Swed64;
using System.Numerics;
using ImGuiNET;
using System.Runtime.InteropServices;
using CS2_External;

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
        ImDrawListPtr drawList;

        Entity localPlayer = new Entity();
        List<Entity> entityList = new List<Entity>();
        List<Entity> enemyTeam = new List<Entity>();
        List<Entity> playerTeam = new List<Entity>();

        IntPtr client;

        // ImGui stuff
        // global colors

        Vector4 teamcolor = new Vector4(0, 0, 1, 1); // RGBA, Blue teammates
        Vector4 enemycolor = new Vector4(1, 0, 0, 1); // RGBA, Red enemies
        Vector4 healthBarcolor = new Vector4(0, 1, 0, 1); // RGBA, Green healthbar
        Vector4 healthBarTextcolor = new Vector4(0, 0, 0, 1); // RGBA, black text

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
        bool enableTeamDistance = true;

        bool enableEnemyLine = true;
        bool enableEnemyBox = true;
        bool enableEnemyDot = true;
        bool enableEnemyHealthBar = true;
        bool enableEnemyDistance = true;


        protected override void Render()
        {
            // only render stuff here
            DrawMenu();
            DrawOverlay();
            Esp();
            ImGui.End();
        }

        void Esp()
        {
            drawList = ImGui.GetWindowDrawList(); // Important to get the overlay

            if (enableESP)
            {
                try // bad fix for stuff breaking but whatever it works
                {
                    foreach (var entity in entityList)
                    {
                        if (entity.teamNum == localPlayer.teamNum)
                        {
                            DrawVisuals(entity, teamcolor, enableTeamLine, enableTeamBox, enableTeamDot, enableTeamHealthBar, enableTeamDistance);
                        }
                        else
                        {
                            DrawVisuals(entity, enemycolor, enableEnemyLine, enableEnemyBox, enableEnemyDot, enableEnemyHealthBar, enableEnemyDistance);
                        }
                    }
                }
                catch
                {

                }
            }
        }

        void DrawVisuals(Entity entity, Vector4 color, bool line, bool box, bool dot, bool healthBar, bool distance)
        {
            // check if 2d position is valid
            if (IsPixelInsideScreen(entity.originScreenPosition))
            {
                // convert our colors from imgui to units

                uint uintColor = ImGui.ColorConvertFloat4ToU32(color);
                uint uiintHealthTextColor = ImGui.ColorConvertFloat4ToU32(healthBarTextcolor);
                uint uintHealthBarColor = ImGui.ColorConvertFloat4ToU32(healthBarcolor);

                // Calculate box attributes

                Vector2 boxWidth = new Vector2((entity.originScreenPosition.Y - entity.absScreenPosition.Y) / 2, 0f); // divide height by 2 simulate width
                Vector2 boxStart = Vector2.Subtract(entity.absScreenPosition, boxWidth); // get top left corner of box
                Vector2 boxEnd = Vector2.Add(entity.originScreenPosition, boxWidth); // get bottom right corner of box

                // Calculate health bar stuff

                float barPercentage = entity.health / 100f; // calculate percentage of health
                Vector2 healthBarHeight = new Vector2(0, barPercentage * (entity.originScreenPosition.Y - entity.absScreenPosition.Y)); // calculate height of health bar by multiplying percentage by height of character
                Vector2 healthBarStart = Vector2.Subtract(Vector2.Subtract(entity.originScreenPosition, boxWidth), healthBarHeight); // get position next to the box using box width
                Vector2 heathBarEnd = Vector2.Subtract(entity.originScreenPosition, Vector2.Add(boxWidth, new Vector2(-4, 0))); // get bottom right end of the bar. The -4 is width of the bar.

                // Finally draw
                
                if (line)
                {
                    drawList.AddLine(lineOrigin, entity.originScreenPosition, uintColor, 3); // draw line to feet of entities

                }   
                if (box)
                {
                    drawList.AddRect(boxStart, boxEnd, uintColor, 3); // draw box around entities
                }
                if (dot)
                {
                    drawList.AddCircleFilled(entity.originScreenPosition, 5, uintColor); // draw dot on entities
                }
                if (healthBar)
                {
                    drawList.AddText(healthBarStart, uiintHealthTextColor, $"HP: {entity.health}"); // draw health text
                    drawList.AddRectFilled(healthBarStart, heathBarEnd, uintHealthBarColor, 3); // draw health bar
                }
            }

        }

        bool IsPixelInsideScreen(Vector2 pixel)
        {
            // Check game window bounds
            return pixel.X > windowLocation.X && pixel.X < windowLocation.X + windowSize.X && pixel.Y > windowLocation.Y && pixel.Y < windowLocation.Y + windowSize.Y;
        }

        ViewMatrix ReadMatrix(IntPtr matrixAddress)
        {
            var viewMatrix = new ViewMatrix();
            var floatMatrix = swed.ReadMatrix(matrixAddress);

            // convert floats to our own viematrix type

            viewMatrix.m11 = floatMatrix[0];
            viewMatrix.m12 = floatMatrix[1];
            viewMatrix.m13 = floatMatrix[2];
            viewMatrix.m14 = floatMatrix[3];

            viewMatrix.m21 = floatMatrix[4];
            viewMatrix.m22 = floatMatrix[5];
            viewMatrix.m23 = floatMatrix[6];
            viewMatrix.m24 = floatMatrix[7];

            viewMatrix.m31 = floatMatrix[8];
            viewMatrix.m32 = floatMatrix[9];
            viewMatrix.m33 = floatMatrix[10];
            viewMatrix.m34 = floatMatrix[11];

            viewMatrix.m41 = floatMatrix[12];
            viewMatrix.m42 = floatMatrix[13];
            viewMatrix.m43 = floatMatrix[14];
            viewMatrix.m44 = floatMatrix[15];

            return viewMatrix;
        }

        Vector2 WorldToScreen(ViewMatrix matrix, Vector3 pos, int width, int height)
        {
            Vector2 screenCoordinates = new Vector2();

            // Calculate screenW

            float screenW = (matrix.m41 * pos.X) + (matrix.m42 * pos.Y) + (matrix.m43 * pos.Z) + matrix.m44;

            if (screenW > 0.001f) // check if entity is in front of us
            {
                // Calculate screen X
                float screenX = (matrix.m11 * pos.X) + (matrix.m12 * pos.Y) + (matrix.m13 * pos.Z) + matrix.m14;

                // Calculate screen Y
                float screenY = -(matrix.m21 * pos.X) + (matrix.m22 * pos.Y) + (matrix.m23 * pos.Z) + matrix.m24;

                // Calculate camera center
                float camX = width / 2;
                float camY = height / 2;

                // Perform perspective division and transformation

                float X = camX + (camX * screenX / screenW);
                float Y = camY - (camY * screenY / screenW);

                // return x and y

                screenCoordinates.X = X;
                screenCoordinates.Y = Y;
                return screenCoordinates;
            }
            else // return out of bounds vector if not in front of us
            {
                return new Vector2(-99, -99);
            }
        }

        void DrawMenu()
        {
            ImGui.Begin("CS2 External Cheat - https://github.com/Zckyy");

            if (ImGui.BeginTabBar("Tabs"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    ImGui.Checkbox("Enable ESP", ref enableESP);

                    
                    ImGui.Checkbox("Enable Team Box", ref enableTeamBox);
                    ImGui.Checkbox("Enable Team Distance (not working atm)", ref enableTeamDistance);
                    ImGui.Checkbox("Enable Team Dot", ref enableTeamDot);
                    ImGui.Checkbox("Enable Team Health Bar", ref enableTeamHealthBar);
                    ImGui.Checkbox("Enable Team Line", ref enableTeamLine);

                    ImGui.Checkbox("Enable Enemy Box", ref enableEnemyBox);
                    ImGui.Checkbox("Enable Enemy Distance (not working atm)", ref enableEnemyDistance);
                    ImGui.Checkbox("Enable Enemy Dot", ref enableEnemyDot);
                    ImGui.Checkbox("Enable Enemy Health Bar", ref enableEnemyHealthBar);
                    ImGui.Checkbox("Enable Enemy Line", ref enableEnemyLine);

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("colors"))
                {
                    // team colors
                    ImGui.ColorPicker4("Team color", ref teamcolor);
                    ImGui.Checkbox("Team Snap Line", ref enableTeamLine);
                    ImGui.Checkbox("Team Box", ref enableTeamBox);
                    ImGui.Checkbox("Team Dot", ref enableTeamDot);
                    ImGui.Checkbox("Team Health Bar", ref enableTeamHealthBar);

                    // enemy colors
                    ImGui.ColorPicker4("Enemy color", ref enemycolor);
                    ImGui.Checkbox("Enemy Snap Line", ref enableEnemyLine);
                    ImGui.Checkbox("Enemy Box", ref enableEnemyBox);
                    ImGui.Checkbox("Enemy Dot", ref enableEnemyDot);
                    ImGui.Checkbox("Enemy Health Bar", ref enableEnemyHealthBar);

                    ImGui.EndTabItem();

                }

                // End the tab bar.
                ImGui.EndTabBar();
            }

            ImGui.End();
        }

        void DrawOverlay() // Draw new window over the game window
        {
            ImGui.SetNextWindowSize(windowSize);
            ImGui.SetNextWindowPos(windowLocation);
            ImGui.Begin("Overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }

        void MainLogic()
        {
            // Calculate window size and location so we can place overlay on top
            var window = GetWindowRect(swed.GetProcess().MainWindowHandle);
            windowLocation = new Vector2(window.left, window.top);
            windowSize = new Vector2(window.right - window.left, window.bottom - window.top);
            lineOrigin = new Vector2(windowLocation.X + windowSize.X / 2, window.bottom);
            windowCenter = new Vector2(lineOrigin.X, window.bottom - windowSize.Y / 2);

            client = swed.GetModuleBase("client.dll");

            while (true) // Always run
            {
                ReloadEntityList();
                Thread.Sleep(3);

                // Debugging
                //foreach (var entity in entityList) 
                //{
                //    Console.WriteLine(entity.origin);
                //    Console.WriteLine(entity.health);
                //}
            }
        }

        void ReloadEntityList()
        {
            entityList.Clear();
            playerTeam.Clear();
            enemyTeam.Clear();

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
            // 1d
            entity.health = swed.ReadInt(entity.address, offsets.health);
            entity.teamNum = swed.ReadInt(entity.address, offsets.teamNum);
            entity.origin = swed.ReadVec(entity.address, offsets.origin);

            // 3d
            entity.origin = swed.ReadVec(entity.address, offsets.origin);
            entity.viewOffset = new Vector3(0, 0, 65); // Simulate view offset (Height of character in game)
            entity.abs = Vector3.Add(entity.origin, entity.viewOffset);

            // 2d, have to calculate 3d before 2d

            var currentViewmatrix = ReadMatrix(client + offsets.ViewMatrix);
            entity.originScreenPosition = Vector2.Add(WorldToScreen(currentViewmatrix, entity.origin, (int)windowSize.X, (int)windowSize.Y), windowLocation);
            entity.absScreenPosition = Vector2.Add(WorldToScreen(currentViewmatrix, entity.abs, (int)windowSize.X, (int)windowSize.Y), windowLocation);

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