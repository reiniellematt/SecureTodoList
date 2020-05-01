﻿using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SecureTodoList.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecureTodoList.Web.Pages.Todo
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _accessToken;

        public List<TodoItem> TodoItems { get; set; } = new List<TodoItem>();

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task OnGet()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            using (var client = _clientFactory.CreateClient())
            {
                client.SetBearerToken(token);
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