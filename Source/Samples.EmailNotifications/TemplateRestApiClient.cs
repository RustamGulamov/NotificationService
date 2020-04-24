using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Samples.EmailNotifications
{
    /// <summary>
    /// Клиент для управления с шаблонами.
    /// </summary>
    public class TemplateRestApiClient
    {
        private static readonly HttpClient client = new HttpClient();

        private readonly List<TemplateModel> templateModels = new List<TemplateModel>();

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="baseAddress">Адрес сервера.</param>
        /// <param name="port">Порт.</param>
        /// <param name="bearerAuthenticationValue">Значение Bearer Authentication.</param>
        public TemplateRestApiClient(string baseAddress, int port, string bearerAuthenticationValue)
        {
            client.BaseAddress = new Uri($"http://{baseAddress}:{port}");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearerAuthenticationValue);
        }

        /// <summary>
        /// Добавить шаблоны.
        /// </summary>
        /// <param name="templateFiles">Шаблон файлы.</param>
        public void AddTemplates(List<TemplateFile> templateFiles)
        {
            foreach (var templateFile in templateFiles)
            {
                TemplateModel parentTemplate = null;
                if (templateFile.ParentTemplatePath != null)
                {
                    parentTemplate = GetParentTemplateModel(templateFile);
                }

                var templateModel = MapTemplateFileToTemplateModel(templateFile);

                if (parentTemplate != null)
                {
                    AddTemplate(parentTemplate).Wait();
                }

                AddTemplate(templateModel).GetAwaiter().GetResult();

                templateModels.Add(templateModel);
            }
        }

        /// <summary>
        /// Получить все шаблоны.
        /// </summary>
        /// <returns>Все шаблоны.</returns>
        public List<string> GetTemplateNames()
        {
            return templateModels.Select(x=>x.Name).ToList();
        }

        /// <summary>
        /// Удалить все шаблоны.
        /// </summary>
        public void RemoveAllTemplates()
        {
            DeleteAllTemplates().GetAwaiter().GetResult();
            templateModels.Clear();
        }

        private Task<HttpResponseMessage> AddTemplate(TemplateModel template)
        {
            var serialize = SerializeData(template);
            var content = new StringContent(serialize, Encoding.UTF8, "application/json");
            return client.PostAsync($"api/MessageTemplates", content);
        }

        private Task DeleteAllTemplates()
        {
            return client.SendAsync(
                new HttpRequestMessage(HttpMethod.Delete, $"api/MessageTemplates"));
        }

        private TemplateModel MapTemplateFileToTemplateModel(TemplateFile templateFile)
        {
            return new TemplateModel
            {
                Body = File.ReadAllText(templateFile.TemplateFilePath),
                Parent = templateFile.ParentTemplatePath == null ? null : Path.GetFileName(templateFile.ParentTemplatePath),
                Title = templateFile.Title,
                Engine = templateFile.Engine,
                Name = Path.GetFileName(templateFile.TemplateFilePath),
            };
        }
        
        private TemplateModel GetParentTemplateModel(TemplateFile templateFile)
        {
            var parentTemplateFile = new TemplateFile
            {
                Engine = templateFile.Engine,
                ParentTemplatePath = null,
                TemplateFilePath = templateFile.ParentTemplatePath,
                Title = "Base title",
            };

            return MapTemplateFileToTemplateModel(parentTemplateFile);
        }

        private string SerializeData(object data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});
        }
    }
}