using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationService.Data.Extensions;
using NotificationService.Data.Models;

namespace NotificationService.Data.Repositories
{
    /// <summary>
    /// Репозиторий для работы с шаблонами в Mongo.
    /// </summary>
    public class MessageTemplatesRepository : IMessageTemplatesRepository
    {
        private readonly IMongoCollection<MessageTemplate> collection;
        private readonly ILogger<MessageTemplate> logger;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="settings">Настройки подключения к Mongo.</param>
        /// <param name="context">Контекст подключения к Mongo.</param>
        /// <param name="logger">Для логирования событий.</param>
        public MessageTemplatesRepository(IOptions<MongoSettings> settings, MongoContext context, ILogger<MessageTemplate> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            MongoSettings mongoSettings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            collection = context.GetCollection<MessageTemplate>(mongoSettings.CollectionName);
        }

        /// <inheritdoc />
        public async Task Add(MessageTemplate template)
        {
            template.ThrowIfNull(nameof(template));
            logger.LogInformation($"Adding template named {template.Name} into collection");
            await collection.InsertOneAsync(template);
        }

        /// <inheritdoc />
        public async Task<DeleteResult> Delete(string templateName)
        {
            templateName.ThrowIfNullOrEmty(nameof(templateName));
            logger.LogInformation($"Deleting template named {templateName} from collection");
            return await collection.DeleteOneAsync(FilterByName(templateName));
        }

        /// <inheritdoc />
        public async Task<DeleteResult> DeleteAll()
        {
            logger.LogInformation("Deleting all templates from collection");
            return await collection.DeleteManyAsync(Builders<MessageTemplate>.Filter.Empty);
        }

        /// <inheritdoc />
        public async Task<UpdateResult> Update(MessageTemplate template)
        {
            template.ThrowIfNull(nameof(template));

            UpdateDefinition<MessageTemplate> update = Builders<MessageTemplate>.Update
                .Set(t => t.Title, template.Title)
                .Set(t => t.Parent, template.Parent)
                .Set(t => t.Body, template.Body)
                .Set(t => t.EngineType, template.EngineType)
                .Set(t => t.UpdatedBy, template.UpdatedBy)
                .Set(t => t.UpdatedDate, template.UpdatedDate);
            
            logger.LogInformation($"Updating template named {template.Name} in collection");
            return await collection.UpdateOneAsync(FilterByName(template.Name), update);
        }

        /// <inheritdoc />
        public async Task<MessageTemplate> Get(string templateName)
        {
            templateName.ThrowIfNullOrEmty(nameof(templateName));
            logger.LogInformation($"Returning template named {templateName} from collection");
            return await collection
                .Find(FilterByName(templateName))
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<List<MessageTemplate>> GetChildTemplates(string templateName)
        {
            templateName.ThrowIfNullOrEmty(nameof(templateName));
            FilterDefinition<MessageTemplate> filter = Builders<MessageTemplate>.Filter
                .Eq(nameof(MessageTemplate.Parent), templateName);

            logger.LogInformation($"Returning child templates from collection linked to {templateName}");
            return await collection
                .Find(filter)
                .ToListAsync();
        }

        /// <inheritdoc />
        public IQueryable<MessageTemplate> GetAll()
        {
            logger.LogInformation("Returning all templates from collection");
            return collection.AsQueryable();
        }

        /// <summary>
        /// Возвращает фильтр шаблонов по названию для поиска по коллекции.
        /// </summary>
        /// <param name="templateName">Название шаблона.</param>
        /// <returns>Фильтр.</returns>
        private FilterDefinition<MessageTemplate> FilterByName(string templateName)
        {
            return Builders<MessageTemplate>.Filter
                .Eq(nameof(MessageTemplate.Name), templateName);
        }
    }
}
