using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicTacToe.Options;
using TicTacToe.Services.Interfaces;

namespace TicTacToe.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailServiceOptions _emailServiceOptions;

        public EmailService(IOptions<EmailServiceOptions> emailServiceOptions)
        {
            _emailServiceOptions = emailServiceOptions.Value;
        }

        public Task SendEmail(string emailTo, string subject, string message)
        { 
            return Task.CompletedTask;
        }
    }
}
