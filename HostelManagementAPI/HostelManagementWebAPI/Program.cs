using API.Extensions;
using DTOs;
using DTOs.Complain;
using Hangfire;
using HostelManagementWebAPI.Extensions;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using Service;
using Service.Interface;
using Service.Vnpay;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.IdentityServices(builder.Configuration);
builder.Services.ApplicationServices(builder.Configuration);

//builder.Services.AddDbContext<DataContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlCloud")));

ConfigurationHelper.Initialize(builder.Configuration);
builder.Services.Configure<VnPayProperties>(builder.Configuration.GetSection("VnPay"));

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<ComplainDto>("Complains");
modelBuilder.EntitySet<NotificationDto>("Notifications");

builder.Services.AddControllers().AddOData(
    options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null).AddRouteComponents(
        "odata",
        modelBuilder.GetEdmModel()));

// Hangfire client
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("SqlCloud")));

// Hangfire server
builder.Services.AddHangfireServer();
builder.Services.AddScoped<IBackgroundService, Service.Implement.BackgroundService>();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddGrpc();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();
//using var scope = app.Services.CreateScope();
//var services = scope.ServiceProvider;
//try
//{
//    var context = services.GetRequiredService<DataContext>();
//    await context.Database.MigrateAsync();
//    await SeedData.SeedAccount(context);
//    await SeedData.SeedHostel(context);
//}
//catch (Exception ex)
//{
//    var logger = services.GetService<ILogger<Program>>();
//    logger.LogError(ex, "An error occured during migration");
//}

//var cache = app.Services.GetRequiredService<IMemoryCache>();

app.MapControllers();

app.UseHangfireDashboard();
app.MapHangfireDashboard("/hangfire");


using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var backgroundService = serviceProvider.GetRequiredService<IBackgroundService>();

    RecurringJob.AddOrUpdate("ScheduleMembershipWhenExpire", () => backgroundService.ScheduleMembershipWhenExpire(), Cron.MinuteInterval(5));
    RecurringJob.AddOrUpdate("ScheduleContractWhenExpire", () => backgroundService.ScheduleContractWhenExpire(), Cron.MinuteInterval(5));
}

app.MapGrpcService<HostelManagementWebAPI.Services.NotificationService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
