using System;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinNavigationTest
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ContentsPage1 : ContentPage
	{

        private ObservableCollection<Person> persons;

        public ContentsPage1 ()
		{
			InitializeComponent ();

            // ListViewにデータバインド
            // データソースオブジェクトを初期化します。
            persons = new ObservableCollection<Person>();

            this.persons.Add(new Person()
            {
                Name = "A",
                Number = "000-0000-0000",
                Address = "Tokyo 0",
                Image = ImageSource.FromResource("XamarinNavigationTest.Resources.apple.png")
            });
            this.persons.Add(new Person()
            {
                Name = "B",
                Number = "111-1111-1111",
                Address = "Tokyo 1",
                Image = ImageSource.FromResource("XamarinNavigationTest.Resources.apple.png")
            });
            this.persons.Add(new Person()
            {
                Name = "C",
                Number = "222-2222-2222",
                Address = "Tokyo 2",
                Image = ImageSource.FromResource("XamarinNavigationTest.Resources.apple.png")
            });
            this.persons.Add(new Person()
            {
                Name = "D",
                Number = "333-3333-3333",
                Address = "Tokyo 3",
                Image = ImageSource.FromResource("XamarinNavigationTest.Resources.apple.png")
            });
            this.persons.Add(new Person()
            {
                Name = "E",
                Number = "444-4444-4444",
                Address = "Tokyo 4",
                Image = ImageSource.FromResource("XamarinNavigationTest.Resources.apple.png")
            });

            this.list1.ItemsSource = this.persons;
        }

        public class Person
        {
            public string Name      { get; set; }
            public string Number    { get; set; }
            public string Address   { get; set; }
            public ImageSource Image { get; set; }
        }

        /****************************************************************************
         * 
         * 
         * 
         * 
         ****************************************************************************/
        void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }
            DisplayAlert("Item Selected", e.SelectedItem.ToString(), "Ok");
        }

        void OnRefresh(object sender, EventArgs e)
        {
            //put your refreshing logic here
            Person[] tmpList = new Person[persons.Count];
            persons.CopyTo(tmpList, 0);
            persons.Clear();
            foreach (var s in tmpList.Reverse())
            {
                persons.Add(s);
            }

            //make sure to end the refresh state
            ListView list = (ListView)sender;
            list.IsRefreshing = false;
        }

        void OnTap(object sender, ItemTappedEventArgs e)
        {
            DisplayAlert("Item Tapped", e.Item.ToString(), "Ok");
        }

        /****************************************************************************
         * 
         * 
         * 
         * 
         ****************************************************************************/
        void OnCall(object sender, EventArgs e)
        {

        }

        void OnMore(object sender, EventArgs e)
        {
            var menu = (MenuItem)sender;

            this.persons.Add(new Person()
            {
                Name = "Test Name",
                Number = "???-????-????",
                Address = "Test Address",
                Image = ImageSource.FromResource("XamarinNavigationTest.Resources.apple.png")
            });

            DisplayAlert("More Context Action", menu.CommandParameter + " more context action", "OK");
        }

        void OnDelete(object sender, EventArgs e)
        {
            var menu = (MenuItem)sender;
            var item = (Person)menu.CommandParameter;

            persons.Remove(item);
            //DisplayAlert("Delete Context Action", menu.CommandParameter.ToString() + " delete context action", "OK");
        }
    }
}