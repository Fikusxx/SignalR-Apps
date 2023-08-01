using ChatAppApi.Materials;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppApi.Controllers;

[ApiController]
[Route("default")]
public class ChatController : ControllerBase
{
	private readonly ChatDB database;

	public ChatController(ChatDB database)
	{
		this.database = database;
	}

	[HttpGet("/create")]
	public IActionResult CreateRoom()
	{
		var room = "red room";
		database.CreateRoom(room);

		return Ok();
	}

	[HttpGet("/rooms")]
	public IActionResult ListRooms()
	{
		var rooms = database.GetRooms();

		return Ok(rooms);
	}
}
