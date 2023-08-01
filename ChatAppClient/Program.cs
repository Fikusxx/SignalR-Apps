using Microsoft.AspNetCore.SignalR.Client;


const string room = "red room";
var client = new ChatClient();
var joinRequest = new RoomRequest(room);
await client.JoinRoom(joinRequest);

while (true)
{
	var message = Console.ReadLine();

	if (message == "exit")
	{
		var leaveRequest = new RoomRequest(room);
		await client.LeaveRoom(leaveRequest);
		break;
	}

	var input = new InputMessage(message!, room);
	await client.SendMessage(input);
}

public interface IChatClientNotifications
{
	public Task RecieveMessage(OutputMessage message);
	public Task RecieveNotification(string message);
}

public interface IChatClient
{
	public Task SendMessage(InputMessage message);
	public Task JoinRoom(RoomRequest request);
	public Task LeaveRoom(RoomRequest request);
}

public class ChatClient : IChatClientNotifications, IChatClient
{
	private readonly HubConnection hubConnection;

	public ChatClient()
	{
		// connect
		hubConnection = new HubConnectionBuilder().WithUrl("http://localhost:5000/chat", opt =>
		{
			opt.Headers.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxZTQzOGQ5Zi03OWZkLTRjMjAtOTVlNS1hNDBkZWUxYTI5MzQiLCJuYW1lIjoiVXNlciAjMSIsImFkbWluIjoidHJ1ZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6ImFkbWluIiwiZXhwIjoxNjkwOTcxODEzLCJpc3MiOiJzb21lSXNzdWVyIiwiYXVkIjoic29tZUF1ZGllbmNlIn0._M7WLKJZgz-PcJJxX_tzaHYq4QWOJxOnWCL6rR7PeqU");
		}).Build();

		// subscribe to events
		hubConnection.On<OutputMessage>(nameof(RecieveMessage), RecieveMessage);
		hubConnection.On<string>(nameof(RecieveNotification), RecieveNotification);

		// start client
		hubConnection.StartAsync().GetAwaiter().GetResult();
	}

	public async Task RecieveMessage(OutputMessage message)
	{
		await Console.Out.WriteLineAsync(message.ToString());
	}

	public async Task RecieveNotification(string message)
	{
		await Console.Out.WriteLineAsync(message);
	}

	public async Task SendMessage(InputMessage message)
	{
		await hubConnection.InvokeAsync(nameof(SendMessage), message);
	}

	public async Task JoinRoom(RoomRequest request)
	{
		var result = await hubConnection.InvokeAsync<List<OutputMessage>>(nameof(JoinRoom), request);

		result.ForEach(x =>
		{
			Console.WriteLine(x.ToString());
		});
	}

	public async Task LeaveRoom(RoomRequest request)
	{
		await hubConnection.InvokeAsync(nameof(LeaveRoom), request);
	}
}


public record InputMessage(string Message, string Room);
public record OutputMessage(string Message, string UserName, string Room, DateTimeOffset SentAt);
public record RoomRequest(string Room);
