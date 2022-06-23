using EasyNetQ;
using EasyNetQ.Logging;
using EasyNetQ.Topology;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace SPC.RabbitMQ
{
    public class MQService
    {
        #region Setup

        private string _product = "Phoebus";

        private string _hostName = "armadillo.rmq.cloudamqp.com";
        private string _virtualHost = "kjtowiia";
        private string _userName = "kjtowiia";
        private string _password = "P43mO_LfWEeQUeEuFDIgjPBTh0vOWUQf";
        private int _port = 5672;

        private IConnection _connection;
        internal IConnection GetConnection()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                var factory = new ConnectionFactory() { HostName = _hostName, VirtualHost = _virtualHost, UserName = _userName, Password = _password, Port = _port, Ssl = new SslOption() { Enabled = false } };

                _connection = factory.CreateConnection();

            }
            return _connection;
        }


        private static MQService _instance;
        public static MQService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MQService();
                }
                return _instance;
            }
        }

        #endregion


        #region Service Bus

        private IBus _bus = null;
        public IBus GetServiceBus()
        {
            if (_bus == null)
            {
                string con = $"host={_hostName}:{_port};virtualHost={_virtualHost};username={_userName};password={_password};product={_product}";
                _bus = RabbitHutch.CreateBus(con);
                                                
                //LogProvider.SetCurrentLogProvider(ConsoleLogProvider.Instance);
            }            
            return _bus;
        }

        
        public static async Task PublishAsync(SPC.Services.COM.IPubSubMessage message)
        {
            var theBus = Instance.GetServiceBus();

            await theBus.PubSub.PublishAsync<SPC.Services.COM.IPubSubMessage>(message);
        }

        public static void DeleteQueue(string queueName)
        {
            var advance = Instance.GetServiceBus().Advanced;

                try
                {                
                    advance.QueueDeclarePassive(queueName);

                    var theQueue = advance.QueueDeclare(queueName);

                    advance.QueueDelete(theQueue);

                }
                catch (System.Exception)
                {

                    
                }
                
         
            
        }
                       

        #endregion
    }
}
