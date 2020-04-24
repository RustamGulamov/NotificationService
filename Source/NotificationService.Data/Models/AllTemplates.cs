using System;
using System.Collections.Generic;

namespace NotificationService.Data.Models
{
    /// <summary>
    /// Используется для пагинации при возвращении всех элементов.
    /// </summary>
    public class AllTemplates : PageInfo
    {
        private int pagesCount;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public AllTemplates()
        {
            pagesCount = 1;
            CurrentPage = 1;
            PageSize = 10;
            TotalTemplates = 0;
            Templates = new MessageTemplate[] {};
        }
        
        /// <summary>
        /// Количество страниц.
        /// </summary>
        public int PagesCount
        {
            get
            {
                int count = (int)Math.Ceiling((double)TotalTemplates / PageSize);
                return count <= 0 ? pagesCount : count;
            }
            set => pagesCount = value;
        }

        /// <summary>
        /// Всего шаблонов.
        /// </summary>
        public int TotalTemplates { get; set; }

        /// <summary>
        /// Список шаблонов на текущей странице.
        /// </summary>
        public IEnumerable<MessageTemplate> Templates { get; set; }
    }
}
