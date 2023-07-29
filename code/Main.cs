#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;

//  For WE only set0 exist
//  For SP only set1 exist

static class MainClass
{

    
    static int Main(string[] args)
    {


        Console.WriteLine("Start");

        //Activity.PrepareCache();


        List<Result> result_we_set0 = Activity.ProcessTestData("WE", "set0");
        List<Result> result_we_set1 = Activity.ProcessTestData("WE", "set1");
        List<Result> result_sp_set1 = Activity.ProcessTestData("SP", "set1");

        //  Its important that we save the result after every result has been calculated.
        //  That way the median and avg values will be correct.

        Console.WriteLine("Saving results");

        foreach (Result result in result_we_set0)
            result.Save();
        foreach (Result result in result_we_set1)
            result.Save();
        foreach (Result result in result_sp_set1)
            result.Save();

        // //Activity.RingConsole();
        //Activity.CaptureAllData("set1");

        Console.WriteLine("End");
        return 0;
    }
}