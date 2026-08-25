using LisAeroGest.Data.Entities;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel para o ecrã de confirmação da reserva.
    /// </summary>
    public class CheckoutConfirmationViewModel
    {
        /// <summary>
        /// Bilhete reservado.
        /// </summary>
        public Ticket? Ticket { get; set; }

        /// <summary>
        /// Voo associado ao bilhete.
        /// </summary>
        public Flight? Flight { get; set; }

        /// <summary>
        /// Lugar selecionado.
        /// </summary>
        public Seat? Seat { get; set; }

        /// <summary>
        /// Passageiro que fez a reserva.
        /// </summary>
        public Passenger? Passenger { get; set; }

        /// <summary>
        /// Preço total da reserva.
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Indica se foi escolhida bagagem extra.
        /// </summary>
        public bool ExtraLuggage { get; set; }

        /// <summary>
        /// Indica se foi escolhida refeição a bordo.
        /// </summary>
        public bool MealIncluded { get; set; }

        /// <summary>
        /// Verifica se o passageiro é convidado (não tem conta).
        /// </summary>
        public bool IsGuest => Passenger?.UserId == null;
    }
}