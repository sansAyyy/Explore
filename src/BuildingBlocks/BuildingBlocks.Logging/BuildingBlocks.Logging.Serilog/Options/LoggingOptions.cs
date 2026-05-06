namespace BuildingBlocks.Logging.Serilog.Options
{
    public class LoggingOptions
    {
        public string ServiceName { get; set; } = "UnknownService";
        public string Environment { get; set; } = "Development";
        public bool WriteToConsole { get; set; } = true;
        public bool WriteToFile { get; set; } = true;
        public string FilePath { get; set; } = "logs/service-.log";
    }
}

