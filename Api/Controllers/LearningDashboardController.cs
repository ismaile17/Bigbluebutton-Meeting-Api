using Application.LearningDashboards.Commands.CreateLearningDashboard;
using Application.LearningDashboards.Queries.GetMeetingByMeetingId;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LearningDashboardController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LearningDashboardController> _logger;

        public LearningDashboardController(IMediator mediator, ILogger<LearningDashboardController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLearningDashboard([FromBody] CreateLearningDashboardCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }


        [HttpGet("{meetingId}")]
        public async Task<IActionResult> GetDashboardByMeetingId(string meetingId)
        {
            var query = new GetDashboardByMeetingIdQuery(meetingId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        private bool ValidateToken(string token)
        {
            try
            {
                var secret = "YOUR_SECRET_KEY"; 
                var key = Encoding.ASCII.GetBytes(secret);

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero 
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering(); 

            request.Body.Position = 0;

            // İstek gövdesini oku
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0; 
                return body;
            }
        }

        private void LogRequest(HttpRequest request, string body)
        {
            var logFilePath = "logs/requests.txt";
            EnsureDirectoryExists(logFilePath);

            var formattedRequest = new StringBuilder();
            var trTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            var trDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, trTimeZone);

            formattedRequest.AppendLine($"Timestamp: {trDateTime.ToString("MM/dd/yyyy HH:mm:ss")}");
            formattedRequest.AppendLine($"Method: {request.Method}");
            formattedRequest.AppendLine($"Path: {request.Path}");
            formattedRequest.AppendLine($"Body: {body}");
            formattedRequest.AppendLine();

            System.IO.File.AppendAllText(logFilePath, formattedRequest.ToString());
        }

        private void LogCommand(CreateLearningDashboardCommand command)
        {
            var logFilePath = "logs/commands.txt";
            EnsureDirectoryExists(logFilePath);

            var formattedCommand = new StringBuilder();
            var trTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            var trDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, trTimeZone);

            formattedCommand.AppendLine($"Timestamp: {trDateTime.ToString("MM/dd/yyyy HH:mm:ss")}");
            formattedCommand.AppendLine($"Command: {Newtonsoft.Json.JsonConvert.SerializeObject(command)}");
            formattedCommand.AppendLine();

            System.IO.File.AppendAllText(logFilePath, formattedCommand.ToString());
        }

        private void LogResponse(object response)
        {
            var logFilePath = "logs/responses.txt";
            EnsureDirectoryExists(logFilePath);

            var formattedResponse = new StringBuilder();
            var trTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            var trDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, trTimeZone);

            formattedResponse.AppendLine($"Timestamp: {trDateTime.ToString("MM/dd/yyyy HH:mm:ss")}");
            formattedResponse.AppendLine($"Response: {response}");
            formattedResponse.AppendLine();

            System.IO.File.AppendAllText(logFilePath, formattedResponse.ToString());
        }

        private void LogError(Exception ex)
        {
            var logFilePath = "logs/errors.txt";
            EnsureDirectoryExists(logFilePath);

            var formattedError = new StringBuilder();
            var trTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            var trDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, trTimeZone);

            formattedError.AppendLine($"Timestamp: {trDateTime.ToString("MM/dd/yyyy HH:mm:ss")}");
            formattedError.AppendLine($"Error: {ex.Message}");
            formattedError.AppendLine($"StackTrace: {ex.StackTrace}");
            formattedError.AppendLine();

            System.IO.File.AppendAllText(logFilePath, formattedError.ToString());
        }

        private void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
