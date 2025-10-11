using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CirendsAPI.DTOs;
using CirendsAPI.Services;
using CirendsAPI.Exceptions;
using System.Security.Claims;
using UnauthorizedAccessException = CirendsAPI.Exceptions.UnauthorizedAccessException2;

namespace CirendsAPI.Controllers
{
    [ApiController]
    [Route("api/activities/{activityId}/tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IMapper _mapper;

        public TasksController(ITaskService taskService, IMapper mapper)
        {
            _taskService = taskService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks(int activityId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var tasks = await _taskService.GetTasksAsync(activityId, userId);
                var taskDtos = _mapper.Map<IEnumerable<TaskItemDto>>(tasks);
                return Ok(taskDtos);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDto>> GetTask(int activityId, int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var task = await _taskService.GetTaskAsync(activityId, id, userId);
                
                if (task == null)
                    return NotFound();

                var taskDto = _mapper.Map<TaskItemDto>(task);
                return Ok(taskDto);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<TaskItemDto>> CreateTask(int activityId, CreateTaskDto createTaskDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var task = await _taskService.CreateTaskAsync(activityId, userId, createTaskDto);
                var taskDto = _mapper.Map<TaskItemDto>(task);
                
                return CreatedAtAction(nameof(GetTask), new { activityId, id = task.Id }, taskDto);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int activityId, int id, UpdateTaskDto updateTaskDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _taskService.UpdateTaskAsync(activityId, id, userId, updateTaskDto);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int activityId, int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _taskService.DeleteTaskAsync(activityId, id, userId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}