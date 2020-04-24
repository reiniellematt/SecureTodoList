using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SecureTodoList.Web.Models;

namespace SecureTodoList.Web.Pages.Todo
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public List<TodoItem> TodoItems { get; set; } = new List<TodoItem>();

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task OnGet()
        {
            using (var client = _clientFactory.CreateClient())
            {
                var response = await client.GetAsync("https://localhost:5001/api/todo");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(response.ReasonPhrase);
                }
                else
                {
                    var message = await response.Content.ReadAsStringAsync();

                    TodoItems = JsonConvert.DeserializeObject<List<TodoItem>>(message);
                }
            }
        }

        // TODO: Integrate with UI and fix to make it more secure.
        public async Task<IActionResult> OnPostMarkDoneAsync(int itemId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            using (var client = _clientFactory.CreateClient())
            {
                var response = await client.PutAsync($"https://localhost:5001/api/todo/markdone/{itemId}", null);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500);
                }

                return RedirectToPage();
            }

        }
    }
}