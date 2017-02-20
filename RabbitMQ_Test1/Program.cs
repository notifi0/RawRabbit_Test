using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Framing.Impl;
using RawRabbit.Configuration;
using RawRabbit.vNext;

namespace RabbitMQ_Test1
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();

            Console.WriteLine("Press any key to exit");
            Console.Read();
        }

        public static async Task MainAsync(string[] args)
        {
            var config = new RawRabbitConfiguration
            {
                Username = "guest",
                Password = "guest",
                Port = 5672,
                Hostnames = new List<string> { "localhost" },
                VirtualHost = "/",
                Queue = new GeneralQueueConfiguration
                {
                    AutoDelete = true,
                    Durable = false,
                    Exclusive = true
                }
            };
            var client = BusClientFactory.CreateDefault(config);

            var subscription = client.SubscribeAsync<string>(async (msg, context) =>
            {
                Console.WriteLine($"Received message: {msg}");
            }, builder =>
            {
                builder.WithExchange(exchangeBuilder =>
                {
                    exchangeBuilder
                        .AssumeInitialized()
                        .WithName("amq.fanout");
                });
            });

            await client.PublishAsync("Test message", Guid.NewGuid(), builder =>
            {
                builder.WithExchange(exchangeBuilder =>
                {
                    exchangeBuilder
                        .AssumeInitialized()
                        .WithName("amq.fanout");
                });
            });

            await Task.Delay(2000);

            await client.PublishAsync("Another test message!", Guid.NewGuid(), builder =>
            {
                builder.WithExchange(exchangeBuilder =>
                {
                    exchangeBuilder
                        .AssumeInitialized()
                        .WithName("amq.fanout");
                });
            });

            await Task.Delay(2000);
            await client.ShutdownAsync();
        }
    }
}