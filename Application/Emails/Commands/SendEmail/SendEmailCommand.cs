using Application.Emails.Model;
using Application.Shared.Results;
using MailKit.Net.Smtp;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using MimeKit;

namespace Application.Emails.Commands.SendEmail
{
    public class SendEmailCommand : IRequest<ResultSingle<EmailDto>>
    {
        public string FullName { get; set; }
        public string Email { get; set; }

        public int Type { get; set; }

        public string Body { get; set; }

        public string Subject { get; set; }
        public string ButtonText { get; set; }
        public string Url { get; set; }
        public string Information { get; set; }

        public string Display { get; set; }

    }

    public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, ResultSingle<EmailDto>>
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _environment;

        public SendEmailCommandHandler( IMediator mediator, IWebHostEnvironment environment)
        {
            _mediator = mediator;
            _environment = environment;
        }

        public async Task<ResultSingle<EmailDto>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {

            if (request.Email == null)
            {
                return Result.Fail<EmailDto>(null, "Email bulunamadı");
            }

            var email = new MimeMessage();

            var webRoot = _environment.WebRootPath;

            string pathToFile = null;
            if (request.Type == 1 || request.Type == 2 || request.Type == 3 || request.Type == 4)
            {
                pathToFile = _environment.WebRootPath + Path.DirectorySeparatorChar.ToString() + "template" + Path.DirectorySeparatorChar.ToString() + "email" + Path.DirectorySeparatorChar.ToString() + "index.html";
            }

            var builder = new BodyBuilder();
            using (StreamReader sourceReader = System.IO.File.OpenText(pathToFile))
            {
                builder.HtmlBody = sourceReader.ReadToEnd();
            }
            builder.HtmlBody = builder.HtmlBody.Replace("[AdSoyad]", request.FullName);
            builder.HtmlBody = builder.HtmlBody.Replace("[ButtonText]", request.ButtonText);
            builder.HtmlBody = builder.HtmlBody.Replace("[ButtonUrl]", request.Url);
            builder.HtmlBody = builder.HtmlBody.Replace("[Body]", request.Body);
            builder.HtmlBody = builder.HtmlBody.Replace("[Information]", request.Information);
            builder.HtmlBody = builder.HtmlBody.Replace("[Display]", request.Display);
            var body = builder.HtmlBody;
            email.From.Add(new MailboxAddress("", ""));
            email.To.Add(new MailboxAddress($"{request.FullName}", request.Email));

            email.Subject = request.Subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, false);

                smtp.Authenticate("", "");

                smtp.Send(email);
                smtp.Disconnect(true);
            }
            return Result.Ok(new EmailDto { }, $"Başarılı");
        }


    }


}

