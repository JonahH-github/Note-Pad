using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Globalization;
namespace Build
{
    struct Pointer
    {
        public int X;
        public int Y;
        public Pointer(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    class Program
    {
        static Font Regular;
        static Font Italic;
        static Font Bold;
        static Font BoldItalic;
        static Image Logo;
        static int MenuBarSize = 30;
        static int SideBarSize = 0;
        static int FontSize = 16;
        static int FontSpacing = 0;
        static float CharWidth = 0;
        //static List<string> Lines = new List<string>();
        static string Text = "";//"words words words\n# \"hello\"\n## ^hello^\n### *hello*\n#### _hello_\n##### -hello-\nhello";

        static Pointer Pointer = new Pointer(0,0);
        static int X = 0;
        static int Y = 0;
        static void DrawText(string text, int x, int y, Color? col = null, Font? font = null)
        {
            Raylib.DrawTextEx(font.HasValue ? font.Value : Regular, text, new Vector2(x, y), FontSize, FontSpacing, col.HasValue ? col.Value : Color.WHITE);
        }
        static void Insert(string value)
        {
            int x = 0;
            int y = 0;
            for (int i = 0; i <= Text.Length; i++)
            {

                if (x == Pointer.X && y == Pointer.Y)
                {
                    Text = Text.Insert(i, value);
                    if (value.Contains('\n'))
                    {
                        Pointer.X = 0;
                        Pointer.Y++;
                    }
                    else Pointer.X += value.Length;
                    return;
                }

                if (Text.Length > 0)
                {
                    if (Text[i] == '\n')
                    {
                        y++;
                        x = 0;
                    }
                    else x++;
                }
            }
        }
        static Color HexToColour(string hexCode)
        {
            int i = 0;
            if (hexCode[0] == '#') i = 1;
            int alpha = 255;
            if (hexCode.Length == 8 + i) alpha = int.Parse(new string(new char[]{hexCode[6+i], hexCode[7+i]}), NumberStyles.HexNumber);

            return new Color(
                int.Parse(new string(new char[]{hexCode[0+i], hexCode[1+i]}), NumberStyles.HexNumber), 
                int.Parse(new string(new char[]{hexCode[2+i], hexCode[3+i]}), NumberStyles.HexNumber), 
                int.Parse(new string(new char[]{hexCode[4+i], hexCode[5+i]}), NumberStyles.HexNumber), 
                alpha
            );
        }
        static void Main()
        {
            Text = File.ReadAllText("assets/text.txt");
            var Json = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("assets/styles.json"));
            FontSize = Json.settings.fontSize;
            FontSpacing = Json.settings.fontSpacing;
            Raylib.InitWindow(800,600, "Note Pad");
            Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Regular = Raylib.LoadFont("assets/font/regular.ttf");
            Italic = Raylib.LoadFont("assets/font/italic.ttf");
            Bold = Raylib.LoadFont("assets/font/bold.ttf");
            BoldItalic = Raylib.LoadFont("assets/font/bold italic.ttf");
            Raylib.SetTextureFilter(Regular.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
            CharWidth = Raylib.MeasureTextEx(Regular, "0", FontSize, 0).X;
            
            Logo = Raylib.LoadImage("assets/logo.png");
            Raylib.SetWindowIcon(Logo);
            SideBarSize = (int)(3*CharWidth);

            Raylib.SetTargetFPS(30);
            
            int keyDelay = 0;
            float delayCap = Raylib.GetFPS()/32;
            bool resetKeyDelay = false;

            while (!Raylib.WindowShouldClose())
            {
                string[] Lines = Regex.Replace(Text, "\r", "").Split("\n");
                delayCap = Raylib.GetFPS()/12;
                if (keyDelay > delayCap)
                {
                    #region pointer movement
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_UP)   && Pointer.Y > 0)    
                    {
                        if (Lines[Pointer.Y-1].Length < Pointer.X)
                            Pointer.X = Lines[Pointer.Y-1].Length;
                        Pointer.Y--;
                        resetKeyDelay = true;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN) && Pointer.Y < Lines.Length-1) 
                    {
                        if (Lines[Pointer.Y+1].Length < Pointer.X)
                            Pointer.X = Lines[Pointer.Y+1].Length;
                        Pointer.Y++;
                        resetKeyDelay = true;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT) && Pointer.X > 0)
                    {
                        Pointer.X--;
                        resetKeyDelay = true;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT) && Pointer.X < Lines[Pointer.Y].Length) 
                    {
                        Pointer.X++;
                        resetKeyDelay = true;
                    }
                    #endregion
                    if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON)) File.WriteAllText("text.txt", Text);
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_ENTER)) 
                    {
                        Insert("\n");
                        resetKeyDelay = true;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_BACKSPACE)) 
                    {
                        int x = 0;
                        int y = 0;
                        for (int i = 0; i <= Text.Length; i++)
                        {
                        
                            if (x == Pointer.X && y == Pointer.Y)
                            {
                                if (x!=0 || y!=0)
                                {
                                    Text = Text.Remove(i-1, 1);
                                    if (x == 0)
                                    {
                                        Pointer.X = Lines[Pointer.Y-1].Length;
                                        Pointer.Y--;
                                    }
                                    else Pointer.X--;
                                    break;
                                }
                            }

                            if (Text.Length > 0)
                            {
                                if (i < Text.Length && Text[i] == '\n')
                                {
                                    y++;
                                    x = 0;
                                }
                                else x++;
                            }
                        }
                        string t = Text;
                        resetKeyDelay = true;
                    }

                    int key = Raylib.GetCharPressed();

                    if (!Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
                    {
                        while (key > 0)
                        {
                            if ((key >= 32) && (key <= 125))
                            {
                                Insert(((char)key).ToString());

                                resetKeyDelay = true;
                            }
                            key = Raylib.GetCharPressed();
                        }
                        
                        float mouseWheel = Raylib.GetMouseWheelMove();
                        int MouseScrollSpeed = FontSize * 2;
                        if (mouseWheel > 0) Y += MouseScrollSpeed;
                        if (mouseWheel < 0) Y -= MouseScrollSpeed;
                    }
                    else
                    {
                        float mouseWheel = Raylib.GetMouseWheelMove();
                        int MouseScrollSpeed = (int)CharWidth * 4;
                        if (mouseWheel > 0) X += MouseScrollSpeed;
                        if (mouseWheel < 0) X -= MouseScrollSpeed;
                        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_V)) Insert(TextCopy.ClipboardService.GetText());
                        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_C)) TextCopy.ClipboardService.SetText(Lines[Pointer.Y]);
                        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_S)) File.WriteAllText("text.txt", Text);
                    }

                }
                if (Y + Lines.Length*FontSize <= 0) Y = -(Lines.Length-1)*FontSize;
                else if (Y >= 0) Y = 0;
                int maxLen = 0;
                foreach (var item in Lines) if (item.Length > maxLen) maxLen = item.Length;

                if (X + maxLen*(int)CharWidth <= 0) X = -(maxLen-1)*(int)CharWidth;
                else if (X >= 0) X = 0;

                if (resetKeyDelay) 
                {
                    keyDelay = 0;
                    resetKeyDelay = false;
                }
                keyDelay++;

                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(24,24,24, 255));

                #region text
                for (int i = 0; i < Lines.Length; i++)
                {
                    string str = Lines[i];
                    int len = Lines[i].Length;
                    Color col = Color.WHITE;
                    string colStr = "";
                    for (int c = 0; c < str.Length; c++)
                    {
                        if (str[c] == ' ') break;

                        colStr += str[c];
                    }
                    foreach (var item in Json.customStyles)
                    {
                        if (colStr == item.signal.ToString()) col = HexToColour(item.color.ToString());
                    }
                        

                    bool inItalic = false;
                    bool inBold = false;
                    bool inBoldItalic = false;

                    bool inUnderline = false;
                    bool inCutThrough = false;
                    
                    string regular = "";
                    string bold = "";       char boldChar = '^';
                    string italic = "";     char italicChar = '"';
                    string boldItalic = ""; char boldItalicChar = '*';
                    

                    for (int c = 0; c < str.Length; c++)
                    {
                        if (str[c]=='_')            inUnderline = !inUnderline;
                        if (str[c]=='-')            inCutThrough = !inCutThrough;
                        if (str[c]==boldChar)       inBold = !inBold;
                        if (str[c]==italicChar)     inItalic = !inItalic;
                        if (str[c]==boldItalicChar) inBoldItalic = !inBoldItalic;

                        if (inBoldItalic || (inBold && inItalic) || str[c]==boldItalicChar)
                        {
                            regular += " ";
                            italic += " ";
                            bold += " ";
                            boldItalic += str[c];
                        }
                        else if (inBold || str[c]==boldChar)
                        {
                            regular += " ";
                            italic += " ";
                            bold += str[c];
                            boldItalic += " ";
                        }
                        else if (inItalic || str[c]==italicChar)
                        {
                            regular += " ";
                            italic += str[c];
                            bold += " ";
                            boldItalic += " ";
                        }
                        else
                        {
                            regular += str[c];
                            italic += " ";
                            bold += " ";
                            boldItalic += " ";
                        }
                        
                        if ((bool)Json.settings.enableUnderline &&  inUnderline || str[c] == '_') Raylib.DrawRectangle(X + (int)(SideBarSize + 10 + (CharWidth*c)), Y + (int)(MenuBarSize + ((float)i+.85)*FontSize), (int)CharWidth + 1, FontSize/10, col);
                        if ((bool)Json.settings.enableSrikeThrough && inCutThrough || str[c] == '-') Raylib.DrawRectangle(X + (int)(SideBarSize + 10 + (CharWidth*c)), Y + (int)(MenuBarSize + ((float)i+.45)*FontSize), (int)CharWidth + 1, FontSize/10, col);
                    }
                    DrawText(regular,    X + (int)(SideBarSize + 10), Y + MenuBarSize + i*FontSize, col, Regular);
                    DrawText(bold,       X + (int)(SideBarSize + 10), Y + MenuBarSize + i*FontSize, col, (bool)Json.settings.enableBold ? Bold : Regular);
                    DrawText(italic,     X + (int)(SideBarSize + 10), Y + MenuBarSize + i*FontSize, col, (bool)Json.settings.enableItalic ? Italic : Regular);
                    DrawText(boldItalic, X + (int)(SideBarSize + 10), Y + MenuBarSize + i*FontSize, col, (bool)Json.settings.enableBoldItalic ? BoldItalic : Regular);
                    

                }
                #endregion
                
                #region Pointer
                Raylib.DrawRectangle(X + (int)(SideBarSize + 10 + Pointer.X*CharWidth), Y + MenuBarSize + Pointer.Y*FontSize, (int)CharWidth, FontSize, new Color(255,255,255, 127));
                #endregion

                #region side bar
                Raylib.DrawRectangle(0, MenuBarSize, MenuBarSize, Raylib.GetScreenHeight(), new Color(24,24,24, 255));
                Raylib.DrawRectangle(SideBarSize + 5, MenuBarSize, 1, Raylib.GetScreenHeight(), new Color(50,50,50,255));
                for (int i = 1; i <= Lines.Length; i++)
                {
                    string I = i.ToString();
                    int x = SideBarSize - (int)((I.Length)*CharWidth);
                    if (x < 0) SideBarSize = (int)((I.Length)*CharWidth);
                    DrawText(I, x, Y + MenuBarSize + (i-1)*FontSize);
                }
                #endregion

                #region Menu Bar
                Raylib.DrawRectangle(0,0, Raylib.GetScreenWidth(), MenuBarSize, new Color(10,10,10,255));
                Raylib.DrawFPS(10,10);
                #endregion
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();

            Raylib.UnloadFont(Regular);
            Raylib.UnloadFont(Bold);
            Raylib.UnloadFont(Italic);
            Raylib.UnloadFont(BoldItalic);
            
            Raylib.UnloadImage(Logo);
        }
    }
}
