using Dapper;
using ExampleApi;
using ExampleApi.Data;
using ExampleApi.Logic;
using MikeyT.EnvironmentSettingsNS.Interface;
using MikeyT.EnvironmentSettingsNS.Logic;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

var envSettings = new EnvironmentSettings(new DefaultEnvironmentVariableProvider(), new DefaultSecretVariableProvider());
envSettings.AddSettings<GlobalSettings>();
Console.WriteLine($"Loaded environment settings\n{envSettings.GetAllAsSafeLogString()}");

DefaultTypeMap.MatchNamesWithUnderscores = true;
SqlMapper.AddTypeHandler(new DateTimeHandler());

builder.Services.AddSingleton<IEnvironmentSettings>(envSettings);
builder.Services.AddSingleton<IConnectionStringProvider>(new ConnectionStringProvider(envSettings));
builder.Services.AddSingleton<INameLogic>(new NameLogic());
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
