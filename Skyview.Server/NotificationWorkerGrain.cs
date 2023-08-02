using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Orleans.Concurrency;
using Skyview.Lib;

namespace Skyview.Server;

[StatelessWorker]
public class NotificationWorkerGrain : Grain, INotificationWorkerGrain
{
	private const string TOPIC = "subscription-arm-events";

	private readonly string subscriptionName;
	private readonly ILogger<NotificationWorkerGrain> _log;
	private readonly IGrainFactory _grainFactory;
	private readonly ServiceBusClient _sb;
	private readonly ServiceBusAdministrationClient _sbm;
	private ServiceBusProcessor _sbProcessor;
	
   public NotificationWorkerGrain(IGrainFactory grainFactory, ILogger<NotificationWorkerGrain> log, ServiceBusClient sb, ServiceBusAdministrationClient sbm)
   {
	   _grainFactory = grainFactory;
	   _log = log;
	   _sb = sb;
	   _sbm = sbm;
	   
	   subscriptionName = $"notification-worker-{this.GetPrimaryKeyString()}";
   }

   public override async Task OnActivateAsync(CancellationToken cancellationToken)
   {
	   await base.OnActivateAsync(cancellationToken);
	   
	   _log.LogInformation("Creating subscription {SubscriptionName}", subscriptionName);
	   await _sbm.CreateSubscriptionAsync(TOPIC, subscriptionName, cancellationToken);
	   
	   _sbProcessor = _sb.CreateProcessor(TOPIC, subscriptionName, new ServiceBusProcessorOptions());
	   _sbProcessor.ProcessMessageAsync += MessageHandler; 
	   _sbProcessor.ProcessErrorAsync += ErrorHandler; 
	   
	   _log.LogInformation("Activated notification worker grain");
   }

   public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
   {
	   await _sbm.DeleteSubscriptionAsync(TOPIC, subscriptionName, cancellationToken);
	   await _sbProcessor.DisposeAsync();
	   await base.OnDeactivateAsync(reason, cancellationToken);
   }

   private Task ErrorHandler(ProcessErrorEventArgs arg)
   {
	   _log.LogInformation("Received error...");
	   return Task.CompletedTask;
   }

   private Task MessageHandler(ProcessMessageEventArgs arg)
   {
	   _log.LogInformation("Received message...");
	   return Task.CompletedTask;
   }

   public Task<bool> IsEnabled()
   {
	   return Task.FromResult(_sbProcessor.IsProcessing);
   }

   public Task Enable()
   {
	   _log.LogInformation("Enabling event processing...");
	   return _sbProcessor.StartProcessingAsync();
   }

   public Task Disable()
   {
	   _log.LogInformation("Disabling event processing...");
	   return _sbProcessor.StopProcessingAsync();
   }
}