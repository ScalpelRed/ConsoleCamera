using Emgu.CV;

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

        while (true)
        {
            ProcessFrame();
            Thread.Sleep(TimeDelta);
        }
    }

    static int clearCounter = 0;
    static void ProcessFrame()
    {

        Mat frame = Capture.QueryFrame();
        byte[] dat = new byte[frame.Cols * frame.Rows * 3];
        if (frame is not null)
        {
            frame.CopyTo(dat);
            clearCounter++;
            if (clearCounter == 30)
            {
                Console.Clear();
                clearCounter = 0;
            }
            Console.WriteLine(FrameToString(StepX, StepY));
        }

        string FrameToString(int wx, int wy)
        {
            string res = "";
            for (int i = 0; i < frame.Rows; i += wy)
            {
                for (int p = frame.Cols - 1; p > 0; p -= wx)
                {
                    int pos = (p + i * frame.Cols) * 3;
                    res += Colors[(int)(dat[pos + 2] / 255f * 8)];

                }
                res += "\n";
            }
            return res;
        }
    }
}

