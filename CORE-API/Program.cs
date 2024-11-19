
using CORE_API.Logging;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CORE_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //TODO add listIncidents method for all incidents
            //TODO 


            IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("secrets.json").Build();

            //IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("config.json").Build();

            var builder = WebApplication.CreateBuilder(args);

            //Add Context
            builder.Services.AddDbContext<UserContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));

            builder.Services.AddDbContext<IncidentContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));

            //builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));
            // Register Redis as a singleton
            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection")));
            builder.Services.AddSingleton<IDatabase>(sp =>
            {
                var connection = sp.GetRequiredService<IConnectionMultiplexer>();
                return connection.GetDatabase();
            });

            // Add services to the container.
            builder.Services.AddControllers();


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();

            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log1.log");
            builder.Logging.AddFileLogger(logFilePath);

        }
    }
}