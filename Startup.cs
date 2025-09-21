using MediaBot.Interfaces;
using MediaBot.Services;

namespace MediaBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add controllers
            services.AddControllers();
            
            // Add Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Register bot services
            services.AddSingleton<IEventLogger, EventLogger>();
            services.AddSingleton<ICallHandler, CallHandler>();
            services.AddSingleton<IBotService, BotService>();

            // Add hosted service to start bot
            services.AddHostedService<BotHostedService>();

            // Add CORS for development
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class BotHostedService : BackgroundService
    {
        private readonly IBotService _botService;
        private readonly ILogger<BotHostedService> _logger;

        public BotHostedService(IBotService botService, ILogger<BotHostedService> logger)
        {
            _botService = botService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _botService.StartAsync();
                
                // Keep the service running
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bot hosted service encountered an error");
                throw;
            }
            finally
            {
                await _botService.StopAsync();
            }
        }
    }
}