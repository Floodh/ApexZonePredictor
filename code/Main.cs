#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;

//  For WE only set0 exist
//  For SP only set1 exist

static class MainClass
{

    
    static int Main(string[] args)
    {


        Console.WriteLine("start");


        Activity.ProcessTestData("WE", "set0");
        Activity.ProcessTestData("WE", "set1");
        //Activity.ProcessTestData("SP", "set1");
        //Activity.RingConsole();
        //Activity.CaptureAllData("set1");

        Console.WriteLine("end");
        return 0;
    }
}