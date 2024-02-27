const connection = new signalR.HubConnectionBuilder()
    .withUrl("/ChatHub")
    .build();

connection.start().catch(err => console.error(err.toString()));

connection.on('SendMessage', (message) => {        
    
    //appendMessage(message,true);
});
connection.on('ReceiveMessage', (message) => {
   // appendMessage(message,true);
});
connection.on('BuildAgentsData', (data) => {
    //BuildAgents(data, false);
});
