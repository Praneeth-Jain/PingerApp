using PingerApp.Model;
using RabbitMQ.Client;
using System;

public class RabbitMQConnectionManager : IRabbitMQConnectionManager
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly RabbitMQSettings _rabbitMQSettings;
    private IConnection _connection;
    private bool _disposed;

    public RabbitMQConnectionManager(RabbitMQSettings rabbitMQSettings)
    {
        _rabbitMQSettings = rabbitMQSettings;
        _connectionFactory = new ConnectionFactory()
        {
            HostName = _rabbitMQSettings.Host,
            UserName = _rabbitMQSettings.Username,
            Password = _rabbitMQSettings.Password,
            Port = _rabbitMQSettings.Port,
            VirtualHost = _rabbitMQSettings.VirtualHost
        };
    }

    public IConnection GetConnection()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            _connection = _connectionFactory.CreateConnection();
        }
        return _connection;
    }

    public IModel CreateChannel()
    {
        return GetConnection().CreateModel();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _connection?.Close();
        _connection?.Dispose();
        _disposed = true;
    }
}
