// See https://aka.ms/new-console-template for more information
using System.Threading.Channels;

Console.WriteLine("Hello, World!");


var channel = Channel.CreateUnbounded<string>();


Task.Run(async () =>
{
    var reader = channel.Reader;
    while (await reader.WaitToReadAsync())
    {
        while (reader.TryRead(out var message))
        {
            Console.WriteLine($"Received: {message}");
        }
    }
});

for (int i = 0; i < 50; i++)
{
    Console.WriteLine($"Sending: Message {i}");
    await channel.Writer.WriteAsync($"Message {i}");
    await Task.Delay(1000);
}