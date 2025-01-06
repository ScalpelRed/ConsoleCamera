using Emgu.CV;
using System.Text;

class Program
{
    static readonly string[] Colors =
    {
        "  ", " ░", "░░", "░▒", "▒▒", "▒▓", "▓▓", "▓█", "██"
    };

    const int DEFAULT_CAMERA_INDEX = 0;
    const float DEFAULT_QUALITY_X = 0.25f;
    const float DEFAULT_QUALITY_Y = 0.25f;

    static VideoCapture Capture = null!;

    static int StepX;
    static bool FlipX;
    static int SizeX;

    static int StepY;
    static bool FlipY;
    static int SizeY;

    static void Main()
    {
        {
            Console.Write($"Camera index (anything else for {DEFAULT_CAMERA_INDEX}) ");
            string input = Console.ReadLine()!;
            int value = DEFAULT_CAMERA_INDEX;
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine($"Value was set to {DEFAULT_CAMERA_INDEX}");
            }
            else if (int.TryParse(input, out int v))
            {
                value = v;
            }
            else
            {
                Console.WriteLine($"Invalid format or value, was set to {DEFAULT_CAMERA_INDEX}");
            }
            Console.WriteLine("Initializing the camera, please wait...");
            Capture = new VideoCapture(value);
            if (!Capture.IsOpened)
            {
                Console.WriteLine("Camera does not exist. Press any key to exit...");
                Console.ReadLine();
                return;
            }
        }
        Console.WriteLine("Camera resolution is " + Capture.Width + "x" + Capture.Height);

        {
            Console.Write($"Quality X (in (0.0, 1.0], anything else for {DEFAULT_QUALITY_X}) ");
            string input = Console.ReadLine()!.Replace('.', ',');
            float value = DEFAULT_QUALITY_X;
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine($"Value was set to {DEFAULT_QUALITY_X}");
            }
            else if (float.TryParse(input, out float v) && v > 0 && v <= 1)
            {
                value = v;
            }
            else
            {
                Console.WriteLine($"Invalid format or value, was set to {DEFAULT_QUALITY_X}");
            }
            StepX = (int)(1f / value);
        }

        {
            Console.Write($"Quality Y (in (0.0, 1.0], anything else for {DEFAULT_QUALITY_Y}) ");
            string input = Console.ReadLine()!.Replace('.', ',');
            float value = DEFAULT_QUALITY_Y;
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine($"Value was set to {DEFAULT_QUALITY_Y}");
            }
            else if (float.TryParse(input, out float v) && v > 0 && v <= 1)
            {
                value = v;
            }
            else
            {
                Console.WriteLine($"Invalid format or value, was set to {DEFAULT_QUALITY_Y}");
            }
            StepY = (int)(1f / value);
        }

        SizeX = Capture.Width;
        SizeY = Capture.Height;

        PrintThread.Start();
        byte[] dat = new byte[SizeX * SizeY * 3];
        int posyInit = 0;
        int posxInit = 0;
        int posyDelta = StepY * SizeX * 3;
        int posxDelta = StepX * 3;
        int sizeX3 = SizeX * 3;
        while (true)
        {
            if (Console.CapsLock)
            {
                if (FlipX != Console.CapsLock)
                {
                    FlipX = true;
                    posxDelta = -posxDelta;
                    posxInit = sizeX3 - posxInit;
                    posxInit += FlipX ? -3 : 3;
                }
            }
            else FlipX = false;

            if (!Console.NumberLock)
            {
                if (FlipY == Console.NumberLock)
                {
                    FlipY = true;
                    posyDelta = -posyDelta;
                    posyInit = dat.Length - posyInit;
                    posyInit += FlipY ? -sizeX3 : sizeX3;
                }
            }
            else FlipY = false;

            Mat frame = Capture.QueryFrame();
            if (frame is not null)
            {
                frame.CopyTo(dat);
                lock (PrintBuffer)
                {
                    PrintBuffer.Clear();
                    int posy = posyInit;
                    for (int y = 0; y < SizeY; y += StepY)
                    {
                        int pos = posy + posxInit;
                        for (int x = 0; x < SizeX; x += StepX)
                        {
                            int ind = (int)MathF.Floor((dat[pos] + dat[pos + 1] + dat[pos + 2]) / 768f * Colors.Length);
                            PrintBuffer.Append(Colors[ind]);
                            pos += posxDelta;
                        }
                        PrintBuffer.AppendLine();
                        posy += posyDelta;
                    }
                }
            }
            lock (PrintThreadLock)
            {
                PrintThreadWait = false;
                Monitor.Pulse(PrintThreadLock);
            }
        }
    }

    static readonly StringBuilder PrintBuffer = new();
    static readonly object PrintThreadLock = new();
    static bool PrintThreadWait = true;
    static readonly Thread PrintThread = new(PrintThreadFunc);
    static void PrintThreadFunc()
    {
        while (true)
        {
            while (PrintThreadWait)
            {
                lock (PrintThreadLock)
                {
                    Monitor.Wait(PrintThreadLock);
                }
            }

            Console.SetCursorPosition(0, 0);
            string ot;
            lock (PrintBuffer)
            {
                ot = PrintBuffer.ToString();
            }
            Console.WriteLine(ot);
            PrintThreadWait = true;
        }
    }
}

