using MoniterMaster.websocket.behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp.Server;

namespace MoniterMaster.websocket
{
    class WServer
    {
        WebSocketServer wssv;
        public WServer() {
            wssv = new WebSocketServer(4649);
            init();
        }

        public void init() {
           // wssv.AddWebSocketService<Chat>("/chat");
        }

        public void addChat(Func<Chat> chat) {
            wssv.AddWebSocketService<Chat>("/chat",chat);
        }
        public void start() {
            wssv.Start();
          
        }

        public void send(Msg msg) {
            Console.WriteLine("user count :"+wssv.WebSocketServices.SessionCount);
            
            wssv.WebSocketServices.Broadcast(msg.toBytes());
        }
    }
}
