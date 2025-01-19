using cmis.Model;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using cmis.Manager;
using MySqlX.XDevAPI.Common;
using System;

namespace cmis.WebSoket
{

    public class WebSocketHandler
    {
        private readonly IMessageService _messageService;
        public WebSocketHandler(IMessageService messageService) { _messageService = messageService; }
        public async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;
            string message;

            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Process the received message (e.g., save to DB)
                await ProcessReceivedMessageAsync(message);

                var msg = JsonSerializer.Deserialize<MessageModel>(message);

                // Retrieve messages based on clubId
                var messages = await _messageService.GetMessagesByClubIdAsync(msg.club_id);

                // Convert the list of messages into a JSON string
                var responseMessage = JsonSerializer.Serialize(messages);


                // Send the retrieved messages to the WebSocket client
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(responseMessage)),
                                          WebSocketMessageType.Text, true, CancellationToken.None);

            }

            //byte[] buffer = new byte[1024 * 4];
            //WebSocketReceiveResult result;
            //string message;

            //while (webSocket.State == WebSocketState.Open)
            //{
            //    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            //    message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            //    // Process the received message (e.g., save to DB)
            //    await ProcessReceivedMessageAsync(message);

            //    // Respond to the sender
            //    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Delivered")), WebSocketMessageType.Text, true, CancellationToken.None);
            //}
        }

        public async Task ProcessReceivedMessageAsync(string messageJson)
        {
            try
            {
                // Deserialize the incoming WebSocket message into MessageModel
                var message = JsonSerializer.Deserialize<MessageModel>(messageJson);

                // Save message to the database using the MessageService
                //var messageService = new MessageService();
                await _messageService.SaveMessageAsync(message);
            }
            catch (Exception ex)
            {

                //throw ex;
            }

        }
    }
}
