using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;

namespace XamarinNavigationTest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContentsPage3 : ContentPage
    {
        Geocoder geoCoder;

        public ContentsPage3()
        {
            InitializeComponent();

        }

        /****************************************************************************
         * 
         * 
         * 
         * 
         ****************************************************************************/
        private void OnChangeMapSlider(object sender, ValueChangedEventArgs e)
        {
            var zoomLevel = e.NewValue;
            var latlongdegrees = 360 / (Math.Pow(2, zoomLevel));

            if ( map.VisibleRegion != null )
            {
                map.MoveToRegion(new MapSpan(map.VisibleRegion.Center, latlongdegrees, latlongdegrees));
            }
        }

        /****************************************************************************
         * 
         * 
         * 
         * 
         ****************************************************************************/
        private void OnChangeMapType(object sender, EventArgs e)
        {
            if ( ((Button)sender).Text == "Street" )
            {
                map.MapType = MapType.Street;
            }
            else if ( ((Button)sender).Text == "Hybrid" )
            {
                map.MapType = MapType.Hybrid;
            }
            else if ( ((Button)sender).Text == "Satellite" )
            {
                map.MapType = MapType.Satellite;
            }
            else
            {

            }
        }

        /****************************************************************************
         * 
         * 
         * 
         * 
         ****************************************************************************/
        async private void OnGetGeoPosition(object sender, EventArgs e)
        {
            geoCoder = new Geocoder();

            if (((Button)sender).Text == "Position1")
            {
                var fortMasonPosition = new Position(37.8044866, -122.4324132);

                var possibleAddresses = await geoCoder.GetAddressesForPositionAsync(fortMasonPosition);
                System.Console.WriteLine("Geocoder Test Position1 Clicked: {0},{1}", fortMasonPosition.Latitude, fortMasonPosition.Longitude);
                foreach (var a in possibleAddresses)
                {
                    System.Console.WriteLine("Geocoder Test Position1 Clicked: {0}", a);
                }


                var pin = new Pin
                {
                    Type = PinType.Place,
                    Position = fortMasonPosition,
                    Label = "Position1: Search address from postion.",
                    Address = possibleAddresses.First()
                };
                map.Pins.Clear();
                map.Pins.Add(pin);

                map.MoveToRegion(MapSpan.FromCenterAndRadius(fortMasonPosition, Distance.FromMiles(1)));

            }
            else if (((Button)sender).Text == "Position2")
            {
                var xamarinAddress = "394 Pacific Ave, San Francisco, California";
                var approximateLocation = await geoCoder.GetPositionsForAddressAsync(xamarinAddress);

                System.Console.WriteLine("Geocoder Test Position1 Clicked: {0}", xamarinAddress);
                foreach (var p in approximateLocation)
                {
                    System.Console.WriteLine("Geocoder Test Position1 Clicked: {0},{1}", p.Latitude, p.Longitude);
                }

                var pin = new Pin
                {
                    Type = PinType.Place,
                    Position = approximateLocation.First(),
                    Label = "Position2: Search position from address.",
                    Address = xamarinAddress
                };
                map.Pins.Clear();
                map.Pins.Add(pin);

                map.MoveToRegion(MapSpan.FromCenterAndRadius(approximateLocation.First(), Distance.FromMiles(1)));

            }
            else if (((Button)sender).Text == "Position3")
            {
                var position = new Position(37, -122); // Latitude, Longitude
                var pin = new Pin
                {
                    Type = PinType.Place,
                    Position = position,
                    Label = "custom pin",
                    Address = "custom detail info"
                };
                map.Pins.Clear();
                map.Pins.Add(pin);

                map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMiles(1)));

            }
            else
            {

            }
        }
    }
}