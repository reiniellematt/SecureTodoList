using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTodoList.Api.Data;
using SecureTodoList.Api.Models;

namespace SecureTodoList.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TodoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/todo
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var items = await _context.TodoItems.ToListAsync();

            return Ok(items);
        }

        // POST api/todo
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TodoItem newItem)
        {
            _context.Entry(newItem).State = EntityState.Added;

            var ok = await _context.SaveChangesAsync();

            if (ok.Equals(1))
            {
                // Will return the newly created item.
                return Ok(newItem);
            }
            else
            {
                return BadRequest();
            }
        }

        // PUT api/todo/markdone/{id}
        [HttpPut("markdone/{id}")]
        public async Task<IActionResult> PutMarkDone(int id)
        {
            var item = await _context.TodoItems.FirstOrDefaultAsync(t => t.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            item.IsDone = true;

            _context.Entry(item).State = EntityState.Modified;

            var ok = await _context.SaveChangesAsync();

            if (ok.Equals(1))
            {
                return Ok();
            }
            else
            {
                return StatusCode(500);
            }
        }

    }
}