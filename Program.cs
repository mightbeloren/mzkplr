using System.Numerics;
using Raylib_cs;

namespace mzkplr;

class Program
{
    private static Dictionary<string, Font> fonts = new();
    private static string fontsPath = "assets/fonts";
    private static string streamsPath = "library/audios";
    private static string digitsPngPath = "assets/images/digits.png";
    private static int audioIndex = 0;
    private static List<string> streams = new();
    private static Music? currentMusicStream;
    private static bool shouldQuit = false;
    private static bool isPaused = false;
    private static int maxFontSize = 130;
    private static int height = 1080;
    private static int width = 1920;
    private static Rectangle progressBarRect;

    //digits
    private static Texture2D digitsTexture;
    private static int charHeight;
    private static int charWidth;
    private static Dictionary<char, int> charIndexes = new();
    private static int heightIndex = 0;

    private static int xStart = 0;
    private static int yStart = 0;
    private static int spacing = 2;

    static void Main(string[] args)
    {
        Initialize();
    }

    private static void LoadDigitsPngTexture()
    {
        digitsTexture = Raylib.LoadTexture(digitsPngPath);
        charHeight = digitsTexture.Height / 3;
        charWidth = digitsTexture.Width / 11;

        charIndexes.Add('0', 0);
        charIndexes.Add('1', 1);
        charIndexes.Add('2', 2);
        charIndexes.Add('3', 3);
        charIndexes.Add('4', 4);
        charIndexes.Add('5', 5);
        charIndexes.Add('6', 6);
        charIndexes.Add('7', 7);
        charIndexes.Add('8', 8);
        charIndexes.Add('9', 9);
        charIndexes.Add(':', 10);
    }

    private static void UnloadDigitsPngTexture()
    {
        Raylib.UnloadTexture(digitsTexture);
    }

    private static void Initialize()
    {
        Raylib.InitWindow(width, height, "mzkplr");
        Raylib.InitAudioDevice();
        LoadDigitsPngTexture();
        LoadFonts();
        LoadMusicFiles();
        LoadMusicStream();
        while (!Raylib.WindowShouldClose() && !shouldQuit)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Gray);
            RunEvents();
            RenderMusicTitle();
            RenderTimePlayed();
            Raylib.EndDrawing();
        }
        if (currentMusicStream != null)
            Raylib.UnloadMusicStream(currentMusicStream.Value);
        UnloadDigitsPngTexture();
        UnloadFonts();
        Raylib.CloseWindow();
    }

    private static void UpdateJitterFrame()
    {
        heightIndex = (int)(Raylib.GetTime() * 1000 / 250) % 3;
    }

    private static void DrawChar(char c, int index, float scale = 0.35f)
    {
        int charIndex = charIndexes[c];
        Rectangle source = new Rectangle(
            charWidth * charIndex,
            charHeight * heightIndex,
            charWidth,
            charHeight
        );

        float scaledW = charWidth * scale;
        float scaledH = charHeight * scale;
        float x = xStart + index * (scaledW + spacing);

        Rectangle dest = new Rectangle(x, yStart, scaledW, scaledH);

        Raylib.DrawTexturePro(digitsTexture, source, dest, new Vector2(0, 0), 0f, Color.White);
    }

    private static void LoadMusicStream()
    {
        if (currentMusicStream != null)
            Raylib.UnloadMusicStream(currentMusicStream.Value);
        if (streams.Count == 0)
        {
            currentMusicStream = null;
            return;
        }
        currentMusicStream = Raylib.LoadMusicStream(streams[audioIndex]);
        Raylib.PlayMusicStream(currentMusicStream.Value);
    }

    private static void RunEvents()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Q))
        {
            shouldQuit = true;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            if (currentMusicStream != null)
                if (isPaused)
                {
                    isPaused = !isPaused;
                    Raylib.ResumeMusicStream(currentMusicStream.Value);
                }
                else
                {
                    isPaused = !isPaused;
                    Raylib.PauseMusicStream(currentMusicStream.Value);
                }
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Right))
        {
            UpdateStream();
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Left))
        {
            UpdateStream(false);
        }
        if (currentMusicStream != null)
        {
            Raylib.UpdateMusicStream(currentMusicStream.Value);
            RenderProgressBar();
        }
    }

    private static void UpdateStream(bool increment = true)
    {
        if (increment)
            audioIndex = (audioIndex + 1) % streams.Count;
        else
            audioIndex = (audioIndex - 1 + streams.Count) % streams.Count;
        LoadMusicStream();
    }

    private static void LoadFonts()
    {
        var files = Directory.EnumerateFiles(fontsPath);
        if (files == null)
            return;
        foreach (var file in files)
        {
            string fontName = Path.GetFileNameWithoutExtension(file);
            Font font = Raylib.LoadFontEx(file, maxFontSize, null, 0);
            Raylib.SetTextureFilter(font.Texture, TextureFilter.Bilinear);
            fonts[fontName] = font;
        }
    }

    private static void UnloadFonts()
    {
        foreach (var font in fonts)
        {
            Raylib.UnloadFont(font.Value);
        }
    }

    private static void RenderMusicTitle()
    {
        if (streams.Count == 0)
            return;
        string musicName = Path.GetFileNameWithoutExtension(streams[audioIndex]);
        if (isPaused)
        {
            musicName = $"{musicName}";
        }
        var boldFont = fonts["JetBrainsMonoNerdFont-Bold"];
        var fontSize = 100;
        var size = Raylib.MeasureTextEx(boldFont, musicName, fontSize, 0);
        var screenHeight = Raylib.GetScreenHeight();
        var screenWidth = Raylib.GetScreenWidth();
        var position = new System.Numerics.Vector2(
            (screenWidth - size.X) / 2,
            ((screenHeight - size.Y) / 2) - 100
        );
        Raylib.DrawTextEx(boldFont, musicName, position, fontSize, 0, Color.White);
    }

    private static void RenderProgressBar()
    {
        var screenHeight = Raylib.GetScreenHeight();
        var screenWidth = Raylib.GetScreenWidth();
        int barWidth = (int)(screenWidth * 0.95);
        int barHeight = (int)(screenHeight * 0.05);
        var position = new System.Numerics.Vector2(
            (screenWidth - barWidth) / 2,
            ((screenHeight - barHeight) / 2) + 100
        );

        progressBarRect = new Rectangle(position.X, position.Y, barWidth, barHeight);

        Raylib.DrawRectangleLines(
            (int)position.X,
            (int)position.Y,
            barWidth,
            barHeight,
            Color.White
        );
        if (currentMusicStream != null)
        {
            var totalSeconds = Raylib.GetMusicTimeLength(currentMusicStream.Value);
            var playedSeconds = Raylib.GetMusicTimePlayed(currentMusicStream.Value);
            float percentage = (playedSeconds / totalSeconds) * 100;
            int fillWidth = (int)(barWidth * (percentage / 100f));

            Raylib.DrawRectangle(
                (int)position.X,
                (int)position.Y,
                fillWidth,
                barHeight,
                Color.White
            );
        }
    }

    public static void RenderTimePlayed()
    {
        if (currentMusicStream != null)
        {
            TimeSpan time = TimeSpan.FromSeconds(
                Raylib.GetMusicTimePlayed(currentMusicStream.Value)
            );
            string timeStr = $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";

            float scale = 0.35f;
            float scaledCharH = charHeight * scale;

            xStart = (int)progressBarRect.X; // align to bar's left edge
            yStart = (int)(progressBarRect.Y - scaledCharH - 10); // 10px padding above bar

            UpdateJitterFrame();
            for (int i = 0; i < timeStr.Length; i++)
            {
                DrawChar(timeStr[i], i);
            }
        }
    }

    private static void LoadMusicFiles()
    {
        var files = Directory.EnumerateFiles(streamsPath);
        if (files == null)
            return;
        streams = files.ToList();
    }
}
