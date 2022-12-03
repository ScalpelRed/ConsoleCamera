using Emgu.CV;

class Program
{

    static VideoCapture Capture;

    static readonly string[] grs =
    {
        "  ", " ░", "░░", "░▒", "▒▒", "▒▓", "▓▓", "▓█", "██"
    };

    static int stepX;
    static int stepY;
    static int interval;

    static void Main()
    {
        Console.WriteLine("Initializing the camera, please wait...");

        Capture = new VideoCapture(0);

        Console.WriteLine("Camera resolution is " + Capture.Width + "x" + Capture.Height);

        Console.Write("Quality X (from 0 to 1, empty for 0.25) ");
        try
        {
            float d = float.Parse(Console.ReadLine().Replace('.', ','));
            if (d <= 0 || d > 1) stepX = 4;
            else stepX = (int)(1f / d);
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid format, value was set to 0.25");
            stepX = 4;
        }
        Console.Write("Quality Y (from 0 to 1, empty for 0.25) ");
        try
        {
            float d = float.Parse(Console.ReadLine().Replace('.', ','));
            if (d <= 0 || d > 1) stepY = 4;
            else stepY = (int)(1f / d);
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid format, value was set to 0.25");
            stepY = 4;
        }
        Console.Write("Target fps (values bigger than 20 are not recommended) ");
        try
        {
            float d = float.Parse(Console.ReadLine());
            if (d <= 0) interval = 500;
            else interval = (int)(1000 / d);
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid format, value was set to 2");
            interval = 500;
        }

        while (true)
        {
            ProcessFrame();
            Thread.Sleep(interval);
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
            Console.WriteLine(FrameToString(stepX, stepY));
        }

        string FrameToString(int wx, int wy)
        {
            string res = "";
            for (int i = 0; i < frame.Rows; i += wy)
            {
                for (int p = frame.Cols - 1; p > 0; p -= wx)
                {
                    int pos = (p + i * frame.Cols) * 3;
                    res += grs[(int)(dat[pos + 2] / 255f * 8)];

                }
                res += "\n";
            }
            return res;
        }
    }
}

