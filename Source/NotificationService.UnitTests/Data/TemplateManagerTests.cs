using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using NotificationService.Data;
using NotificationService.Data.Models;
using NotificationService.Data.Repositories;
using Xunit;

namespace NotificationService.UnitTests.Data
{
    /// <summary>
    /// Класс с тестами менеджера шаблонов.
    /// </summary>
    public class TemplateManagerTests
    {
        private const string TestTemplateName = "Test template";
        private const string TestTemplateTitle= "Test title";
        private const string TestTemplateBody = "Test body";

        /// <summary>
        /// Список шаблонов.
        /// </summary>
        private List<MessageTemplate> Templates { get; } = new List<MessageTemplate>
        {
            new MessageTemplate { Name = "parent", Body = TestTemplateBody },
            new MessageTemplate { Name = "child", Body = TestTemplateBody, Parent = "parent" }
        };
        
        /// <summary>
        /// Тест проверяет добавление шаблона в репозиторий.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task AddTemplate_Should_Create_Template_In_Repository()
        {
            // Arrange.
            ITemplateManager manager = GetManager();
            var template = new MessageTemplate
            {
                Name = TestTemplateName, 
                Title = TestTemplateTitle, 
                Body = TestTemplateBody
            };

            // Act.
            await manager.AddTemplate(template);

            // Assert.
            MessageTemplate created = await manager.GetTemplate(TestTemplateName);
            Assert.NotNull(created);
            Assert.Equal(template, created);
        }

        /// <summary>
        /// Тест выполняет проверку добавления шаблона с некорректными полями.
        /// </summary>
        /// <param name="name">Название шаблона.</param>
        /// <param name="parent">Имя родительского шаблона.</param>
        /// <param name="title">Заголовок письма.</param>
        /// <param name="body">Тело шаблона.</param>
        /// <param name="engine">Движок шаблонизатора.</param>
        /// <param name="expectedMessage">Ожидаемое сообщение с ошибкой.</param>
        /// <returns>Тест.</returns>
        [Theory]
        [InlineData("Parent", "", TestTemplateTitle, TestTemplateBody, Engines.Razor, "Template with the given name parent already exists")]
        [InlineData(TestTemplateName, "Parent", TestTemplateTitle, TestTemplateBody, Engines.Jinja, "The engine types of parent and child templates should be match")]
        [InlineData(TestTemplateName, "child", TestTemplateTitle, TestTemplateBody, Engines.Razor, "Parent template child shouldn't have own parents")]
        [InlineData(TestTemplateName, "not_existing", TestTemplateTitle, TestTemplateBody, Engines.Razor, "Couldn't found parent template named not_existing")]
        public async Task AddTemplate_Should_Throw_Exception_On_Incorrect_Template_Fields(
            string name, string parent, string title, string body, Engines engine, string expectedMessage)
        {
            // Arrange.
            ITemplateManager manager = GetManager();
            var template = new MessageTemplate
            {
                Name = name,
                Title = title,
                Body = body,
                Parent = parent,
                EngineType = engine
            };

            // Act.
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(() => manager.AddTemplate(template));

            // Assert.
            Assert.Equal(expectedMessage, exception.Message
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty));
        }

        /// <summary>
        /// Тест проверяет удаление всех шаблонов из репозитория.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task DeleteAllTemplates_TemplatesCountEqualsTo0()
        {
            // Arrange.
            ITemplateManager manager = GetManager();

            // Act.
            bool result = await manager.DeleteAllTemplates();

            // Assert.
            Assert.True(result);
            Assert.Empty(Templates);
        }

        /// <summary>
        /// Тест проверяет удаление шаблона из репозитория.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task DeleteTemplate_Should_Delete_Template_From_Repository()
        {
            // Arrange.
            ITemplateManager manager = GetManager();
            var name = "Child";

            // Act.
            bool result = await manager.DeleteTemplate(name);

            // Assert.
            Assert.True(result);
            Assert.DoesNotContain(Templates, template => template.Name.Equals(name));
        }

        /// <summary>
        /// Тест выполняет проверку удаления шаблона с некорректным запросом.
        /// </summary>
        /// <param name="templateName">Название удаляемого шаблона.</param>
        /// <param name="expectedMessage">Ожидаемое сообщение с ошибкой.</param>
        /// <returns>Тест.</returns>
        [Theory]
        [InlineData(TestTemplateName, "Couldn't found template named test template")]
        [InlineData("Parent", "Can't delete parent template with linked child templates")]
        public async Task DeleteTemplate_Should_Throw_Exception_On_Incorrect_Input_Data(string templateName, string expectedMessage)
        {
            // Arrange.
            ITemplateManager manager = GetManager();

            // Act.
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(() => manager.DeleteTemplate(templateName));
            
            // Assert.
            Assert.Equal(expectedMessage, exception.Message);
        }
        
        /// <summary>
        /// Тест проверяет негативный результат удаления шаблона.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task DeleteTemplate_Should_Fail_On_Negative_DeleteResult()
        {
            // Arrange.
            Mock<DeleteResult> deleteResultMock = GetDeleteResultMock();
            deleteResultMock.Setup(deleteResult => deleteResult.IsAcknowledged).Returns(false);
            Mock<IMessageTemplatesRepository> repositoryMock = GetRepositoryMock(deleteResultMock);
            ITemplateManager manager = GetManager(repositoryMock);
            var name = "Child";

            // Act.
            bool result = await manager.DeleteTemplate(name);

            // Assert.
            Assert.False(result);
        }

        /// <summary>
        /// Тест проверяет удаление шаблона из репозитория.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task UpdateTemplate_Should_Update_Template_In_Repository()
        {
            // Arrange.
            ITemplateManager manager = GetManager();
            var name = "parent";
            var template = new MessageTemplate
            {
                Name = name, 
                Body = TestTemplateName, 
                Title = TestTemplateName
            };

            // Act.
            bool result = await manager.UpdateTemplate(template);

            // Assert.
            Assert.True(result);
            
            MessageTemplate updated = Templates.FirstOrDefault(temp => temp.Name.Equals(name));
            Assert.NotEmpty(updated.Body);
            Assert.NotEmpty(updated.Title);
        }

        /// <summary>
        /// Тест выполняет проверку обновления шаблона с некорректными полями.
        /// </summary>
        /// <param name="name">Название шаблона.</param>
        /// <param name="parent">Имя родительского шаблона.</param>
        /// <param name="title">Заголовок письма.</param>
        /// <param name="body">Тело шаблона.</param>
        /// <param name="engine">Движок шаблонизатора.</param>
        /// <param name="expectedMessage">Ожидаемое сообщение с ошибкой.</param>
        /// <returns>Тест.</returns>
        [Theory]
        [InlineData("Child", "Parent", TestTemplateTitle, TestTemplateBody, Engines.Jinja, "The engine types of parent and child templates should be match")]
        [InlineData("Parent", "", TestTemplateTitle, TestTemplateBody, Engines.Jinja, "The engine types of parent and child templates should be match")]
        [InlineData("Parent", "Child", TestTemplateTitle, TestTemplateBody, Engines.Razor, "Parent template child shouldn't have own parents")]
        [InlineData("Child", "not_existing", TestTemplateTitle, TestTemplateBody, Engines.Razor, "Couldn't found parent template named not_existing")]
        [InlineData(TestTemplateName, "", TestTemplateTitle, TestTemplateBody, Engines.Razor, "Couldn't found template named test template")]
        public async Task UpdateTemplate_Should_Throw_Exception_On_Incorrect_Input_Data(
            string name, string parent, string title, string body, Engines engine, string expectedMessage)
        {
            // Arrange.
            ITemplateManager manager = GetManager();
            var template = new MessageTemplate
            {
                Name = name,
                Title = title,
                Body = body,
                Parent = parent, 
                EngineType = engine
            };

            // Act.
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(() => manager.UpdateTemplate(template));
            
            // Assert.
            Assert.Equal(expectedMessage, exception.Message
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty));
        }

        /// <summary>
        /// Тест проверяет негативный результат обновления шаблона.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task UpdateTemplate_Should_Fail_On_Negative_UpdateResult()
        {
            // Arrange.
            Mock<UpdateResult> updateResultMock = GetUpdateResultMock();
            updateResultMock.Setup(deleteResult => deleteResult.IsAcknowledged).Returns(false);
            Mock<IMessageTemplatesRepository> repositoryMock = GetRepositoryMock(updateResultMock: updateResultMock);
            ITemplateManager manager = GetManager(repositoryMock);

            // Act.
            bool result = await manager.UpdateTemplate(new MessageTemplate
            {
                Name = "Child",
                Title = TestTemplateTitle,
                Body = TestTemplateBody
            });

            // Assert.
            Assert.False(result);
        }

        /// <summary>
        /// Тест проверяет возвращение шаблона из репозитория.
        /// </summary>
        /// <param name="name">Название шаблона.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
        [Theory]
        [InlineData("parent")]
        [InlineData("child")]
        public async Task GetTemplate_Should_Return_Template(string name)
        {
            // Arrange.
            ITemplateManager manager = GetManager();

            // Act.
            MessageTemplate result = await manager.GetTemplate(name);

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
        }

        /// <summary>
        /// Тест проверяет возвращение исключения об отсутствии шаблона.
        /// </summary>
        /// <param name="name">Название шаблона.</param>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
        [Theory]
        [InlineData("not_exist")]
        public async Task GetTemplate_Should_Throw_Exception_On_Not_Existing_Template(string name)
        {
            // Arrange.
            ITemplateManager manager = GetManager();

            // Act.
            var exception = await Assert.ThrowsAnyAsync<Exception>(() => manager.GetTemplate(name));
            
            // Assert.
            Assert.Equal($"Couldn't found template named {name}", exception.Message);
        }

        /// <summary>
        /// Тест проверяет генерацию исключения при неверной модели страницы.
        /// </summary>
        /// <param name="currentPage">Текущая страница.</param>
        /// <param name="pageSize">Размер выводимой страницы.</param>
        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(1, 2)]
        public void GetTemplatesPaged_Should_Return_Correct_AllTemplatesModel(int currentPage, int pageSize)
        {
            // Arrange.
            ITemplateManager manager = GetManager();
            var pageInfo = new PageInfo
            {
                CurrentPage = currentPage,
                PageSize = pageSize
            };

            // Act.
            AllTemplates result = manager.GetTemplatesPaged(pageInfo);
            
            // Assert.
            Assert.NotNull(result);
            Assert.NotEmpty(result.Templates);
            Assert.Equal(currentPage, result.CurrentPage);
            Assert.Equal(pageSize, result.PageSize);
        }
        
        /// <summary>
        /// Тест проверяет постраничное возвращение всех шаблонов по умолчанию.
        /// </summary>
        [Fact]
        public void GetTemplatesPaged_By_Default_Should_Return_AllTemplatesModel()
        {
            // Arrange.
            ITemplateManager manager = GetManager();

            // Act.
            AllTemplates result = manager.GetTemplatesPaged(null);

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalTemplates);
            Assert.Equal(2, result.Templates.Count());
            Assert.Equal(2, result.PageSize);
            Assert.Equal(1, result.PagesCount);
        }

        /// <summary>
        /// Тест проверяет постраничное возвращение всех шаблонов при отсутствии шаблонов.
        /// </summary>
        [Fact]
        public void GetTemplatesPaged_Should_Return_Empty_AllTemplatesModel()
        {
            // Arrange.
            ITemplateManager manager = GetManager();
            Templates.Clear();

            // Act.
            AllTemplates result = manager.GetTemplatesPaged(null);

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalTemplates);
            Assert.Empty(result.Templates);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.PagesCount);
        }
        
        /// <summary>
        /// Тест проверяет генерацию исключения при неверной модели страницы.
        /// </summary>
        /// <param name="currentPage">Текущая страница.</param>
        /// <param name="pageSize">Размер выводимой страницы.</param>
        /// <param name="expectedMessage">Текст исключения.</param>
        [Theory]
        [InlineData(-1, 1, "Value of the CurrentPage should be greater than 0")]
        [InlineData(1, -1, "Value of the PageSize should be greater than 0")]
        [InlineData(2, 3, "Value of the CurrentPage shouldn't be greater than total 1")]
        public void GetTemplatesPaged_Should_Throw_Exception_On_Incorrect_Input_Data(int currentPage, int pageSize, string expectedMessage)
        {
            // Arrange.
            ITemplateManager manager = GetManager();
            var pageInfo = new PageInfo
            {
                CurrentPage = currentPage,
                PageSize = pageSize
            };

            // Act.
            var exception = Assert.Throws<ArgumentException>(() => manager.GetTemplatesPaged(pageInfo));
            
            // Assert.
            Assert.Equal(expectedMessage, exception.Message);
        }

        /// <summary>
        /// Удаляет шаблон по названию из списка шаблонов.
        /// </summary>
        /// <param name="name">Название шаблона.</param>
        private void RemoveTemplate(string name)
        {
            Templates.RemoveAll(template =>
                template.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Создает и возвращает менеджер шаблонов.
        /// </summary>
        /// <param name="repositoryMock">Мок настроенного репозитория.</param>
        /// <returns>Возвращает менеджер шаблонов.</returns>
        private ITemplateManager GetManager(Mock<IMessageTemplatesRepository> repositoryMock = null)
        {
            return new TemplateManager(repositoryMock?.Object ?? GetRepositoryMock().Object,
                new Mock<ILogger<TemplateManager>>().Object);
        }
        
        /// <summary>
        /// Возвращает репозиторий.
        /// </summary>
        /// <param name="deleteResultMock">Мок результата удаления шаблона.</param>
        /// <param name="updateResultMock">Мок результата обновления шаблона.</param>
        /// <returns>Мок репозитория.</returns>
        private Mock<IMessageTemplatesRepository> GetRepositoryMock(
            Mock<DeleteResult> deleteResultMock = null, Mock<UpdateResult> updateResultMock = null)
        {
            var repositoryMock = new Mock<IMessageTemplatesRepository>();
            
            repositoryMock.SetupAllProperties();

            repositoryMock.Setup(repository => repository.Add(It.IsAny<MessageTemplate>()))
                .Callback<MessageTemplate>(template => Templates.Add(template))
                .Returns(Task.CompletedTask);
            repositoryMock.Setup(repository => repository.Delete(It.IsAny<string>()))
                .Callback<string>(name => RemoveTemplate(name))
                .ReturnsAsync(deleteResultMock?.Object ?? GetDeleteResultMock().Object);
            repositoryMock.Setup(repository => repository.DeleteAll())
                .Callback(() => Templates.Clear())
                .ReturnsAsync(deleteResultMock?.Object ?? GetDeleteResultMock().Object);
            repositoryMock.Setup(repository => repository.Update(It.IsAny<MessageTemplate>()))
                .Callback<MessageTemplate>(template =>
                {
                    RemoveTemplate(template.Name);
                    Templates.Add(template);
                })
                .ReturnsAsync(updateResultMock?.Object ?? GetUpdateResultMock().Object);
            repositoryMock.Setup(repository => repository.Get(It.IsAny<string>()))
                .ReturnsAsync((string name) => Templates.FirstOrDefault(template => template.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
            repositoryMock.Setup(repository => repository.GetChildTemplates(It.IsAny<string>()))
                .ReturnsAsync((string name) => Templates.Where(template => 
                        !string.IsNullOrEmpty(template.Parent) && 
                        template.Parent.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .ToList());
            repositoryMock.Setup(repository => repository.GetAll())
                .Returns(Templates.AsQueryable());

            return repositoryMock;
        }

        /// <summary>
        /// Возвращает результат удаления.
        /// </summary>
        /// <returns>Мок результата удаления шаблона.</returns>
        private Mock<DeleteResult> GetDeleteResultMock()
        {
            var deleteResultMock = new Mock<DeleteResult>();
            deleteResultMock.SetupAllProperties();
            deleteResultMock.Setup(updateResult => updateResult.DeletedCount)
                .Returns(1);
            deleteResultMock.Setup(updateResult => updateResult.IsAcknowledged)
                .Returns(true);
            
            return deleteResultMock;
        }

        /// <summary>
        /// Возвращает результат обновления.
        /// </summary>
        /// <returns>Мок результата обновления шаблона.</returns>
        private Mock<UpdateResult> GetUpdateResultMock()
        {
            var updateResultMock = new Mock<UpdateResult>();
            updateResultMock.SetupAllProperties();
            updateResultMock.Setup(updateResult => updateResult.IsAcknowledged)
                .Returns(true);
            
            return updateResultMock;
        }
    }
}
