using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SecureTodoList.Web.Models;

namespace SecureTodoList.Web.Pages.Todo
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        [BindProperty]
        public TodoItem NewItem { get; set; } = new TodoItem();

        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var token = await HttpContext.GetTokenAsync("access_token");

            using (var client = _clientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.BaseAddress = new Uri("https://localhost:5001");
                client.SetBearerToken(token);

                var content = new StringContent(JsonConvert.SerializeObject(NewItem), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("/api/todo", content);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500);
                }

                return RedirectToPage("./Index");
            }
        }


    }
}