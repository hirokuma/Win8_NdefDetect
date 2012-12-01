#define USE_TOAST
#define USE_TILE

using NotificationsExtensions.ToastContent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Proximity;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace _02_NdefDetect
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Windows.Networking.Proximity.ProximityDevice proximityDevice;

        
        public MainPage()
        {
            this.InitializeComponent();
            proximityDevice = ProximityDevice.GetDefault();

        }

        /// <summary>
        /// このページがフレームに表示されるときに呼び出されます。
        /// </summary>
        /// <param name="e">このページにどのように到達したかを説明するイベント データ。Parameter 
        /// プロパティは、通常、ページを構成するために使用します。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (proximityDevice != null)
            {
                //カード検出(うっとうしいのでコメントアウト)
                //proximityDevice.DeviceArrived += DeviceArrived;
                //proximityDevice.DeviceDeparted += DeviceDeparted;

                //メッセージ受信
                //proximityDevice.SubscribeForMessage("NDEF", messageReceived);         //例外発生
                //proximityDevice.SubscribeForMessage("NDEF:wkt", messageReceived);     //例外発生
                //複数設定は可能なようだ。
                proximityDevice.SubscribeForMessage("NDEF:wkt.T", messageReceived);   //TEXT
                proximityDevice.SubscribeForMessage("NDEF:wkt.U", messageReceived);   //URI
                proximityDevice.SubscribeForMessage("NDEF:wkt.Sp", messageReceived);  //SmartPoster
            }
            else
            {
                MessageDialog dlg = new MessageDialog("NFC R/Wがありません", "No R/W");
            }
        }

        private void DeviceArrived(ProximityDevice proximityDevice)
        {
#if USE_TILE
            var tile = NotificationsExtensions.TileContent.TileContentFactory.CreateTileSquareText01();
            tile.TextHeading.Text = "カード！";
            tile.TextBody1.Text = "detect";
            tile.TextBody2.Text = "見つけた";
            tile.TextBody3.Text = "ouch";
            var tileNfy = tile.CreateNotification();
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNfy);
#endif  //USE_TILE

#if USE_TOAST
            showToast(proximityDevice.DeviceId);
#endif //USE_TOAST
        }

        private void DeviceDeparted(ProximityDevice proximityDevice)
        {
#if USE_TILE
            var tile = NotificationsExtensions.TileContent.TileContentFactory.CreateTileSquareText01();
            var tileNfy = tile.CreateNotification();
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNfy);
#endif //USE_TILE
        }


        private void showToast(string str)
        {
            var toast = ToastContentFactory.CreateToastText01();
            toast.TextBodyWrap.Text = str;
            toast.Duration = NotificationsExtensions.ToastContent.ToastDuration.Long;

            //Shortだと設定できない(実行時に例外発生)！
            toast.Audio.Content = NotificationsExtensions.ToastContent.ToastAudioContent.LoopingAlarm;
            toast.Audio.Loop = true;

            //なんだろう？
            //toast.Launch = "{\"type\":\"toast\",\"param1\":\"12345\",\"param2\":\"67890\"}";

            var toastNfy = toast.CreateNotification();
            ToastNotificationManager.CreateToastNotifier().Show(toastNfy);
        }

        private void messageReceived(ProximityDevice proximityDevice, ProximityMessage message)
        {
            var rawMsg = WindowsRuntimeBufferExtensions.ToArray(message.Data);
            //このrawMsgをNDEF解析すればよいようだ
            //NDEF解析はOSでやってくれないらしい。OSがやるのはNFPだけか。

            string str = "メッセージ";
            switch (rawMsg[3])
            {
                case (byte)'T':
                    str += "TEXT";
                    break;
                case (byte)'U':
                    str += "URI";
                    break;
                case (byte)'S':
                    if (rawMsg[4] == (byte)'p')
                    {
                        str += "SmartPoster";
                    }
                    break;
            }
            showToast(str);
        }

    }
}
