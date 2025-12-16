using System;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

string GetBaseDirectory( [CallerFilePath] string? callerFilePath = null ) => callerFilePath?.Replace("Program.cs","") ?? "";

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel((context, options) =>
{
    // Port configured for WebTransport
    IPEndPoint local = new IPEndPoint(IPAddress.IPv6Loopback, 8080);
    options.Listen(local, listenOptions =>
    {
        // Load the certificate and private key
        var secrets = GetBaseDirectory();
        var certPath = Path.Combine(secrets, "localhost.pem");
        var keyPath = Path.Combine(secrets, "localhost.key");
        var certificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);

        var hash = SHA256.HashData(certificate.RawData);
        var certStr = Convert.ToHexStringLower(hash);
        Console.WriteLine($"\n\n\n\n\nCertificate: {certStr}\n\n\n\n"); // <-- you will need to put this output into the JS API call to allow the connection

        // Use the loaded certificate with UseHttps
        listenOptions.UseHttps(certificate);
        listenOptions.Protocols = HttpProtocols.Http3;
    });
});
builder.WebHost.UseQuic();
var app = builder.Build();

app.Run(async (context) =>
{
    var feature = context.Features.GetRequiredFeature<IHttpWebTransportFeature>();
    if (!feature.IsWebTransportRequest)
    {
        return;
    }
    var session = await feature.AcceptAsync(CancellationToken.None);

    ConnectionContext? stream = null;
    IStreamDirectionFeature? direction = null;
    while (true)
    {
        // wait until we get a stream
        stream = await session.AcceptStreamAsync(CancellationToken.None);
        if (stream is null)
        {
            // if a stream is null, this means that the session failed to get the next one.
            // Thus, the session has ended, or some other issue has occurred. We end the
            // connection in this case.
            return;
        }

        // check that the stream is bidirectional. If yes, keep going, otherwise
        // dispose its resources and keep waiting.
        direction = stream.Features.GetRequiredFeature<IStreamDirectionFeature>();
        if (direction.CanRead && direction.CanWrite)
        {
            break;
        }
        else
        {
            await stream.DisposeAsync();
        }
    }

    var inputPipe = stream!.Transport.Input;
    var outputPipe = stream!.Transport.Output;

    // read some data from the stream into the memory
    var memory = new Memory<byte>(new byte[1024]);
    var length = await inputPipe.AsStream().ReadAsync(memory);

    // slice to only keep the relevant parts of the memory
    var outputMemory = memory[..length];

    // do some operations on the contents of the data
    outputMemory.Span.Reverse();

    // write back the data to the stream
    await outputPipe.WriteAsync(outputMemory);
});

await app.RunAsync();
