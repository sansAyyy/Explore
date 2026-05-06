using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Messaging.RabbitMQ.Options
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public string Port { get; set; } = "5672";
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        /// <summary>
        /// 并发控制
        /// </summary>
        public int? ConcurrentMessageLimit { get; set; } = 10;
    }
}

