using EventStore.Core.Bus;
using EventStore.Core.Messages;
using EventStore.Core.Services.TimerService;
using EventStore.Core.Services.Transport.Http;
using EventStore.Core.TransactionLog.Chunks;
using EventStore.Projections.Core.Messages;
using EventStore.Projections.Core.Services.Http;
using EventStore.Projections.Core.Services.Management;

namespace EventStore.Projections.Core
{
    public class ProjectionManagerNode
    {
        private readonly ProjectionManager _projectionManager;
        private readonly InMemoryBus _output;

        private ProjectionManagerNode(IPublisher inputQueue, IPublisher[] queues)
        {
            _output = new InMemoryBus("ProjectionManagerOutput");
            _projectionManager = new ProjectionManager(inputQueue, _output, queues, new RealTimeProvider());
        }

        public InMemoryBus Output
        {
            get { return _output; }
        }

        public void SetupMessaging(ISubscriber mainBus)
        {
            mainBus.Subscribe<SystemMessage.StateChangeMessage>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.Post>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.UpdateQuery>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.GetQuery>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.Delete>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.GetStatistics>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.GetState>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.GetResult>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.GetDebugState>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.Disable>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.Enable>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.Internal.CleanupExpired>(_projectionManager);
            mainBus.Subscribe<ProjectionManagementMessage.Internal.Deleted>(_projectionManager);
            mainBus.Subscribe<CoreProjectionManagementMessage.Started>(_projectionManager);
            mainBus.Subscribe<CoreProjectionManagementMessage.Stopped>(_projectionManager);
            mainBus.Subscribe<CoreProjectionManagementMessage.Faulted>(_projectionManager);
            mainBus.Subscribe<CoreProjectionManagementMessage.Prepared>(_projectionManager);
            mainBus.Subscribe<CoreProjectionManagementMessage.StateReport>(_projectionManager);
            mainBus.Subscribe<CoreProjectionManagementMessage.ResultReport>(_projectionManager);
            mainBus.Subscribe<CoreProjectionManagementMessage.DebugState>(_projectionManager);
            mainBus.Subscribe<CoreProjectionManagementMessage.StatisticsReport>(_projectionManager);
            mainBus.Subscribe<ClientMessage.WriteEventsCompleted>(_projectionManager);
            mainBus.Subscribe<ClientMessage.ReadStreamEventsBackwardCompleted>(_projectionManager);
        }

        public static ProjectionManagerNode Create(TFChunkDb db, 
                                                   QueuedHandler inputQueue, 
                                                   HttpService httpService, 
                                                   IPublisher networkSendQueue,
                                                   IPublisher[] queues)
        {
            var projectionManagerNode =
                new ProjectionManagerNode(inputQueue, queues);
            httpService.SetupController(new ProjectionsController(inputQueue, networkSendQueue));

            return projectionManagerNode;
        }
    }
}