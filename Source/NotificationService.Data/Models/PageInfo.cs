namespace NotificationService.Data.Models
{
    /// <summary>
    /// Информация о номере текущей страницы и её размере..
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// Номер текущей страницы.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Размер страницы.
        /// </summary>
        public int PageSize { get; set; }
    }
}
