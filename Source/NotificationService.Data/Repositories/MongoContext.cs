using System;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationService.Data.Models;

namespace NotificationService.Data.Repositories
{
    /// <summary>
    /// Контекст подключения к Mongo.
    /// </summary>
    public class MongoContext
    {
        private readonly IMongoDatabase database;

        /// <summary>
        /// Конструктор, создает подключение к БД.
        /// </summary>
        /// <param name="settings">Настройки подключения к Mongo.</param>
        public MongoContext(IOptions<MongoSettings> settings)
        {
            MongoSettings mongoSettings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            MongoUrl url = MongoUrl.Create(mongoSettings.ConnectionString);
            IMongoClient client = new MongoClient(url);
            database = client.GetDatabase(url.DatabaseName);
        }

        /// <summary>
        /// Возвращает коллекцию по названию.
        /// </summary>
        /// <param name="collectionName">Название коллекции.</param>
        /// <returns>Коллекция типа T.</returns>
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return database.GetCollection<T>(collectionName);
        }
    }
}
