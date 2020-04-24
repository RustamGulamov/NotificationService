namespace NotificationService.Data.Models
{
    /// <summary>
    /// Настройки подключения к Mongo.
    /// </summary>
    public class MongoSettings
    {
        /// <summary>
        /// Строка подключения, должна содержать название БД.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Название коллекции.
        /// </summary>
        public string CollectionName { get; set; }
    }
}
