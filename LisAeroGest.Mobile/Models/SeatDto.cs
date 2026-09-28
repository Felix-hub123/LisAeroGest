using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace LisAeroGest.Mobile.Models
{
    public partial class SeatDto : ObservableObject
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string SeatClass { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        public decimal BasePrice { get; set; }


        // =====================================================
        // ESTADO DE SELEÇÃO
        // =====================================================

        [ObservableProperty]
        private bool _isSelected;


        // =====================================================
        // PROPRIEDADES VISUAIS
        // =====================================================

        public bool IsOccupied => !IsAvailable;

        public string StatusText
        {
            get
            {
                if (!IsAvailable)
                    return "OCUPADO";

                if (IsSelected)
                    return "SELECIONADO";

                return "DISPONÍVEL";
            }
        }

        public string DisplayPrice =>
            BasePrice > 0
                ? BasePrice.ToString(
                    "C",
                    new CultureInfo("pt-PT"))
                : string.Empty;


        // =====================================================
        // ATUALIZAR ESTADO VISUAL
        // =====================================================

        partial void OnIsSelectedChanged(bool value)
        {
            OnPropertyChanged(nameof(StatusText));
        }
    }
}