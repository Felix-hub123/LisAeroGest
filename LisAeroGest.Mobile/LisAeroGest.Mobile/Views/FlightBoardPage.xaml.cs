using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class FlightBoardPage : ContentPage
    {
        public FlightBoardPage(FlightBoardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            BuildCalendarStrip();

            if (BindingContext is FlightBoardViewModel vm)
            {
                await vm.LoadDeparturesCommand.ExecuteAsync(null);
            }
        }

        private void BuildCalendarStrip()
        {
            CalendarStrip.Children.Clear();

            var today = DateTime.Today;
            var vm = BindingContext as FlightBoardViewModel;

            if (vm == null)
                return;

            for (int i = -7; i <= 7; i++)
            {
                var date = today.AddDays(i);
                var isSelected = date.Date == vm.SelectedDate.Date;
                var isToday = date.Date == today.Date;

                var dayButton = new Button
                {
                    Text = $"{date:ddd}\n{date.Day}\n{date:MMM}",
                    BackgroundColor = isSelected
                        ? Color.FromArgb("#1F5C99")
                        : isToday
                            ? Color.FromArgb("#FFF3CD")
                            : Colors.White,
                    TextColor = isSelected
                        ? Colors.White
                        : Color.FromArgb("#1F5C99"),
                    BorderColor = isSelected
                        ? Color.FromArgb("#1F5C99")
                        : Color.FromArgb("#DDDDDD"),
                    BorderWidth = 1,
                    WidthRequest = 60,
                    HeightRequest = 70,
                    CornerRadius = 10,
                    FontSize = 10,
                    Padding = 0,
                    FontAttributes = FontAttributes.Bold,
                    Command = vm.SelectDateCommand,
                    CommandParameter = date
                };

                CalendarStrip.Children.Add(dayButton);
            }
        }

        private async void OnFlightTapped(object sender, TappedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                "[BOARD] OnFlightTapped foi chamado.");

            await DisplayAlert(
                "Diagnóstico",
                "O toque no voo foi detectado.",
                "OK");

            if (e.Parameter is not FlightDto flight)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[BOARD] O parâmetro não é um FlightDto.");

                await DisplayAlert(
                    "Erro",
                    "O voo seleccionado não foi recebido.",
                    "OK");

                return;
            }

            System.Diagnostics.Debug.WriteLine(
                $"[BOARD] Voo seleccionado: {flight.Id} - {flight.FlightNumber}");

            if (sender is VisualElement element)
            {
                await element.FadeTo(0.45, 70);
                await element.FadeTo(1.0, 70);
            }

            if (BindingContext is FlightBoardViewModel vm
                && vm.SelectFlightCommand.CanExecute(flight))
            {
                System.Diagnostics.Debug.WriteLine(
                    "[BOARD] A executar SelectFlightCommand.");

                await vm.SelectFlightCommand.ExecuteAsync(flight);
            }
            else
            {
                await DisplayAlert(
                    "Erro",
                    "SelectFlightCommand não pode ser executado.",
                    "OK");
            }
        }

    }
}
