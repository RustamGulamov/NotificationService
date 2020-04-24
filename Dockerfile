FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine
WORKDIR /app
COPY . ./
ENV ASPNETCORE_ENVIRONMENT="Production"
ENTRYPOINT ["dotnet", "NotificationService.Web.dll"]
