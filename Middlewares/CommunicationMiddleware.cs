using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TicTacToe.Models;
using TicTacToe.Services;
using TicTacToe.Services.Interfaces;

namespace TicTacToe.Middlewares
{
    public class CommunicationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUserService _userService;

        public CommunicationMiddleware(RequestDelegate next, IUserService userService)
        {
            _next = next;
            _userService = userService;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var cancellationToken = context.RequestAborted;
                var json = await ReceiveStringAsync(webSocket, cancellationToken);
                var command = JsonConvert.DeserializeObject<dynamic>(json);
                switch (command.Operation.ToString())
                {
                    case "CheckEmailConfirmationStatus":
                    {
                        await ProcessEmailConfirmation(context, webSocket, cancellationToken, command.Parameters.ToString());
                        break;
                    }
                }
            }
            else if (context.Request.Path.Equals("/CheckEmailConfirmationStatus"))
            {
                await ProcessEmailConfirmation(context);
            }
            else
            {
                await _next?.Invoke(context);
            }
        }

        private async Task ProcessEmailConfirmation(HttpContext context)
        {
            var email = context.Request.Query["email"];
            var user = await _userService.GetUserByEmail(email);

            if (string.IsNullOrEmpty(email))
            {
                await context.Response.WriteAsync("Nieprawidłowe żądanie: Wymagany jest adres e-mail");
            }
            else if ((await _userService.GetUserByEmail(email)).IsEmailConfirmed)
            {
                await context.Response.WriteAsync("OK");
            }
            else
            {
                await context.Response.WriteAsync("Oczekiwanie na potwierdzenie adresu e-mail");
                user.IsEmailConfirmed = true;
                user.EmailConfirmationDate = DateTime.Now;
                _userService.UpdateUser(user).Wait();
            }
        }

        private async Task ProcessEmailConfirmation(HttpContext context, WebSocket currentSocket, CancellationToken cancellationToken, string email)
        {
            UserModel user = await _userService.GetUserByEmail(email);
            while (!cancellationToken.IsCancellationRequested && !currentSocket.CloseStatus.HasValue && user?.IsEmailConfirmed == false)
            {
                if (user.IsEmailConfirmed)
                {
                    await SendStringAsync(currentSocket, "OK", cancellationToken);
                }
                else
                {
                    user.IsEmailConfirmed = true;
                    user.EmailConfirmationDate = DateTime.Now;
                    await _userService.UpdateUser(user);
                    await SendStringAsync(currentSocket, "OK", cancellationToken);
                }
            }
            Task.Delay(500).Wait();
            user = await _userService.GetUserByEmail(email);
        }

        private static Task SendStringAsync(WebSocket socket, string data, CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = Encoding.UTF8.GetBytes(data);
            var segment = new ArraySegment<byte>(buffer);
            return socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }

        private static async Task<String> ReceiveStringAsync(WebSocket socket, CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, cancellationToken);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                    throw new Exception("Unexpected statement.");

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
