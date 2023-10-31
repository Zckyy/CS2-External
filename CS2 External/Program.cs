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

        const int MENU_HOTKEY = 0x70; // F1 Hotkey
        const int PANIC_KEY = 0x73; // F4 Hotkey
        static int TRIGGER_KEY = 0x05;// X1 mouse button Hotkey

        // ImGui stuff

        Vector4 teamBoxColor = new Vector4(0, 0, 1, 1); // RGBA, Blue teammates
        Vector4 lineColor = new Vector4(1, 1, 1, 1);
        Vector4 enemyBoxColor = new Vector4(1, 0, 0, 1); // RGBA, Red enemies
        Vector4 healthBarcolor = new Vector4(0, 1, 0, 1); // RGBA, Green healthbar
        Vector4 healthBarTextcolor = new Vector4(0, 0, 0, 1); // RGBA, black text
        Vector4 boixFillColor = new Vector4(0, 0, 0, (float)0.20); // RGBA, black box fill

        // Screen variables, we update these later

        Vector2 windowLocation = new Vector2(0, 0);
        Vector2 windowSize = new Vector2(1920, 1080);
        Vector2 lineOrigin = new Vector2(1920 / 2, 1080);
        Vector2 windowCenter = new Vector2(1920 / 2, 1080 / 2);

        // ImGui checkboxes and stuff

        bool showMenu = true;

        bool killswitch = false;

        bool enableESP = true;

        bool enableTeamLine = false;
        bool enableTeamBox = false;
        bool enableTeamDot = false;
        bool enableTeamHealthBar = false;
        bool enableTeamHealthBarText = false;
        bool enableTeamBoxFill = false;

        bool enableEnemyLine = true;
        bool enableEnemyBox = true;
        bool enableEnemyDot = true;
        bool enableEnemyHealthBar = true;
        bool enableEnemyHealthBarText = true;
        bool enableEnemyBoxFill = true;

        bool isTriggerEnabled = true;

        //Hotkey assign in menu
        static string inputText = "";
        static bool isButtonPress = false;
        static bool captureInput = false;
        static int captureKey = -1;

        bool IsAimingAtEnemy()
        {
            if (localPlayer.m_iIDEntIndex > -1 && GetAsyncKeyState(TRIGGER_KEY) > 0)
            {
                LeftClick((int)windowCenter.X, (int)windowCenter.Y);
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

            if (showMenu)
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
                                    DrawVisuals(entity, teamBoxColor, enableTeamLine, lineColor, enableTeamBox, enableTeamDot, enableTeamHealthBar, enableTeamHealthBarText, boixFillColor, enableTeamBoxFill);
                                }
                                else
                                {
                                    DrawVisuals(entity, enemyBoxColor, enableEnemyLine, lineColor, enableEnemyBox, enableEnemyDot, enableEnemyHealthBar, enableEnemyHealthBarText, boixFillColor, enableEnemyBoxFill);
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

        void DrawVisuals(Entity entity, Vector4 color, bool line, Vector4 lineColor, bool box, bool dot, bool healthBar, bool healthBarText, Vector4 boxFillColour, bool boxFill)
        {
            // check if 2d position is valid
            if (IsPixelInsideScreen(entity.originScreenPosition))
            {
                // convert our colors from imgui to units

                uint uintColor = ImGui.ColorConvertFloat4ToU32(color);
                uint uiintHealthTextColor = ImGui.ColorConvertFloat4ToU32(healthBarTextcolor);
                uint uintHealthBarColor = ImGui.ColorConvertFloat4ToU32(healthBarcolor);
                uint uintLineColor = ImGui.ColorConvertFloat4ToU32(lineColor);
                uint uintBoxFill = ImGui.ColorConvertFloat4ToU32(boxFillColour);

                // Calculate box attributes

                Vector2 boxWidth = new Vector2((entity.originScreenPosition.Y - entity.absScreenPosition.Y) / 2, 0f); // divide height by 2 simulate width
                Vector2 boxStart = Vector2.Subtract(entity.absScreenPosition, boxWidth); // get top left corner of box
                Vector2 boxEnd = Vector2.Add(entity.originScreenPosition, boxWidth); // get bottom right corner of box
                Vector2 boxCenter = new Vector2((boxStart.X + boxEnd.X) / 2, ((boxStart.Y + boxEnd.Y) / 2));
                Vector2 boxTopCenter = new Vector2(boxCenter.X, boxStart.Y);
                Vector2 boxUpperUpperCenter = new Vector2((boxStart.X + boxEnd.X) / 2, boxStart.Y + (boxEnd.Y - boxStart.Y) / 8);

                // Calculate health bar stuff

                float barPercentage = entity.health / 100f; // calculate percentage of health
                Vector2 healthBarHeight = new Vector2(0, barPercentage * (entity.originScreenPosition.Y - entity.absScreenPosition.Y)); // calculate height of health bar by multiplying percentage by height of character
                Vector2 healthBarStart = Vector2.Subtract(Vector2.Subtract(entity.originScreenPosition, boxWidth), healthBarHeight); // get position next to the box using box width
                Vector2 heathBarEnd = Vector2.Subtract(entity.originScreenPosition, Vector2.Add(boxWidth, new Vector2(-4, 0))); // get bottom right end of the bar. The -4 is width of the bar.

                // Finally draw
                
                if (line)
                {
                    drawList.AddLine(lineOrigin, boxTopCenter, uintLineColor, 1); // draw line to feet of entities

                }   
                if (box)
                {
                    drawList.AddRect(boxStart, boxEnd, uintColor, 1); // draw box around entities
                }
                if (dot)
                {
                    drawList.AddCircleFilled(boxUpperUpperCenter, 1, uintColor); // draw dot on entities
                }
                if (healthBar)
                {
                    drawList.AddRectFilled(healthBarStart, heathBarEnd, uintHealthBarColor, 2); // draw health bar
                }
                if (healthBarText)
                {
                    drawList.AddText(entity.originScreenPosition, uiintHealthTextColor, $"HP: {entity.health}"); // draw health text
                }
                if (boxFill)
                {
                    drawList.AddRectFilled(boxStart, boxEnd, uintBoxFill); // fill the box with solid color
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
                ImGuiStylePtr style = ImGui.GetStyle();
                ImGuiIOPtr io = ImGui.GetIO();

                style.WindowRounding = 6f;
                style.WindowBorderSize = 3f;
                style.WindowMinSize = new Vector2(700, 700);

                // Background and border
                style.Colors[(int)ImGuiCol.Border] = new Vector4(0, 0, 0, 0.5f);
                style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0, 0, 0, 1f);
                style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0, 0, 0, 1f);
                style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0, 0, 0, 1f);
                style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0, 0, 0, 1f);
                style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(255, 255, 255, 1f);
                style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(200, 200, 200, 1f);
                // Text , buttons, checkboxes, sliders
                style.Colors[(int)ImGuiCol.Text] = new Vector4(255, 255, 255, 1f);
                style.Colors[(int)ImGuiCol.Button] = new Vector4(0, 255, 0, 1f);
                style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(200, 200, 200, 0.6f);
                style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(255, 0, 0, 0.5f);
                style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(255, 255, 255, 0.5f);
                style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(255, 255, 255, 0.5f);
                style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(255, 255, 255, 0.5f);
                // Tabs
                style.Colors[(int)ImGuiCol.Tab] = new Vector4(0, 0, 0, 1f);
                style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0, 255, 0, 1f);
                style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0, 255, 0, 0.5f);

                // Creating the actual menu

                ImGui.Begin($"3DX - Remaining Subscription time -> ${AuthHelper.remaningSubTime}", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize);

                if (ImGui.BeginTabBar("Tabs"))
                {
                    if (ImGui.BeginTabItem("ESP"))
                    {
                        ImGui.Text("ESP");

                        ImGui.Checkbox("Enable ESP", ref enableESP);
                        ShowContextMenuTooltip("Toggles the Wallhacks");
                        ImGui.Separator();

                        ImGui.Text("Team");

                        ImGui.Checkbox("Enable Team Box", ref enableTeamBox);
                        ImGui.Checkbox("Enable Team Dot", ref enableTeamDot);
                        ImGui.Checkbox("Enable Team Health Bar", ref enableTeamHealthBar);
                        ImGui.Checkbox("Enable Team Health Bar Text", ref enableTeamHealthBarText);
                        ImGui.Checkbox("Enable Team Line", ref enableTeamLine);
                        ShowContextMenuTooltip("Toggles Snaplines, these lines start from the bottom center of the game window");
                        ImGui.Checkbox("Enable Box Fill", ref enableTeamBoxFill);
                        ShowContextMenuTooltip("Fills the box with a solid color");

                        ImGui.Separator();

                        ImGui.Text("Enemy");

                        ImGui.Checkbox("Enable Enemy Box", ref enableEnemyBox);
                        ImGui.Checkbox("Enable Enemy Dot", ref enableEnemyDot);
                        ImGui.Checkbox("Enable Enemy Health Bar", ref enableEnemyHealthBar);
                        ImGui.Checkbox("Enable Enemy Health Bar Text", ref enableTeamHealthBarText);
                        ImGui.Checkbox("Enable Enemy Line", ref enableEnemyLine);
                        ShowContextMenuTooltip("Toggles Snaplines, these lines start from the bottom center of the game window");
                        ImGui.Checkbox("Enable Box Fill", ref enableEnemyBoxFill);
                        ShowContextMenuTooltip("Fills the box with a solid color");

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Colors"))
                    {

                        // team colors
                        ImGui.Text("Team Box Color");

                        ImGui.ColorPicker4("Team color Color", ref teamBoxColor);
                        ImGui.Separator();

                        // enemy colors
                        ImGui.Text("Enemy");

                        ImGui.ColorPicker4("Enemy color", ref enemyBoxColor);
                        ImGui.Separator();

                        ImGui.Text("Box Fill Color");
                        ImGui.ColorPicker4("Box Fill color", ref boixFillColor);

                        ImGui.Separator();

                        ImGui.Text("Line Fill Color");
                        ImGui.ColorPicker4("Box Fill color", ref lineColor);

                        ImGui.EndTabItem();

                    }

                    if (ImGui.BeginTabItem("Trigger"))
                    {
                        ImGui.Checkbox("Trigger Bot", ref isTriggerEnabled);
                        ImGui.EndTabItem();

                        if (ImGui.Button("Trigger Hotkey"))
                        {
                            isButtonPress = true;
                            captureInput = true;
                            inputText = "";
                            captureKey = -1;
                        }

                        if (isButtonPress)
                        {
                            if (captureInput)
                            {
                                ImGui.Text("Press any Key... ");
                            }
                            else 
                            {
                                if (captureKey != 1)
                                {
                                    string captureKeyName = Enum.GetName(typeof(CheatHelper.VirtualKeys), TRIGGER_KEY);

                                    ImGui.Text($"Hotkey Input: {captureKeyName}");
                                }
                                else
                                {
                                    ImGui.Text($"Hotkey Input: None");
                                }
                            }
                        }
                    }

                    if (ImGui.BeginTabItem("Debug"))
                    {
                        ImGui.Text($"Grounded?: {localPlayer.m_bOnGroundLastTick}");
                        ImGui.Text($"m_iIDEntIndex: {localPlayer.m_iIDEntIndex}");
                        ImGui.EndTabItem();
                    }

                    // End the tab bar.
                    ImGui.EndTabBar();
                }

                if (isButtonPress && captureInput)
                {
                    for (int key = 0; key < 256; key++)
                    {
                        if ((GetAsyncKeyState(key) & 0x8000) != 0)
                        {
                            TRIGGER_KEY = key;
                            captureInput = false;
                        }
                    }
                }

                ImGui.End();
            }
            catch
            {

            }
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
            lineOrigin = new Vector2(windowLocation.X + windowSize.X / 2, window.top); // TOP CENTER OF SCREEN
            windowCenter = new Vector2(lineOrigin.X, window.bottom - windowSize.Y / 2);

            client = swed.GetModuleBase("client.dll");

            while (true) // Always run
            {
                ReloadEntityList();
                Thread.Sleep(1);

                if (isTriggerEnabled)
                {
                    IsAimingAtEnemy();
                }

                if (killswitch == true || GetAsyncKeyState(PANIC_KEY) > 0)
                {
                    Environment.Exit(0);
                }

                if (GetAsyncKeyState(MENU_HOTKEY) > 0)
                {
                    showMenu = !showMenu;
                    Thread.Sleep(160);
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
            entity.m_bOnGroundLastTick = swed.ReadBool(entity.address, offsets.m_bOnGroundLastTick);
        }

        static void Main(string[] args)
        {
            //AuthHelper.Init();

            Program program = new Program();
            program.Start().Wait();

            Thread mainLogicThread = new Thread(program.MainLogic) { IsBackground = true };
            mainLogicThread.Start();
        }
    }
}