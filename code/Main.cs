#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;

static class MainClass
{

    
    static int Main(string[] args)
    {


        Console.WriteLine("start");


        Activity.ProcessTestData();
        //Activity.RingConsole();
        // DataSource.CaptureDropData();


        Console.WriteLine("end");
        return 0;
    }
}