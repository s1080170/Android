using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinNavigationTest
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

        /// <summary>
        /// [NavigateToNextPage]ボタン押下時に呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnTransitNextPage(object sender, EventArgs e)
        {
            //await Navigation.PushAsync(new MenuPage(), true);
            await Navigation.PushAsync(new MenuPage(), false);
        }

        private void OnEnableHandsOn(object sender, EventArgs e)
        {
            if (buttonEnableHandsOn.Image == "SpeakerOn.png")
            {
                buttonEnableHandsOn.Image = "SpeakerOff.png";
            }
            else
            {
                buttonEnableHandsOn.Image = "SpeakerOn.png";
            }
        }

        private void OnChangeMode(object sender, EventArgs e)
        {
            if (buttonChangeMode.Text == "一斉")
            {
                buttonChangeMode.Text = "ｸﾞﾙｰﾌﾟ";
            }
            else if (buttonChangeMode.Text == "ｸﾞﾙｰﾌﾟ")
            {
                buttonChangeMode.Text = "個別";
            }
            else if (buttonChangeMode.Text == "個別")
            {
                buttonChangeMode.Text = "一斉";
            }
            else
            {
                buttonChangeMode.Text = "ALL";
            }
        }

        private void OnPttPressed(object sender, EventArgs e)
        {
            buttonPtt.Image = "PttOn.png";
        }

        private void OnPttReleased(object sender, EventArgs e)
        {
            buttonPtt.Image = "PttOff.png";
        }
    }
}
