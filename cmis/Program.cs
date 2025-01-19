using cmis;
using cmis.WebSoket;
using Microsoft.AspNetCore.Builder;

public class Program
{
    public static void Main(string[] args)
    {


        //var builder = WebApplication.CreateBuilder(args);

        //// Register your services with appropriate lifetimes
        //builder.Services.AddScoped<IMessageService, MessageService>(); // IMessageService is Scoped
        //builder.Services.AddScoped<WebSocketHandler>(); // WebSocketHandler is Scoped

        //var app = builder.Build();

        //// Enable WebSocket support
        //app.UseWebSockets();

        //// Map WebSocket route
        //app.Map("/ws", websocketApp =>
        //{
        //    websocketApp.Use(async (context, next) =>
        //    {
        //        if (context.WebSockets.IsWebSocketRequest)
        //        {
        //            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        //            // Retrieve WebSocketHandler from DI container and call HandleWebSocketAsync
        //            var webSocketHandler = context.RequestServices.GetRequiredService<WebSocketHandler>();
        //            await webSocketHandler.HandleWebSocketAsync(webSocket);
        //        }
        //        else
        //        {
        //            context.Response.StatusCode = 400; // Bad Request if it's not a WebSocket request
        //        }

        //        await next(); // Continue to next middleware if needed
        //    });
        //});

        //app.Run();
        CreateHostBuilder(args).Build().Run();

    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}