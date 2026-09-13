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

            for (int i = -7; i <= 7; i++)
            {
                var date = today.AddDays(i);
                var isSelected = vm != null && date.Date == vm.SelectedDate.Date;
                var isToday = date == today;

                var dayButton = new Border
                {
                    Padding = new Thickness(10, 6),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                    Stroke = isSelected ? Color.FromArgb("#1F5C99") : Color.FromArgb("#DDDDDD"),
                    BackgroundColor = isSelected ? Color.FromArgb("#1F5C99") :
                                       isToday ? Color.FromArgb("#FFF3CD") : Colors.White,
                    StrokeThickness = isSelected ? 2 : 1,
                    WidthRequest = 60,
                    HeightRequest = 70
                };

                var stack = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center };

                var dayName = new Label
                {
                    Text = date.ToString("ddd", new System.Globalization.CultureInfo("pt-PT")).Substring(0, 3),
                    FontSize = 10,
                    TextColor = isSelected ? Colors.White : Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center
                };

                var dayNumber = new Label
                {
                    Text = date.Day.ToString(),
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = isSelected ? Colors.White : Color.FromArgb("#1F5C99"),
                    HorizontalOptions = LayoutOptions.Center
                };

                var monthLabel = new Label
                {
                    Text = date.ToString("MMM", new System.Globalization.CultureInfo("pt-PT")).Substring(0, 3),
                    FontSize = 9,
                    TextColor = isSelected ? Colors.White : Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center
                };

                stack.Children.Add(dayName);
                stack.Children.Add(dayNumber);
                stack.Children.Add(monthLabel);

                dayButton.Content = stack;

                // Adicionar gesto de clique
                var tapGesture = new TapGestureRecognizer();
                var selectedDate = date;
                tapGesture.Tapped += async (s, e) =>
                {
                    if (vm != null)
                    {
                        await vm.SelectDateCommand.ExecuteAsync(selectedDate);
                        BuildCalendarStrip(); // Redesenhar
                    }
                };
                dayButton.GestureRecognizers.Add(tapGesture);

                CalendarStrip.Children.Add(dayButton);
            }
        }
    }
}