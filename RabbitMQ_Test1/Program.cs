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

            var subscription = client.SubscribeAsync<TestClass>(async (msg, context) =>
            {
                Console.WriteLine($"Received message: {msg.LastName}, {msg.FirstName} born on {msg.DateOfBirth:d}");
            }, builder =>
            {
                builder.WithExchange(exchangeBuilder =>
                {
                    exchangeBuilder
                        .AssumeInitialized()
                        .WithName("amq.fanout");
                });
            });

            var person1 = new TestClass
            {
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = new DateTime(1984, 03, 10)
            };

            await client.PublishAsync(person1, Guid.NewGuid(), builder =>
            {
                builder.WithExchange(exchangeBuilder =>
                {
                    exchangeBuilder
                        .AssumeInitialized()
                        .WithName("amq.fanout");
                });
            });

            await Task.Delay(2000);

            var person2 = new TestClass
            {
                FirstName = "Melanie",
                LastName = "Smith",
                DateOfBirth = new DateTime(1985, 10, 25)
            };

            await client.PublishAsync(person2, Guid.NewGuid(), builder =>
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