using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using TicTacToe.Models;
using TicTacToe.Services;
using TicTacToe.Services.Interfaces;

namespace TicTacToe.Controllers
{
    public class UserRegistrationController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public UserRegistrationController(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EmailConfirmation(string email)
        {
            var user = await _userService.GetUserByEmail(email);

            var urlAction = new UrlActionContext
            {
                Action = "ConfirmEmail",
                Controller = "UserRegistration",
                Values = new { email },
                Protocol = Request.Scheme,
                Host = Request.Host.ToString()
            };

            var message = "Thanks for registration! To confirm your e-mail address please click here " + $"{Url.Action(urlAction)}";

            try
            {
                _emailService.SendEmail(email, "Confirm e-mail address in tic tac toe game.", message).Wait();
            }
            catch (Exception ex)
            { 
            }

            if (user?.IsEmailConfirmed == true)
                return RedirectToAction("Index", "GameInvitation", new { email = email });
            
            ViewBag.Email = email;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                await _userService.RegisterUser(userModel);
                return RedirectToAction(nameof(EmailConfirmation), new {userModel.Email});
            }
            else
            {
                return View(userModel);
            }
        }
    }
}
