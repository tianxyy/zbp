using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;

namespace MoniterMaster.websocket
{
    public class Msg
    {
        public Msg() {
        }

        public Msg(byte[] data) {
            string json = Encoding.GetEncoding("utf-8").GetString(data);
            Msg tmp  = JsonConvert.DeserializeObject<Msg>(json);
            this.data = tmp.data;
            this.message = tmp.message;
            this.type = tmp.type;
        }
        public int type { get; set; }
        public byte[] data { get; set; }

        public string message { get; set; }

        public byte[] toBytes() {
            string json = JsonConvert.SerializeObject(this);
            return Encoding.GetEncoding("UTF-8").GetBytes(json);
        }
        
    }
}
