using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Targets;
using static System.IO.File;

namespace NotificationService.Web.Controllers
{
    /// <summary>
    /// Представляет api для диагностики состояния сервиса.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticController : ControllerBase
    {
        private readonly ILogger<DiagnosticController> logger;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="logger">Для логирования действий.</param>
        public DiagnosticController(ILogger<DiagnosticController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Проверяет доступность сервиса.
        /// </summary>
        /// <returns>Пустой ответ со статусом 200..</returns>
        [HttpPost]
        public IActionResult Ping()
        {
            logger.LogInformation("Sending response to the ping request");
            return Ok(string.Empty);
        }

        /// <summary>
        /// Возвращает содержимое лога.
        /// </summary>
        /// <returns>Содержимое лога.</returns>
        [HttpGet]
        public IActionResult Log()
        {
            logger.LogInformation("Sending response to the log request");
            
            string file = GetDebugLogFile();
            return Exists(file) ? 
                Ok(ReadAllText(file)) : 
                StatusCode((int)HttpStatusCode.NotFound, $"Debug log file {file} doesn't found");
        }

        /// <summary>
        /// Возвращает путь до файла с логами.
        /// </summary>
        /// <returns>Полный путь до файла с логами.</returns>
        private string GetDebugLogFile()
        {
            FileTarget debugTarget = (FileTarget)LogManager.Configuration.AllTargets
                .FirstOrDefault(t => t.Name.Equals("debugFileTarget"));

            return debugTarget?.FileName
                .ToString()
                .Replace("'", string.Empty);
        }
    }
}