namespace ClientConsole
{
    using System;
    using System.Threading.Tasks;
    using Messages;
    using NServiceBus;
    using NServiceBus.Features;

    class Program
    {
        static async Task Main()
        {

            var endpointConfiguration = new EndpointConfiguration("ClientUI");

            var transport = endpointConfiguration.UseTransport<MsmqTransport>();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");

            endpointConfiguration.EnableInstallers();
            endpointConfiguration.DisableFeature<TimeoutManager>();
            endpointConfiguration.Recoverability().Delayed(d => { d.NumberOfRetries(0); });
            transport.DisablePublishing();

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            var _endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            while (true)
            {
                Console.WriteLine("Press any key to send an order. Press Enter to exit");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Enter) break;
                var orderId = Guid.NewGuid().ToString().Substring(0, 8);

                var command = new PlaceOrder { OrderId = orderId };

                // Send the command
                await _endpointInstance.Send(command)
                    .ConfigureAwait(false);

            }

            await _endpointInstance.Stop().ConfigureAwait(false);

        }
    }
}
