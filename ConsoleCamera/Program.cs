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
    const float DEFAULT_FPS = 20f;

    static VideoCapture Capture = null!;
    static int StepX;
    static int StepY;
    static int TimeDelta;

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
                Console.WriteLine($"Invalid format or value, was set to {DEFAULT_QUALITY_X}");
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
            Console.Write($"Quality Y ((in (0.0, 1.0], anything else for {DEFAULT_QUALITY_Y}) ");
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
        Console.SetBufferSize(Capture.Width / StepX, Capture.Height / StepY);
        if (Console.BufferWidth < Console.LargestWindowWidth && Console.BufferHeight < Console.LargestWindowHeight)
            Console.SetWindowSize(Console.BufferWidth, Console.BufferHeight);

        {
            Console.Write($"Target fps (more than 0, anything else for {DEFAULT_FPS}) ");
            string input = Console.ReadLine()!.Replace('.', ',');
            float value = DEFAULT_FPS;
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine($"Value was set to {DEFAULT_FPS}");
            }
            else if (float.TryParse(input, out float v) && v > 0)
            {
                value = v;
            }
            else
            {
                Console.WriteLine($"Invalid format or value, was set to {DEFAULT_FPS}");
            }
            TimeDelta = (int)(1000f / value);
        }

        PrintThread.Start();
        while (true)
        {
            Mat frame = Capture.QueryFrame();
            byte[] dat = new byte[frame.Cols * frame.Rows * 3];
            if (frame is not null)
            {
                frame.CopyTo(dat);
                PrintBuffer.Clear();
                for (int y = 0; y < frame.Rows; y += StepY)
                {
                    for (int x = 0; x < frame.Cols; x += StepX)
                    {
                        int pos = (x + y * frame.Cols) * 3;
                        int ind = (int)MathF.Floor((dat[pos] + dat[pos + 1] + dat[pos + 2]) / 256f * 3);
                        PrintBuffer.Append(Colors[ind]);
                    }
                    PrintBuffer.AppendLine();
                }
            }
            lock (PrintThreadLock)
            {
                PrintThreadWait = false;
                Monitor.Pulse(PrintThreadLock);
            }
            Thread.Sleep(TimeDelta);
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
            Console.WriteLine(PrintBuffer.ToString());
            PrintThreadWait = true;
        }
    }
}

