using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Data;
using NotificationService.Data.Models;
using NotificationService.Web.Controllers;
using NotificationService.Web.Models;
using Xunit;

namespace NotificationService.UnitTests.Web
{
    /// <summary>
    /// Класс с тестами контроллера.
    /// </summary>
    public class MessageTemplatesControllerTests
    {
        private const string TemplateName = "Default template name";

        private readonly ILogger<MessageTemplatesController> logger;
        private readonly PageInfo pageInfoRequest;
        private readonly Template templateModelRequest;
        
        /// <summary>
        /// Конструктор, инициализирует логгер.
        /// </summary>
        public MessageTemplatesControllerTests()
        {
            logger = new Mock<ILogger<MessageTemplatesController>>().Object;
            pageInfoRequest = new PageInfo();
            templateModelRequest = new Template { Name = TemplateName};
        }

        /// <summary>
        /// Тест проверяет статус код при успешном добавлении шаблона.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task Add_Should_Return_Created()
        {
            // Arrange.
            MessageTemplatesController controller = GetController();
            templateModelRequest.Name = "new";

            // Act.
            var result = await controller.Add(templateModelRequest) as ObjectResult;
            
            // Assert.
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        }

        /// <summary>
        /// Тест проверяет статус код при возникновении исключения в менеджере шаблонов.
        /// </summary>
        /// <param name="exceptionType">Тип исключения.</param>
        /// <returns>Тест.</returns>
        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(InvalidOperationException))]
        public async Task Add_Should_Return_Error_When_Exception_Was_Thrown(Type exceptionType)
        {
            // Arrange.
            var manager = new Mock<ITemplateManager>();
            manager.Setup(m => m.AddTemplate(It.IsAny<MessageTemplate>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(exceptionType));
            MessageTemplatesController controller = GetController(manager);

            // Act.
            var result = await Assert.ThrowsAnyAsync<Exception>(() => controller.Add(templateModelRequest));
            
            // Assert.
            Assert.Equal(exceptionType, result.GetType());
        }

        /// <summary>
        /// Тест проверяет статус код при успешном обновлении шаблона.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task Update_Should_Return_Ok()
        {
            // Arrange.
            var manager = new Mock<ITemplateManager>();
            manager.Setup(m => m.UpdateTemplate(It.IsAny<MessageTemplate>()))
                .ReturnsAsync(true);
            MessageTemplatesController controller = GetController(manager);
            
            // Act.
            var result = await controller.Update(templateModelRequest) as ObjectResult;

            // Assert.
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        /// <summary>
        /// Тест проверяет статус код при возникновении исключения в менеджере шаблонов.
        /// </summary>
        /// <param name="exceptionType">Тип исключения.</param>
        /// <returns>Тест.</returns>
        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(InvalidOperationException))]
        public async Task Update_Should_Return_Error_When_Exception_Was_Thrown(Type exceptionType)
        {
            // Arrange.
            var manager = new Mock<ITemplateManager>();
            manager.Setup(m => m.UpdateTemplate(It.IsAny<MessageTemplate>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(exceptionType));
            MessageTemplatesController controller = GetController(manager);
            
            // Act.
            var result = await Assert.ThrowsAnyAsync<Exception>(() => controller.Update(templateModelRequest));
            
            // Assert.
            Assert.Equal(exceptionType, result.GetType());
        }
        
        /// <summary>
        /// Тест проверяет статус код при успешном удалении шаблона.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task Delete_Should_Return_Ok()
        {
            // Arrange.
            var manager = new Mock<ITemplateManager>();
            manager.Setup(m => m.DeleteTemplate(It.IsAny<string>()))
                .ReturnsAsync(true);

            MessageTemplatesController controller = GetController(manager);
            
            // Act.
            var result = await controller.Delete(TemplateName) as ObjectResult;
            
            // Assert.
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        /// <summary>
        /// Тест проверяет статус код при успешном удалении всех шаблонов.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task DeleteAll_ResponseEqualToOk()
        {
            // Arrange.
            var manager = new Mock<ITemplateManager>();
            manager.Setup(m => m.DeleteAllTemplates())
                .ReturnsAsync(true);

            MessageTemplatesController controller = GetController(manager);
            
            // Act.
            var result = await controller.DeleteAll() as ObjectResult;
            
            // Assert.
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }


        /// <summary>
        /// Тест проверяет статус код при возникновении исключения в менеджере шаблонов.
        /// </summary>
        /// <param name="exceptionType">Тип исключения.</param>
        /// <returns>Тест.</returns>
        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(InvalidOperationException))]
        public async Task Delete_Should_Return_Error_When_Exception_Was_Thrown(Type exceptionType)
        {
            // Arrange.
            var manager = new Mock<ITemplateManager>();
            manager.Setup(m => m.DeleteTemplate(It.IsAny<string>()))
                .ThrowsAsync((Exception)Activator.CreateInstance(exceptionType));

            MessageTemplatesController controller = GetController(manager);
            
            // Act.
            var result = await Assert.ThrowsAnyAsync<Exception>(() => controller.Delete(TemplateName));

            // Assert.
            Assert.Equal(exceptionType, result.GetType());
        }

        /// <summary>
        /// Тест проверяет возвращение шаблона по имени.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
        [Fact]
        public async Task Get_Should_Return_Valid_Template()
        {
            // Arrange.
            var template = new MessageTemplate();
            var manager = new Mock<ITemplateManager>();
            manager.Setup(m => m.GetTemplate(It.IsAny<string>()))
                .ReturnsAsync(template);

            MessageTemplatesController controller = GetController(manager);
            
            // Act.
            ActionResult<MessageTemplate> result = await controller.Get(TemplateName);

            // Assert.
            Assert.Equal(template, result.Value);
        }

        /// <summary>
        /// Тест проверяет статус код при возникновении исключения в менеджере шаблонов.
        /// </summary>
        /// <param name="exceptionType">Тип исключения.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(InvalidOperationException))]
        public async Task Get_Should_Return_Error_When_Exception_Was_Thrown(Type exceptionType)
        {
            // Arrange.
            var manager = new Mock<ITemplateManager>();
            manager.Setup(m => m.GetTemplate(It.IsAny<string>()))
                .Throws((Exception)Activator.CreateInstance(exceptionType));

            MessageTemplatesController controller = GetController(manager);
            
            // Act.
            var result = await Assert.ThrowsAnyAsync<Exception>(() => controller.Get(TemplateName));

            // Assert.
            Assert.Equal(exceptionType, result.GetType());
        }
        
        /// <summary>
        /// Тест проверяет возвращение шаблона по имени.
        /// </summary>
        [Fact]
        public void GetAll_Should_Return_Valid_Templates()
        {
            // Arrange.
            var model = new AllTemplates();
            var manager = new Mock<ITemplateManager>();
            manager.Setup(m => m.GetTemplatesPaged(It.IsAny<PageInfo>()))
                .Returns(model);
            
            MessageTemplatesController controller = GetController(manager);
            
            // Act.
            ActionResult<AllTemplates> result = controller.GetAll(pageInfoRequest);

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(model, result.Value);
        }

        /// <summary>
        /// Тест проверяет статус код при возникновении исключения в менеджере шаблонов.
        /// </summary>
        /// <param name="exceptionType">Тип исключения.</param>
        [Theory]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(InvalidOperationException))]
        public void GetAll_Should_Return_Error_When_Exception_Was_Thrown(Type exceptionType)
        {
            // Arrange.
            var manager = new Mock<ITemplateManager>();
            manager.Setup(m => m.GetTemplatesPaged(It.IsAny<PageInfo>()))
                .Throws((Exception)Activator.CreateInstance(exceptionType));

            MessageTemplatesController controller = GetController(manager);
            
            // Act.
            var result = Assert.ThrowsAny<Exception>(() => controller.GetAll());

            // Assert.
            Assert.Equal(exceptionType, result.GetType());
        }

        /// <summary>
        /// Создает и возвращает контроллер.
        /// </summary>
        /// <param name="manager">Менеджер шаблонов.</param>
        /// <returns>Экземпляр контроллера.</returns>
        private MessageTemplatesController GetController(Mock<ITemplateManager> manager = null)
        {
            return new MessageTemplatesController(manager?.Object ?? new Mock<ITemplateManager>().Object, logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = null
                    }
                }
            };
        }
    }
}
