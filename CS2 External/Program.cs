﻿using ClickableTransparentOverlay;
using CS2_External_Cheat;
using Swed64;
using System.Numerics;
using ImGuiNET;
using System.Runtime.InteropServices;
using CS2_External;
using System.Windows.Forms;

namespace CS2EXTERNAL
{
    class Program : Overlay
    {
        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
        }

        // imports and struct
        [DllImport("user32.dll")]
        static extern int GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

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

        // constants
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        const int MENU_HOTKEY = 0x70; // F1 Hotkey
        const int ESP_HOTKEY = 0x71; // F2 Hotkey
        const int PANIC_KEY = 0x73; // F4 Hotkey
        const int TRIGGER_KEY = 0x05; // X1 mouse button Hotkey

        // ImGui stuff

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

        bool showWindow = true;

        float r = 1.0f;
        float g = 1.0f;
        float b = 1.0f;

        bool killswitch = false;

        bool enableESP = true;
        //bool enableAimbot = false;
        //bool enableAimClosestToCrosshair = false;

        bool enableTeamLine = false;
        bool enableTeamBox = true;
        bool enableTeamDot = true;
        bool enableTeamHealthBar = true;
        bool enableTeamDistance = true;

        bool enableEnemyLine = false;
        bool enableEnemyBox = true;
        bool enableEnemyDot = true;
        bool enableEnemyHealthBar = true;
        bool enableEnemyDistance = true;

        bool IsAimingAtEnemy()
        {
            if (localPlayer.m_iIDEntIndex > -1)
            {
                return true;
            }

            return false;
        }

        void LeftClick(int x, int y)
        {
            Cursor.Position = new System.Drawing.Point(x, y);
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        }

        protected override void Render()
        {
            // only render stuff here

            if (!showWindow)
            {
                DrawMenu();
            }

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
                    if (entityList.Any())
                    {
                        foreach (var entity in entityList)
                        {
                            if (entity != null) 
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
                float screenY = (matrix.m21 * pos.X) + (matrix.m22 * pos.Y) + (matrix.m23 * pos.Z) + matrix.m24;

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
            try
            {
                // Styling
                RGBUpdate();
                ImGuiStylePtr style = ImGui.GetStyle();
                ImGuiIOPtr io = ImGui.GetIO();

                style.WindowRounding = 5f;
                style.WindowBorderSize = 3f;
                style.WindowMinSize = new Vector2(700, 700);

                // Background and border
                style.Colors[(int)ImGuiCol.Border] = new Vector4(r, b, g, 0.5f);
                style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0, 0, 0, 1f);
                style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0, 0, 0, 1f);
                style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0, 0, 0, 1f);
                style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0, 0, 0, 1f);
                style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(255, 255, 255, 1f);
                style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(200, 200, 200, 1f);
                // Text , buttons, checkboxes
                style.Colors[(int)ImGuiCol.Text] = new Vector4(255, 255, 255, 1f);
                style.Colors[(int)ImGuiCol.Button] = new Vector4(255, 255, 255, 1f);
                style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(200, 200, 200, 0.6f);
                style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(255, 0, 0, 0.5f);
                style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(255, 255, 255, 0.5f);
                // Tabs
                style.Colors[(int)ImGuiCol.Tab] = new Vector4(0, 0, 0, 1f);
                style.Colors[(int)ImGuiCol.TabActive] = new Vector4(r, g, b, 1f);

                // Creating the actual menu

                ImGui.Begin("CS2 External Cheat - https://github.com/Zckyy", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize);

                if (ImGui.BeginTabBar("Tabs"))
                {
                    if (ImGui.BeginTabItem("ESP"))
                    {
                        ImGui.Text("ESP");
                        ImGui.Text($"{localPlayer.dwPlantedC4}");

                        ImGui.Checkbox("Enable ESP", ref enableESP);
                        ShowContextMenuTooltip("Toggles the Wallhacks");
                        ImGui.Separator();

                        ImGui.Text("Team");

                        ImGui.Checkbox("Enable Team Box", ref enableTeamBox);
                        ImGui.Checkbox("Enable Team Distance (not working atm)", ref enableTeamDistance);
                        ImGui.Checkbox("Enable Team Dot", ref enableTeamDot);
                        ImGui.Checkbox("Enable Team Health Bar", ref enableTeamHealthBar);
                        ImGui.Checkbox("Enable Team Line", ref enableTeamLine);
                        ShowContextMenuTooltip("Toggles Snaplines, these lines start from the bottom center of the game window");
                        ImGui.Separator();

                        ImGui.Text("Enemy");

                        ImGui.Checkbox("Enable Enemy Box", ref enableEnemyBox);
                        ImGui.Checkbox("Enable Enemy Distance (not working atm)", ref enableEnemyDistance);
                        ImGui.Checkbox("Enable Enemy Dot", ref enableEnemyDot);
                        ImGui.Checkbox("Enable Enemy Health Bar", ref enableEnemyHealthBar);
                        ImGui.Checkbox("Enable Enemy Line", ref enableEnemyLine);
                        ShowContextMenuTooltip("Toggles Snaplines, these lines start from the bottom center of the game window");

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Colors"))
                    {

                        // team colors
                        ImGui.Text("Team");

                        ImGui.ColorPicker4("Team color", ref teamcolor);
                        ImGui.Checkbox("Team Snap Line", ref enableTeamLine);
                        ImGui.Checkbox("Team Box", ref enableTeamBox);
                        ImGui.Checkbox("Team Dot", ref enableTeamDot);
                        ImGui.Checkbox("Team Health Bar", ref enableTeamHealthBar);
                        ImGui.Separator();

                        // enemy colors
                        ImGui.Text("Enemy");

                        ImGui.ColorPicker4("Enemy color", ref enemycolor);
                        ImGui.Checkbox("Enemy Snap Line", ref enableEnemyLine);
                        ImGui.Checkbox("Enemy Box", ref enableEnemyBox);
                        ImGui.Checkbox("Enemy Dot", ref enableEnemyDot);
                        ImGui.Checkbox("Enemy Health Bar", ref enableEnemyHealthBar);
                        ImGui.Separator();

                        ImGui.EndTabItem();

                    }

                    if (ImGui.BeginTabItem("Misc"))
                    {
                        ImGui.Checkbox("Killsitch (Closes the Cheat)", ref killswitch);

                        ImGui.EndTabItem();
                    }



                    // End the tab bar.
                    ImGui.EndTabBar();
                }

                ImGui.End();
            }
            catch
            {

            }
        }

        void RGBUpdate()
        {
            const float step = 0.005f;

            if (r == 1.0f && g >= 0.0f && b <= 0.0f)
            {
                g += step;
                if (g > 1.0f)
                {
                    g = 1.0f;
                    r -= step;
                }
            }
            else if (r <= 1.0f && g >= 1.0f && b == 0.0f)
            {
                r -= step;
                if (r < 0.0f)
                {
                    r = 0.0f;
                    b += step;
                }
            }
            else if (r <= 0.0f && g == 1.0f && b >= 0.0f)
            {
                b += step;
                if (b > 1.0f)
                {
                    b = 1.0f;
                    g -= step;
                }
            }
            else if (r == 0.0f && g <= 1.0f && b >= 1.0f)
            {
                g -= step;
                if (g < 0.0f)
                {
                    g = 0.0f;
                    r += step;
                }
            }
            else if (r >= 0.0f && g <= 0.0f && b == 1.0f)
            {
                r += step;
                if (r > 1.0f)
                {
                    r = 1.0f;
                    b -= step;
                }
            }
            else if (r >= 1.0f && g >= 0.0f && b <= 1.0f)
            {
                b -= step;
                if (b < 0.0f)
                {
                    b = 0.0f;
                    g += step;
                }
            }

            Thread.Sleep(1);
        }

        public static void ShowContextMenuTooltip(string tooltipText)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextColored(new Vector4(255, 255, 255, 1),$"{tooltipText}");
                ImGui.EndTooltip();
            }
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
                Thread.Sleep(1);

                if (IsAimingAtEnemy() & GetAsyncKeyState(TRIGGER_KEY) > 1)
                {
                    LeftClick((int)windowCenter.X, (int)windowCenter.Y);
                    Thread.Sleep(40);
                }

                if (killswitch == true || GetAsyncKeyState(PANIC_KEY) > 0)
                {
                    Environment.Exit(0);
                }

                if (GetAsyncKeyState(ESP_HOTKEY) > 0)
                {
                    enableESP = !enableESP;
                    LeftClick((int)windowCenter.X, (int)windowCenter.Y);
                }

                if (GetAsyncKeyState(MENU_HOTKEY) > 0)
                {
                    showWindow = !showWindow;
                    Thread.Sleep(80);
                }
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

            // check if enableAimClosestToCrosshair is enabled
            /*if (enableAimClosestToCrosshair)
            {
                if (enemyTeam.Count > 0)
                {
                    enemyTeam = enemyTeam.OrderBy(x => x.angleDifference).ToList(); // sort the list by angle difference
                }
            }
            else
            {
                enemyTeam = enemyTeam.OrderBy(x => x.magnitude).ToList(); // sort the list by distance
            }*/
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
            // Calculate 3d then 2d then 1d

            // 3d
            entity.origin = swed.ReadVec(entity.address, offsets.origin);
            entity.viewOffset = new Vector3(0, 0, 65); // Simulate view offset (Height of character in game)
            entity.abs = Vector3.Add(entity.origin, entity.viewOffset);

            // 2d

            var currentViewmatrix = ReadMatrix(client + offsets.ViewMatrix);
            entity.originScreenPosition = Vector2.Add(WorldToScreen(currentViewmatrix, entity.origin, (int)windowSize.X, (int)windowSize.Y), windowLocation);
            entity.absScreenPosition = Vector2.Add(WorldToScreen(currentViewmatrix, entity.abs, (int)windowSize.X, (int)windowSize.Y), windowLocation);

            // 1d
            entity.health = swed.ReadInt(entity.address, offsets.health);
            entity.teamNum = swed.ReadInt(entity.address, offsets.teamNum);
            entity.origin = swed.ReadVec(entity.address, offsets.origin);
            entity.m_iIDEntIndex = swed.ReadInt(entity.address, offsets.m_iIDEntIndex);
            entity.dwGameRules = swed.ReadInt(entity.address, offsets.dwGameRules);
            entity.dwPlantedC4 = swed.ReadBool(entity.address, offsets.dwPlantedC4 + entity.dwGameRules);
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