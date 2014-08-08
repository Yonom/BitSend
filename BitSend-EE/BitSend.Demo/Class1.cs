using System.Text;
using CupCake;
using CupCake.Messages.Receive;

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
            client.Send(
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