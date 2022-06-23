using ConsoleTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPC.RabbitMQ;
using EasyNetQ;
using SPC.Services.COM;
using pbs.Helper;
using SPC.Services.COM.RabbitMQ;

namespace SPC.BO.RabbitMQ
{
    internal class Program
    {
        static void Main(string[] args)
        {
            subscribers = new List<EasyNetQ.ISubscriptionResult>();

            var menu = new ConsoleMenu(args, level: 0)
                          .Add("Message", async () => await RunAction("Message"))
                          .Add("Invalidate", async () => await RunAction("Invalidate"))
                          .Add("AddSubscriber", async () => await RunAction("AddSubscriber"))
                          .Add("Exit", () =>
                              {
                                 
                                  var theBus = MQService.Instance.GetServiceBus();
                                 
                                  theBus.Dispose();

                                  Console.Clear();
                                  
                                  Console.WriteLine("Goodbye!!!");
                                  
                                  Console.ReadKey();
                                  
                                  Environment.Exit(0);
                                  
                              }
                          )
                          .Configure(config =>
                          {
                              config.Selector = "--> ";
                              config.EnableFilter = true;
                              config.Title = "Queue menu";
                              config.EnableWriteTitle = true;
                              config.EnableBreadcrumb = true;
                          });

            menu.Show();


        }


        private static List<EasyNetQ.ISubscriptionResult> subscribers;

        private static async Task RunAction(string cmd)
        {
            try
            {
                string msg = "";

                switch (cmd)
                {
                    case "Message":
                        Console.Write("Enter message: ");

                        msg = Console.ReadLine();

                        if (!string.IsNullOrEmpty(msg))
                        {
                           Message themsg = Message.TestMessage();

                            themsg.CommandUrl = msg;

                            themsg.MessageBody = msg;

                            themsg.DataOperation = "message";

                            await MQService.PublishAsync(themsg);
                                                        

                        }
                        break;
                        
                    case "Invalidate":
                        Console.Write("Enter class id: ");

                        msg = Console.ReadLine();

                        if (!string.IsNullOrEmpty(msg))
                        {
                            Message themsg = Message.InvalidateMessage(msg);

                            themsg.MessageData.InsertUpdate("Entity", "VSA");

                           themsg.MessageBody = msg;

                            await MQService.PublishAsync(themsg);


                           //pbs.BO.PS.InvalidateMessage themsg1 = pbs.BO.PS.InvalidateMessage.NewMessage(msg,"Warning");

                           // await MQService.PublishAsync(themsg1);

                        }
                        break;
                       

                    case "AddSubscriber":
                        
                        Console.Write("Enter queue name: ");

                        string queueName = Console.ReadLine();

                        if (!string.IsNullOrEmpty(queueName))
                        {
                            IBus theBus = MQService.Instance.GetServiceBus();

                           EasyNetQ.IPubSub pubsub = theBus.PubSub;

                            var newSub = pubsub.Subscribe<SPC.Services.COM.IPubSubMessage>(queueName, (IPubSubMessage message) =>
                             {
                                 Console.WriteLine();

                                 Console.WriteLine($"Subscriber {queueName} received message ({message.MessageType}): {message.Originator} {message.MessageData.GetValueByKey("Action", "")} {message.MessageBody}: {message.MessageData.GetValueByKey("ObjectId", "")}");

                             },
                             (config) =>
                             {
                                 config.WithAutoDelete()
                                 .WithQueueName($"Console_{Environment.MachineName}_{queueName}");
                             });




                            subscribers.Add(newSub);

                            Console.Write($"Created new subscriber: {queueName}");
                        }

                        

                        break;
                    default:
                        break;
                }


            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.InnerException.Message}");
            }
            //Console.WriteLine("Press any key to continue");
            //Console.ReadKey();

        }
    }
}
