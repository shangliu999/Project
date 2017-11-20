using ETexsys.Common.Log;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.Common.Rabbit
{
    public class RabbitMQHelper
    {
        private static ConnectionFactory _connectionFactory = null;

        private static string _queueName;

        static RabbitMQHelper()
        {
            _connectionFactory = new ConnectionFactory();
            _connectionFactory.HostName = ConfigurationManager.AppSettings["RabbitMQHostName"].ToString();
            _connectionFactory.UserName = ConfigurationManager.AppSettings["RabbitMQUserName"].ToString();
            _connectionFactory.Password = ConfigurationManager.AppSettings["RabbitMQPassword"].ToString();
            _queueName = ConfigurationManager.AppSettings["RabbitMQName"].ToString();
            _connectionFactory.AutomaticRecoveryEnabled = true;
        }

        /// <summary>
        /// 消息入队
        /// </summary>
        /// <typeparam name="TItem"></typeparam> 
        /// <param name="routingKey">路由关键字</param>
        /// <param name="message">消息实例</param>
        public static void Enqueue<T>(T message)
        {
            try
            {
                if (message != null)
                {
                    using (IConnection _connection = _connectionFactory.CreateConnection())
                    {
                        using (IModel _channel = _connection.CreateModel())
                        {
                            _channel.QueueDeclare(_queueName, true, false, false, null);

                            string messageString = JsonConvert.SerializeObject(message);

                            byte[] body = Encoding.UTF8.GetBytes(messageString);

                            var properties = _channel.CreateBasicProperties();
                            properties.DeliveryMode = 2;
                            properties.SetPersistent(true);//消息持久化

                            _channel.BasicPublish("", _queueName, properties, body);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Log4NetFile().Log(ex.Message);
            }
        }
    }
}
