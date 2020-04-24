using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Samples.EmailNotifications
{
    internal static class Program
    {
        private const string BaseAdress = "localhost";
        private const int Port = 2588;

        private static readonly NotificationsRabbitMqClient notificationClient;
        private static TemplateRestApiClient templateRestApiClient;

        static Program()
        {
            notificationClient = new NotificationsRabbitMqClient(BaseAdress);
        }

        private static readonly List<TemplateFile> templateFiles = new List<TemplateFile>
        {
            new TemplateFile { Engine = Engines.Jinja, Title = "Jinja base template - DEBUG", TemplateFilePath = GetPath(Path.Combine("Jinja", "BaseTemplate", "baseTemplate.html")), ParentTemplatePath = null},
            new TemplateFile { Engine = Engines.Jinja, Title = "Jinja template and base template - DEBUG", TemplateFilePath = GetPath(Path.Combine("Jinja", "BaseAndInheritedTemplate", "emailTemplate.html")), ParentTemplatePath = GetPath(Path.Combine("Jinja", "BaseAndInheritedTemplate", "baseEmailTemplate.html")) },
            new TemplateFile { Engine = Engines.Razor, Title = "Razor base template - DEBUG", TemplateFilePath = GetPath(Path.Combine("Razor", "BaseTemplate", "Layout.cshtml")), ParentTemplatePath = null },
            new TemplateFile { Engine = Engines.Razor, Title = "Razor template and base template - DEBUG", TemplateFilePath = GetPath(Path.Combine("Razor", "BaseAndInheritedTemplate", "EmailTemplate.cshtml")), ParentTemplatePath = GetPath(Path.Combine("Razor", "BaseAndInheritedTemplate", "LayoutEmailTemplate.cshtml")) },
        };

        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Not added in arguments Bearer authentication value.");
                return;
            }

            templateRestApiClient = new TemplateRestApiClient(BaseAdress, Port, args[0]);
            
            templateRestApiClient.AddTemplates(templateFiles);
            notificationClient.CreateAndSendMessage(templateRestApiClient.GetTemplateNames());

            Thread.Sleep(5000);

            templateRestApiClient.RemoveAllTemplates();
        }
        
        private static string GetPath(string path)
        {
            var rootDirName = "HtmlTemplates";

            return Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(rootDirName, path));
        }
    }
}
