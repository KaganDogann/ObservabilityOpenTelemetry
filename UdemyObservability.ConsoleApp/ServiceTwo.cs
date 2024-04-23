
namespace UdemyObservability.ConsoleApp;

internal class ServiceTwo
{
    internal async Task<int> WriteToFile(string text)
    {
        using (var activity = ActivitySourceProvider.Source.StartActivity("a")) // name: "WriteToFile", kind: System.Diagnostics.ActivityKind.Server
        {
            await File.WriteAllTextAsync("myFile.txt", text);

            return (await File.ReadAllTextAsync("myFile.txt")).Length;
        }

        //using (var activity12 = ActivitySourceProvider.Source.StartActivity("b")) // name: "WriteToFile", kind: System.Diagnostics.ActivityKind.Server
        //{
        //    await File.WriteAllTextAsync("myFileAdada.txt", text);

        //    var b = (await File.ReadAllTextAsync("myFileAdada.txt")).Length;
        //}

        //using (var curt = ActivitySourceProvider.Source.StartActivity("cdasda")) // name: "WriteToFile", kind: System.Diagnostics.ActivityKind.Server
        //{
        //    //Activity.Current?.SetTag("aassss", "asda");

        //    await File.WriteAllTextAsync("myFile000.txt", text);

        //    return (await File.ReadAllTextAsync("myFile00.txt")).Length;
        //}
    }
}