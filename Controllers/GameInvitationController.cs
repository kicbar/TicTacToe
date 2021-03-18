using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using TicTacToe.Models;
using TicTacToe.Services;
using TicTacToe.Services.Interfaces;

namespace TicTacToe.Controllers
{
    public class GameInvitationController : Controller
    {
        private readonly IUserService _userService;
        private IStringLocalizer<GameInvitationController> _stringLocalizer;
        

        public GameInvitationController(IUserService userService, IStringLocalizer<GameInvitationController> stringLocalizer)
        {
            _userService = userService;
            _stringLocalizer = stringLocalizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string email)
        {
            var gameInvitationModel = new GameInvitationModel()
            {
                InvitedBy = email
            };
            HttpContext.Session.SetString("email", email);
            return View(gameInvitationModel);
        }

        [HttpPost]
        public IActionResult Index(GameInvitationModel gameInvitationModel)
        {
            string template = _stringLocalizer["GameInvitationConfirmationMessage"];
            string message = string.Format(template, gameInvitationModel.EmailTo.ToString());
            return Content(message);
        }
    }
}
