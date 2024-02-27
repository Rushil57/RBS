using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public async Task SendMessage(SMSMessage sMS,int ReceiverAgentId, int SenderAgentId)
    {

        //await Clients.All.SendAsync("SendMessage", sMS);
        await Clients.User(SenderAgentId.ToString()).SendAsync("SendMessage", sMS);
        await Clients.User(ReceiverAgentId.ToString()).SendAsync("ReceiveMessage", sMS);
    }

    public async Task ReceiveMessage(SMSMessage sMS)
    {
        await Clients.All.SendAsync("ReceiveMessage", sMS);
    }

    public async Task BuildAgentsData(List<SMSMessage> sMS)
    {
        await Clients.All.SendAsync("BuildAgentsData", sMS);
    }
}
