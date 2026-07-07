using Raylib_cs;

namespace mzkplr;

class Program
{
    private static Dictionary<string, Font> fonts = new();
    private static string fontsPath = "/home/mbl/Work/mzkplr/assets/fonts";
    private static string streamsPath = "/home/mbl/Work/mzkplr/library/audios";
    private static int audioIndex = 0;
    private static List<string> streams = new();
    private static Music? currentMusicStream;
    private static bool shouldQuit = false;
    private static bool isPaused = false;

    static void Main(string[] args)
    {
        Initialize();
    }

    private static void Initialize()
    {
        Raylib.InitWindow(1920, 1080, "mzkplr");
        Raylib.InitAudioDevice();
        LoadFonts();
        LoadMusicFiles();
        LoadMusicStream();
        while (!Raylib.WindowShouldClose() && !shouldQuit)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.SkyBlue);
            RunEvents();
            RenderMusicTitle();
            Raylib.EndDrawing();
        }
        if (currentMusicStream != null)
            Raylib.UnloadMusicStream(currentMusicStream.Value);
        UnloadFonts();
        Raylib.CloseWindow();
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
        }
    }

    // private static void RenderSong()
    // {
    //     if (currentMusicStream == null)
    //         return;
    //     if (!Raylib.IsMusicStreamPlaying(currentMusicStream.Value))
    //     {
    //         UpdateStream();
    //         LoadMusicStream();
    //     }
    // }

    private static void UpdateStream(bool increment = true)
    {
        if (increment)
            audioIndex = (audioIndex + 1) % streams.Count;
        else
            audioIndex = (audioIndex - 1) % streams.Count;
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
            Font font = Raylib.LoadFont(file);
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
        var boldFont = fonts["JetBrainsMono-Bold"];
        var fontSize = 100;
        var textWidth = Raylib.MeasureText(musicName, fontSize);
        Raylib.DrawTextEx(
            fonts["JetBrainsMono-Bold"],
            musicName,
            new System.Numerics.Vector2(
                (Raylib.GetScreenWidth() - textWidth) / 2,
                Raylib.GetScreenHeight() / 2
            ),
            fontSize,
            0,
            Color.Black
        );
    }

    private static void LoadMusicFiles()
    {
        var files = Directory.EnumerateFiles(streamsPath);
        if (files == null)
            return;
        streams = files.ToList();
    }
}
