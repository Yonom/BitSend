using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CupCake;
using CupCake.Messages.Receive;
using PlayerIOClient;

namespace BitSend.Demo
{
    public class Class1 : CupCakeMuffin
    {
        protected override void Enable()
        {
            this.Events.Bind<InitReceiveEvent>(this.OnInit);
        }

        private void OnInit(object sender, InitReceiveEvent e)
        {
            var client = new BitSendClient(this.ConnectionPlatform.Connection, this.PlayerService.OwnPlayer.UserId);
            var bytes = new byte[1024];
            for (byte i = 0; i < 255; i++)
            {
                bytes[i] = i;
                bytes[256 + i] = i;
                bytes[256 + 256 + i] = i;
                bytes[256 + 256 + 256 + i] = i;
            }
            client.Send(//bytes);
            Encoding.Unicode.GetBytes(
                "HaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHai" +
                "HaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHai" +
                "HaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHai" +
                "HaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHai" +
                "HaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHai" +
                "HaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHai" +
                "HaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHaiHai"));
        }
    }
}