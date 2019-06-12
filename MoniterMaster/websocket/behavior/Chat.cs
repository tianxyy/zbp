using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace MoniterMaster.websocket.behavior
{
    class Chat : WebSocketBehavior
    {
        Func<Msg, int> SendMsg;
        public Chat() {

        }

        public void setMsgBack(Func<Msg, int> sm) {
            this.SendMsg = sm;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Type == Opcode.Binary) {
                Msg msg = new Msg(e.RawData);
                if (msg != null&&SendMsg!=null) {
                    SendMsg(msg);
                }
                if (msg.type == 1) {
                    msg.message = "recieved";
                    msg.data = new byte[] { 0};
                    msg.type = 2;
                    Send(msg.toBytes());

                }
            }
        }

       

    }
}
