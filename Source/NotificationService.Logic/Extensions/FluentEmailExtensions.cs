using FluentEmail.Core.Interfaces;
using FluentEmail.Smtp;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Logic.Factories;
using NotificationService.Logic.Models;
using NotificationService.Logic.NotificationChannels;
using NotificationService.Logic.NotificationChannels.Interfaces;

namespace NotificationService.Logic.Extensions
{
    /// <summary>
    /// Расширения коллекции сервисов для добавления FluentEmail.
    /// </summary>
    public static class FluentEmailExtensions
    {
        /// <summary>
        /// Добавить FluentEmail.
        /// </summary>
        /// <param name="serviceCollection">Коллекция сервисов.</param>
        /// <param name="settings">Настройки SMTP клиента.</param>
        public static void AddFluentEmail(this IServiceCollection serviceCollection, SmtpClientSettings settings)
        {
            serviceCollection
                .AddScoped<ISender>(serviceProvider => new SmtpSender(SmtpClientFactory.Create(settings)))
                .AddScoped<IEmailNotifier, EmailNotifier>()
                .AddFluentEmail(settings.DefaultEmail)
                .AddRazorRenderer();
        }
    }
}