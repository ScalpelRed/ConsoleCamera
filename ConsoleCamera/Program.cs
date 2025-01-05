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
    static int StepY;

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

        PrintThread.Start();
        byte[] dat = new byte[Capture.Width * Capture.Height * 3];
        int posyDelta = StepY * Capture.Width * 3;
        int posDelta = StepX * 3;
        while (true)
        {
            Mat frame = Capture.QueryFrame();
            if (frame is not null)
            {
                frame.CopyTo(dat);
                lock (PrintBuffer)
                {
                PrintBuffer.Clear();
                    int posy = 0;
                    for (int y = 0; y < Capture.Height; y += StepY)
                {
                        int pos = posy;
                        for (int x = 0; x < Capture.Width; x += StepX)
                    {
                            int ind = (int)MathF.Floor((dat[pos] + dat[pos + 1] + dat[pos + 2]) / 768f * 9);
                        PrintBuffer.Append(Colors[ind]);
                            pos += posDelta;
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

