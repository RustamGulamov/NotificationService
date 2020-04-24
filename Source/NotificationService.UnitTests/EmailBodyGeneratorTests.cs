using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using NotificationService.Data;
using NotificationService.Data.Models;
using NotificationService.Data.Repositories;
using NotificationService.Logic.MessageTemplateEngines;
using NotificationService.Logic.Services;
using NotificationService.Logic.Services.Interfaces;
using Xunit;

namespace NotificationService.UnitTests
{
    /// <summary>
    /// Тест для класса <see cref="EmailBodyGenerator"/>.
    /// </summary>
    public class EmailBodyGeneratorTests
    {
        private readonly IServiceCollection serviceCollection = new ServiceCollection();
        private readonly IEmailBodyGenerator generator;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public EmailBodyGeneratorTests()
        {
            var loggerEmailBodyGeneratorMock = new Mock<ILogger<EmailBodyGenerator>>();

            var repositoryMock = new Mock<IMessageTemplatesRepository>();

            repositoryMock.Setup(repository => repository.Get(It.IsAny<string>()))
                .Returns((string name) => Task.FromResult(Templates.FirstOrDefault(template => template.Name.Equals(name, StringComparison.OrdinalIgnoreCase))));

            serviceCollection.AddScoped(x => new JinjaTemplateEngine(new Mock<ILogger<JinjaTemplateEngine>>().Object));
            serviceCollection.AddScoped(x => new RazorTemplateEngine(new Mock<ILogger<RazorTemplateEngine>>().Object));

            generator = new EmailBodyGenerator(serviceCollection.BuildServiceProvider(), new TemplateManager(repositoryMock.Object, new Mock<ILogger<TemplateManager>>().Object), loggerEmailBodyGeneratorMock.Object);
        }

        private IEnumerable<MessageTemplate> Templates { get; } = new List<MessageTemplate>
        {
            new MessageTemplate { EngineType = Engines.Jinja, Name = "baseJinjaTemplate", Parent = null, Body = "{% block title %}{% endblock %}{% block content %}Content{% endblock %} {% block footer %} Copyright 2019 by  {% endblock %}", },
            new MessageTemplate { EngineType = Engines.Razor, Name = "baseRazorTemplate", Parent = null, Body = "<h1>@RenderBody()</h1><h2>It is base template.</h2>"},

            new MessageTemplate { EngineType = Engines.Jinja, Name = "contentTemplate", Parent = "baseJinjaTemplate", Body = "{% extends \"baseJinjaTemplate\" %}{% block title %}Content title{% endblock %} {% block content %} It is content. {% endblock %}", },

            new MessageTemplate { EngineType = Engines.Jinja, Name = "baseBlock", Parent = null, Body = "Hi, {{userName}}.{% block firstBlock %}{% endblock %}{% block secondBlock %}{% endblock %} Sincerely, {{companyName}}.", },
            new MessageTemplate { EngineType = Engines.Jinja, Name = "firstBlock", Parent = "baseBlock", Body = "{% extends 'baseBlock' %}{% block firstBlock %}It is first block.{% endblock %}" },
            new MessageTemplate { EngineType = Engines.Jinja, Name = "secondBlock", Parent = "baseBlock", Body = "{% extends 'baseBlock' %}{% block secondBlock %} It is second block with text {{text}}.{% endblock %}" },
            new MessageTemplate { EngineType = Engines.Jinja, Name = "thirdBlock", Parent = "baseBlock", Body = "{% extends 'baseBlock' %}{% block secondBlock %} {%for item in array%} {{item}} {%endfor%}{% endblock %}"},
            new MessageTemplate { EngineType = Engines.Jinja, Name = "fourthBlock", Parent = null, Body = "some markup here... {% block fourthBlock %}It is fourth Block.{% endblock %}"},
            new MessageTemplate { EngineType = Engines.Jinja, Name = "fifthBlock", Parent  = "fourthBlock", Body = "{% extends 'fourthBlock' %}{% block fourthBlock %}It is fourth Block block from nested.{% endblock %}"},

            new MessageTemplate { EngineType = Engines.Jinja, Name = "LayoutJinjaHtml", Parent = null, Body = "<div><h2>This is part of my base template</h2><br>{% block content %}{% endblock %}<br><h2>This is part of my base template</h2></div>"},
            new MessageTemplate { EngineType = Engines.Jinja, Name = "InheritedJinjaHtml", Parent = "LayoutJinjaHtml", Body = @"{% extends ""LayoutJinjaHtml"" %}{% block content %}<h3> This is the start of my child template</h3><br><p>My string: {{my_string}}</p><p>Value from the list: {{my_list[3]}}</p><p>Loop through the list:</p><ul>{% for n in my_list %}<li>{{n}}</li>{% endfor %}</ul><h3> This is the end of my child template</h3>{% endblock %}"},
            new MessageTemplate { EngineType = Engines.Jinja, Name = "JinjaListObjects", Parent = "LayoutJinjaHtml", Body = @"{% extends ""LayoutJinjaHtml"" %}{% block content %}<h3> This is the start of my child template</h3><br><p>My string: {{my_string}}</p><p>Value from the list: {{my_list[1].name}}</p><p>Loop through the list:</p><ul>{% for item in my_list %}<li>{{item.name}}</li>{% endfor %}</ul><h3> This is the end of my child template</h3>{% endblock %}"},
            new MessageTemplate { EngineType = Engines.Jinja, Name = "sixthBlock", Parent  = "baseBlock", Body = "{% extends 'baseBlock' %}{% block firstBlock %} Recipient UserName: {{user.Name}}, Recipient Email:{{user.email}} Recipient Address:{{user.address.zipcode}}.{% endblock %}"},

            new MessageTemplate { EngineType = Engines.Razor, Name = "LayoutRazorHtml", Parent = null, Body = "<h1>@RenderBody()</h1><h2>С уважением, ИнфоТеКС.</h2>"},
            new MessageTemplate { EngineType = Engines.Razor, Name = "BodyRazorHtml", Parent = "LayoutRazorHtml", Body = @"@{Layout = ""LayoutRazorHtml"";}Привет @Model.UserName."},
            new MessageTemplate { EngineType = Engines.Razor, Name = "EmailRazorHtml", Parent = "LayoutRazorHtml", Body = @"@{Layout = ""LayoutRazorHtml"";} Привет @Model.UserName. Название проекта: @Model.ProjectName. Версия: @Model.Version. Статус сборки: @Model.BuildState."},

        };


        /// <summary>
        /// Тест выполняет проверку GenerateEmailBody базовым шаблоном.
        /// </summary>
        /// <param name="parentTemplateName">Имя базового шаблона.</param>
        /// <param name="exceptedMessage">Ожидаемое сообщение.</param>
        /// <returns>Тест.</returns>
        [Theory]
        [InlineData("baseJinjaTemplate", "Content  Copyright 2019 by")]
        [InlineData("baseRazorTemplate", "<h1></h1><h2>It is base template.</h2>")]
        public async Task GenerateEmailBody_TemplatesIsParentTemplate_EmailBodyEqualToExpected(string parentTemplateName, string exceptedMessage)
        {
            var template = GetTemplate(parentTemplateName);

            var emailBody = await generator.GenerateEmailBody(template, new JObject());

            Assert.Equal(exceptedMessage, emailBody);
        }

        /// <summary>
        /// Тест выполняет проверку GenerateEmailBody с корректным именем шаблона и параметра. Параметр шаблона объект.
        /// </summary>
        /// <param name="templateName">Имя шаблона.</param>
        /// <param name="exceptedMessage">Ожидаемое сообщение с ошибкой.</param>
        /// <returns>Тест.</returns>
        [Theory]
        [InlineData("baseBlock", "Hi, Ivan Ivanovich. Sincerely,   Group.")]
        [InlineData("firstBlock", "Hi, Ivan Ivanovich.It is first block. Sincerely,   Group.")]
        [InlineData("secondBlock", "Hi, Ivan Ivanovich. It is second block with text Notification Service working. Sincerely,   Group.")]
        [InlineData("thirdBlock", "Hi, Ivan Ivanovich.  1  2  3  Sincerely,   Group.")]
        public async Task GenerateEmailBody_Jinja_EmailParamsAreObjects_EmailBodyEqualToExpected(string templateName, string exceptedMessage)
        {
            var emailBodyParameters = new
            {
                UserName = "Ivan Ivanovich",
                CompanyName = "  Group",
                Text = "Notification Service working",
                Array = new[] { 1, 2, 3 }
            };

            var template = GetTemplate(templateName);

            var emailBody = await generator.GenerateEmailBody(template, emailBodyParameters);

            Assert.Equal(exceptedMessage, emailBody);
        }

        /// <summary>
        /// Тест выполняет проверку GenerateEmailBody с корректным именем шаблона и параметра. Параметр шаблона в json формате.
        /// </summary>
        /// <param name="templateName">Имя шаблона.</param>
        /// <param name="jsonEmailParams">Параметр шаблона в json формате.</param>
        /// <param name="exceptedMessage">Ожидаемое сообщение с ошибкой.</param>
        /// <returns>Тест.</returns>
        [Theory]
        [InlineData("baseBlock", "{userName: \"Alex\", companyName:\"  Group\"}", "Hi, Alex. Sincerely,   Group.")]
        [InlineData("firstBlock", "{userName: \"Alex\", companyName:\" \"}", "Hi, Alex.It is first block. Sincerely,  .")]
        [InlineData("secondBlock", "{userName: \"Alex\", text:\"SomeText\", companyName:\" \"}", "Hi, Alex. It is second block with text SomeText. Sincerely,  .")]
        [InlineData("thirdBlock", "{userName: \"Alex\", array:[1,\"test\", 2], companyName:\" \"}", "Hi, Alex.  1  test  2  Sincerely,  .")]
        [InlineData("sixthBlock", "{userName: \"Alex\", user:{name: \"Gulamov\", email:\"rustam.gulamov@ .ru\", address: {zipcode: 123456, address: \"Moscow\"}}, companyName:\" \"}", "Hi, Alex. Recipient UserName: Gulamov, Recipient Email:rustam.gulamov@ .ru Recipient Address:123456. Sincerely,  .")]
        public async Task GenerateEmailBody_Jinja_EmailParamsAreJson_EmailBodyEqualToExpected(string templateName, string jsonEmailParams, string exceptedMessage)
        {
            var template = GetTemplate(templateName);
            var jObject = JObject.Parse(jsonEmailParams);

            var emailBody = await generator.GenerateEmailBody(template, jObject);

            Assert.Equal(exceptedMessage, emailBody);
        }

        /// <summary>
        /// Тест выполняет проверку GenerateEmailBody с шаблоном html формате.
        /// </summary>
        /// <param name="templateName">Название шаблона.</param>
        /// <param name="jsonEmailParams">Параметры шаблона.</param>
        /// <param name="exceptedMessage">Ожидаемый результат сообщения.</param>
        /// <returns>Тест.</returns>
        [Theory]
        [InlineData("EmailRazorHtml", "{UserName: \"Alex\", ProjectName:\"ViPNet PKI Client\", Version:\"1.0.1\" }", "<h1> Привет Alex. Название проекта: ViPNet PKI Client. Версия: 1.0.1. Статус сборки: .</h1><h2>С уважением, ИнфоТеКС.</h2>")]
        [InlineData("EmailRazorHtml", "{UserName: \"Alex\", ProjectName:\"ViPNet PKI Client\", Version:\"1.0.1\", BuildState:\"succeed\" }", "<h1> Привет Alex. Название проекта: ViPNet PKI Client. Версия: 1.0.1. Статус сборки: succeed.</h1><h2>С уважением, ИнфоТеКС.</h2>")]
        public async Task GenerateEmailBody_Razor_TemplateIsHtmlAndEmailParamsAreJson_EmailBodyEqualToExpected(string templateName, string jsonEmailParams, string exceptedMessage)
        {
            var template = GetTemplate(templateName);
            var json = JObject.Parse(jsonEmailParams);
            var emailBody = await generator.GenerateEmailBody(template, json);

            Assert.Equal(exceptedMessage, emailBody);
        }

        /// <summary>
        /// Тест выполняет проверку GenerateEmailBody с шаблоном html формате.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task GenerateEmailBody_Razor_TemplateIsHtml_EmailBodyEqualToExpected()
        {
            var templateName = "BodyRazorHtml";
            var template = GetTemplate(templateName);
            var emailBody = await generator.GenerateEmailBody(template, new { UserName = "John" });

            Assert.Equal("<h1>Привет John.</h1><h2>С уважением, ИнфоТеКС.</h2>", emailBody);
        }

        /// <summary>
        /// Тест выполняет проверку метода GenerateEmailBody с шаблоном html формате.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task GenerateEmailBody_Jinja_TemplateIsHtml_EmailBodyEqualToExpected()
        {
            var template = GetTemplate("InheritedJinjaHtml");
            var emailBody = await generator.GenerateEmailBody(template, new { my_string = "My name is John.", my_list = new[] { 1, 2, 3, 4, 5 } });
            var expectedMessage =
                "<div><h2>This is part of my base template</h2><br><h3> This is the start of my child template</h3><br><p>My string: My name is John.</p><p>Value from the list: 4</p><p>Loop through the list:</p><ul><li>1</li><li>2</li><li>3</li><li>4</li><li>5</li></ul><h3> This is the end of my child template</h3><br><h2>This is part of my base template</h2></div>";

            Assert.Equal(expectedMessage, emailBody);
        }

        /// <summary>
        /// Тест выполняет проверку метода GenerateEmailBody с шаблоном html формате, если шаблон имеет список объектов.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task GenerateEmailBody_JinjaTemplateHasListObjects_EmailBodyEqualToExpected()
        {
            var template = GetTemplate("JinjaListObjects");
            var emailBody = await generator.GenerateEmailBody(template, 
                new { my_string = "My name is John.", my_list = new[]
                {
                    new { name = "first" },
                    new { name = "second"}
                }});

            var expectedMessage =
                "<div><h2>This is part of my base template</h2><br><h3> This is the start of my child template</h3><br><p>My string: My name is John.</p><p>Value from the list: second</p><p>Loop through the list:</p><ul><li>first</li><li>second</li></ul><h3> This is the end of my child template</h3><br><h2>This is part of my base template</h2></div>";

            Assert.Equal(expectedMessage, emailBody);
        }

        /// <summary>
        ///  Тест выполняет проверку GenerateEmailBody с шаблоном несовпадающим с типом родительского шаблона.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task GenerateEmailBody_Razor_ParentTemplateTypeNotExists_ThrowsInvalidOperationException()
        {
            var template = new MessageTemplate()
            {
                Name = "test",
                Parent = "NotExistsTemplate",
                EngineType = Engines.Razor,
                Body = "@{Layout = \"LayoutRazorHtml\";}Привет @Model.UserName."
            };

            var exception = await Assert.ThrowsAnyAsync<Exception>(() => generator.GenerateEmailBody(template, new { }));

            Assert.Equal($"Couldn't found template named {template.Parent.ToLower()}", exception.Message);
        }

        /// <summary>
        ///  Тест выполняет проверку GenerateEmailBody с шаблоном несовпадающим с типом родительского шаблона.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task GenerateEmailBody_ParentTemplateTypeDoesNotMatch_ThrowsInvalidOperationException()
        {
            var template = new MessageTemplate()
            {
                Name = "JinjaTemplate",
                Parent = "LayoutRazorHtml",
                EngineType = Engines.Jinja,
                Body = "{% block JinjaTemplate %}{% endblock %}"
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => generator.GenerateEmailBody(template, new { }));

            Assert.Equal($"Template type {template.Name} does not match type of the parent template {template.Parent}.", exception.Message);
        }

        /// <summary>
        /// Тест выполняет проверку GenerateEmailBody с некорректным именем шаблона.
        /// </summary>
        /// <returns>Тест.</returns>
        [Fact]
        public async Task GenerateEmailBody_NonexistentTemplate_ThrowsArgumentException()
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => generator.GenerateEmailBody(null, new { }));
            Assert.NotNull(exception);
        }

        private MessageTemplate GetTemplate(string templateName)
        {
            return Templates.FirstOrDefault(x => x.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase));
        }
    }
}