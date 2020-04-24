using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NotificationService.Data;
using NotificationService.Data.Models;
using NotificationService.Data.Repositories;
using NotificationService.Logic;
using NotificationService.Logic.Extensions;
using NotificationService.Logic.Factories;
using NotificationService.Logic.Factories.Interfaces;
using NotificationService.Logic.MessageTemplateEngines;
using NotificationService.Logic.Models;
using NotificationService.Logic.NotificationMessages;
using NotificationService.Logic.Services;
using NotificationService.Logic.Services.Interfaces;
using NotificationService.Web.Authorization;
using NotificationService.Web.Extensions;
using NotificationService.Web.Filters;
using NotificationService.Web.Middleware;
using NotificationService.Web.Validators;
using RabbitMQClient.Messaging.Extensions;
using RabbitMQClient.Messaging.Rabbit;

namespace NotificationService.Web
{
    /// <summary>
    /// Класс конфигурации веб-приложения.
    /// </summary>
    public class Startup
    {
        private const string CorsPolicyName = "CorsPolicy";

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="configuration">Конфигурация.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Конфигурация приложения.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Конфигурирует pipeline запросов.
        /// </summary>
        /// <param name="app">Строитель приложения.</param>
        /// <param name="env">Параметры окружения.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors(CorsPolicyName);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    "default",
                    "{controller}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API V1");
            });
            app.UseHttpsRedirection();
            app.UseMiddleware(typeof(GlobalExceptionHandler));
        }

        /// <summary>
        /// Конфигурирует все сервисы приложения.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            ValidatorOptions.LanguageManager.Enabled = false;

            services.AddCors(options => 
            {
                options.AddPolicy(CorsPolicyName,
                    builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetIsOriginAllowed(x => true));
            });

            services.Configure<MongoSettings>(Configuration.GetSection(nameof(MongoSettings)).Bind);
            services.Configure<EmailSettings>(Configuration.GetSection(nameof(EmailSettings)).Bind);

            services.AddSingleton<MongoContext>();
            services.AddSingleton<IMessageTemplatesRepository, MessageTemplatesRepository>();
            services.AddSingleton<ITemplateManager, TemplateManager>();

            services.AddControllers();

            services.AddAuthentication();
            services.AddAuthorization();

            services.AddMvcCore(options => options.Filters.Add(typeof(FormatResponseFilter)))
                .AddNewtonsoftJson()
                .AddApiExplorer()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());
            
            services.AddScoped<JinjaTemplateEngine>();
            services.AddScoped<RazorTemplateEngine>();
            
            services.AddScoped<IEmailBodyGenerator, EmailBodyGenerator>();

            var rabbitConfiguration = new RabbitConfiguration();
            Configuration.GetSection(nameof(RabbitConfiguration)).Bind(rabbitConfiguration);

            services.AddMessageReceiver<RabbitMessageReceiver<NotificationMessage<EmailNotificationMessage>, EmailNotificationHandler>>(rabbitConfiguration);

            services.Configure<AuthorizationSettings>(Configuration.GetSection(nameof(AuthorizationSettings)).Bind);
            services.AddScoped<IAsyncAuthorizationFilter, JWTAuthorizationFilterAttribute>();
            services.AddScoped<IEmailMessageFactory, EmailMessageFactory>();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification Service", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer token\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new List<string>()
                    }
                });
                
                string xmlCommentsPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                options.IncludeXmlComments(xmlCommentsPath);
            });

            services.AddFluentEmail(Configuration.GetSection(nameof(SmtpClientSettings)).Get<SmtpClientSettings>());
            services.Configure<ApiBehaviorOptions>(o => o.InvalidModelStateResponseFactory = actionContext => 
                new BadRequestObjectResult(actionContext.ModelState).ToFormattedResponse());

            services.AddTransient<IValidator<JwtSecurityToken>, JwtSecurityTokenValidator>();
        }
    }
}
