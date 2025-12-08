using LexiScan.Coordinator;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var app = new AppCoordinator();

while (true)
{
    Console.Write("Nhập input (Enter để thoát): ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        break;

    await app.ProcessAsync(input);
}

Console.WriteLine("\nĐã thoát.");
