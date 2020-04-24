using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NotificationService.Data;
using NotificationService.Data.Models;
using NotificationService.Web.Models;

namespace NotificationService.Web.Controllers
{
    /// <summary>
    /// Представляет api для управления шаблонами сообщений.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(IAsyncAuthorizationFilter))]
    public class MessageTemplatesController : ControllerBase
    {
        private const string LogMessageFormat = "{0} request from user with id {1} for message template named {2}";
        private const string GetAllLogMessageFormat = "GetAll message templates request from user with id {0}";
        private const string DeleteAllLogMessageFormat = "DeleteAll message templates request from user with id {0}";

        private readonly ITemplateManager templateManager;
        private readonly ILogger<MessageTemplatesController> logger;

        /// <summary>
        /// Конструктор, инициализируем репозиторий с шаблонам сообщений.
        /// </summary>
        /// <param name="templateManager">Менеджер шаблонов сообщений.</param>
        /// <param name="logger">Для логирования действий.</param>
        public MessageTemplatesController(ITemplateManager templateManager, ILogger<MessageTemplatesController> logger)
        {
            this.templateManager = templateManager;
            this.logger = logger;
        }

        /// <summary>
        /// Идентификатор пользователя, от чьего имени выполняется запрос.
        /// </summary>
        private string UserId => User.FindFirstValue("Id");

        /// <summary>
        /// Добавляет новый шаблон.
        /// </summary>
        /// <param name="templateModel">Описание полей шаблона.</param>
        /// <returns>Результат добавления шаблона.</returns>
        [HttpPost]
        public async Task<IActionResult> Add(Template templateModel)
        {
            logger.LogInformation(LogMessageFormat, "Add", UserId, templateModel.Name);
            
            await templateManager.AddTemplate(new MessageTemplate
            {
                Name = templateModel.Name,
                Parent = templateModel.Parent,
                Title = templateModel.Title,
                Body = templateModel.Body,
                EngineType = templateModel.Engine,
                CreatedBy = UserId,
                CreatedDate = DateTime.Now
            });

            return StatusCode((int)HttpStatusCode.Created, await templateManager.GetTemplate(templateModel.Name));
        }

        /// <summary>
        /// Обновляет существующий шаблон по имени.
        /// </summary>
        /// <param name="templateModel">Описание полей шаблона.</param>
        /// <returns>Результат обновления шаблона.</returns>
        [HttpPut]
        public async Task<IActionResult> Update(Template templateModel)
        {
            logger.LogInformation(LogMessageFormat, "Update", UserId, templateModel.Name);

            return await templateManager.UpdateTemplate(new MessageTemplate
            {
                Name = templateModel.Name,
                Parent = templateModel.Parent,
                Title = templateModel.Title,
                Body = templateModel.Body,
                EngineType = templateModel.Engine,
                UpdatedBy = UserId,
                UpdatedDate = DateTime.Now
            }) ? Ok("Template successfully updated") :
                StatusCode((int)HttpStatusCode.NotAcceptable, $"Update operation doesn't complete successfully for {templateModel.Name}");
        }

        /// <summary>
        /// Возвращает существующий шаблон по имени.
        /// </summary>
        /// <param name="templateName">Имя шаблона.</param>
        /// <returns>Задача <seealso cref="Task"/> с результатом операции удаления шаблона.</returns>
        [HttpGet("{templateName}")]
        public async Task<ActionResult<MessageTemplate>> Get(string templateName)
        {
            logger.LogInformation(LogMessageFormat, "Get", UserId, templateName);

            return await templateManager.GetTemplate(templateName);
        }
        
        /// <summary>
        /// Возвращает список всех шаблонов постранично.
        /// </summary>
        /// <param name="pageInfo">Информация о выводимой странице.</param>
        /// <returns>Возвращает указанное число элементов на странице.</returns>
        [HttpGet]
        public ActionResult<AllTemplates> GetAll([FromQuery]PageInfo pageInfo = null)
        {
            logger.LogInformation(GetAllLogMessageFormat, UserId);
            return templateManager.GetTemplatesPaged(pageInfo);
        }

        /// <summary>
        /// Удаляет существующий шаблон по имени.
        /// </summary>
        /// <param name="templateName">Имя шаблона.</param>
        /// <returns>Результат операции удаления шаблона.</returns>
        [HttpDelete("{templateName}")]
        public async Task<IActionResult> Delete(string templateName)
        {
            logger.LogInformation(LogMessageFormat, "Removal", UserId, templateName);
            return await templateManager.DeleteTemplate(templateName) ? 
                    Ok("Template successfully deleted") :
                    StatusCode((int)HttpStatusCode.NotAcceptable, $"Delete operation doesn't complete successfully for {templateName}");
        }

        /// <summary>
        /// Удаляет все существующие шаблоны.
        /// </summary>
        /// <returns>Результат операции удаления шаблона.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            logger.LogInformation(DeleteAllLogMessageFormat, UserId);
            return await templateManager.DeleteAllTemplates() ? 
                    Ok("All templates successfully deleted") :
                    StatusCode((int)HttpStatusCode.NotAcceptable, "Delete operation doesn't complete successfully");
        }
    }
}
