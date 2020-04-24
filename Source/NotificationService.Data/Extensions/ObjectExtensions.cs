using System;

namespace NotificationService.Data.Extensions
{
    /// <summary>
    /// Методы расширения класса <see cref="object"/>.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Проверка на то что объект имеет значение null.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="obj">Объект.</param>
        /// <param name="parameterName">Имя параметра.</param>
        public static void ThrowIfNull<T>(this T obj, string parameterName) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Проверка строковой переменной на пустоту или null.
        /// </summary>
        /// <param name="obj">Проверяемая строка.</param>
        /// <param name="parameterName">Имя параметра, на которое будет выброшено исключение <see cref="ArgumentNullException"/>.</param>
        public static void ThrowIfNullOrEmty(this string obj, string parameterName)
        {
            if (string.IsNullOrEmpty(obj))
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}