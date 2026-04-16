using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomService.DTOs;
using RoomService.Models;
using RoomService.Services;

namespace RoomService.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _roomService.GetAllRooms();
            return Ok(rooms);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await _roomService.GetRoomById(id);
            if (room == null)
            {
                return NotFound();
            }
            return Ok(room);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("roomstypes/{roomTypeId}")]
        public async Task<IActionResult> GetAllRoomsByType(int roomTypeId)
        {
            var rooms = await _roomService.GetAllRoomsByType(roomTypeId);
            return Ok(rooms);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("by-roomnumber/{roomNumber}")]
        public async Task<IActionResult> GetRoomById(string roomNumber)
        {
            var room = await _roomService.GetRoomById(roomNumber);
            if (room == null)
            {
                return NotFound();
            }
            return Ok(room);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("types")]
        public async Task<IActionResult> CreateRoomType([FromBody] CreateRoomTypeRequest request)
        {
            var roomType = await _roomService.CreateRoomType(request);
            return Ok(roomType);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var room = await _roomService.CreateRoom(request);
            return Ok(room);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{roomNumber}")]
        public async Task<IActionResult> UpdateRoom(
            string roomNumber,
            [FromBody] UpdateRoomRequest request
        )
        {
            await _roomService.UpdateRoom(roomNumber, request);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{roomNumber}")]
        public async Task<IActionResult> DeleteRoom(string roomNumber)
        {
            await _roomService.DeleteRoom(roomNumber);
            return NoContent();
        }
    }
}
