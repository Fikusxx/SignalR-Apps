using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppApi.Materials;

//[Authorize]
public class ChatHub : Hub<IChatClientActions>
{
	private readonly ChatDB database;

	public ChatHub(ChatDB database)
	{
		this.database = database;
	}

	public async Task<List<OutputMessage>> JoinRoom(RoomRequest request)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, request.Room);
		await Clients.All.RecieveNotification("Someone joined");

		return database.GetMessages(request.Room)
			.Select(m => m.Output)
			.ToList();
	}

	public async Task LeaveRoom(RoomRequest request)
	{
		await Clients.All.RecieveNotification("Someone left");
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, request.Room);
	}

	public Task SendMessage(InputMessage message)
	{
		//var username = Context.User.Claims.FirstOrDefault(x => x.Type == "name").Value; // JwtRegisteredClaimNames.Name
		var username = "User #1";

		//var userMessage = new UserMessage(new(Context.UserIdentifier, username), message.Message, message.Room, DateTimeOffset.Now);
		var userMessage = new UserMessage(new("someUserId", username), message.Message, message.Room, DateTimeOffset.Now);

		database.AddMessage(message.Room, userMessage);

		return Clients.GroupExcept(message.Room, new[] { Context.ConnectionId })
			.RecieveMessage(userMessage.Output);
	}
}

public interface IChatClientActions
{
	public Task RecieveMessage(OutputMessage message);
	public Task RecieveNotification(string notification);
}
