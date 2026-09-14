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

            // Construir o calendário horizontal
            BuildCalendarStrip();

            // Carregar os voos
            if (BindingContext is FlightBoardViewModel vm)
            {
                await vm.LoadDeparturesCommand.ExecuteAsync(null);
            }
        }

        /// <summary>
        /// Constrói uma faixa de calendário com 15 dias (7 antes, 7 depois).
        /// </summary>
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
                var isToday = date == today;

                var dayButton = new Button
                {
                    Text = $"{date:ddd}\n{date.Day}\n{date:MMM}",
                    BackgroundColor = isSelected ? Color.FromArgb("#1F5C99") :
                                       isToday ? Color.FromArgb("#FFF3CD") : Colors.White,
                    TextColor = isSelected ? Colors.White : Color.FromArgb("#1F5C99"),
                    BorderColor = isSelected ? Color.FromArgb("#1F5C99") : Color.FromArgb("#DDDDDD"),
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
    }
}