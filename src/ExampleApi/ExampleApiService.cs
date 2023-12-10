using Dapper;
using ExampleApi.Data;
using ExampleApi.Logic;
using MikeyT.EnvironmentSettingsNS.Interface;
using MikeyT.EnvironmentSettingsNS.Logic;

namespace ExampleApi;

public static class ExampleApiService
{
    public static void Run(string[] args, DbType dbType)
    {
        DotEnv.Load();

        var servicePort = Environment.GetEnvironmentVariable("SERVICE_PORT")?.Trim();
        if (servicePort == null || servicePort == string.Empty)
        {
            throw new Exception("Env var missing: SERVICE_PORT");
        }

        var builder = WebApplication.CreateBuilder(args);

        var envSettings = new EnvironmentSettings(new DefaultEnvironmentVariableProvider(), new DefaultSecretVariableProvider());
        envSettings.AddSettings<GlobalSettings>();
        Console.WriteLine($"Loaded environment settings\n{envSettings.GetAllAsSafeLogString()}");

        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new DateTimeHandler());

        builder.Services.AddSingleton<IEnvironmentSettings>(envSettings);
        builder.Services.AddSingleton<IConnectionStringProvider>(new ConnectionStringProvider(envSettings, dbType));
        builder.Services.AddSingleton<INameLogic>(new NameLogic());
        
        switch (dbType)
        {
            case DbType.Postgres:
                builder.Services.AddScoped<IPersonRepository, PersonRepositoryPostgres>();
                break;
            case DbType.SqlServer:
                builder.Services.AddScoped<IPersonRepository, PersonRepositorySqlServer>();
                break;
            default:
                throw new Exception("DbType not implemented: " + Enum.GetName(dbType));
        }

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // app.UseHttpsRedirection();

        app.MapGet("/", context =>
        {
            context.Response.Redirect("/swagger/index.html");
            return Task.CompletedTask;
        });

        app.UseAuthorization();

        app.MapControllers();

        app.Run($"http://localhost:{servicePort}");

    }
}
