using RabbitMQ.Client;

public interface IRabbitMQConnectionManager : IDisposable
{
    IConnection GetConnection();
    IModel CreateChannel();
}
