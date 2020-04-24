using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.Data.Models
{
    /// <summary>
    /// Шаблон сообщения.
    /// </summary>
    /// <remarks>
    /// Предназначен для преобразования писем от других служб с использованием шаблонизатора.
    /// </remarks>
    public class MessageTemplate
    {
        /// <summary>
        /// Содержимое шаблона.
        /// </summary>
        public string Body { get; set; }
        
        /// <summary>
        /// Название шаблона.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; }

        /// <summary>
        /// Название родительского шаблона.
        /// </summary>
        public string Parent { get; set; }

        /// <summary>
        /// Заголовок оповещения.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Тип используемого шаблонизатора.
        /// </summary>
        public Engines EngineType { get; set; }

        /// <summary>
        /// Кем создано - идентификатор.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Дата создания.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата последнего изменения.
        /// </summary>
        public DateTime UpdatedDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Кем изменено - идентификатор.
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
