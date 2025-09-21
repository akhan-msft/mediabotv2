namespace MediaBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting Teams Media Bot - Phase 1 (Event Logging)");
            Console.WriteLine("=================================================");
            
            var host = CreateHostBuilder(args).Build();
            
            Console.WriteLine($"Bot will be available at: https://localhost:5001");
            Console.WriteLine($"Health check endpoint: https://localhost:5001/api/health");
            Console.WriteLine($"Callback endpoint: https://localhost:5001/api/callback");
            Console.WriteLine($"Manual join endpoint: POST https://localhost:5001/api/callback/join");
            Console.WriteLine("=================================================");
            
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                    });
                });
    }
}