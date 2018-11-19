using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinNavigationTest
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MenuPage : ContentPage
	{
		public MenuPage ()
		{
			InitializeComponent ();
		}

        /// <summary>
        /// [NavigateToNextPage]ボタン押下時に呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnTransitPreviousPage(object sender, EventArgs e)
        {
            //await Navigation.PopAsync(true);
            await Navigation.PopAsync(false);
        }

        private async void OnTransitContents1(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ContentsPage1(), false);
        }

        private async void OnTransitContents2(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ContentsPage2(), false);
        }

        private async void OnTransitContents3(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ContentsPage3(), false);
        }

        private async void OnTransitContents4(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ContentsPage4(), false);
        }

        private async void OnTransitContents5(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ContentsPage5(), false);
        }

        private async void OnTransitContents6(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ContentsPage6(), false);
        }

    }
}