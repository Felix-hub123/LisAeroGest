using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.ViewModels;

/// <summary>
/// Communicações do turno — por agora é um hub simples que
/// redireciona para a página de notificações partilhada com passageiros.
/// Futuros endpoints de broadcast/announcements dariam lógica real aqui.
/// </summary>
public partial class OperationsCommunicationsViewModel : ObservableObject
{
    public OperationsCommunicationsViewModel()
    {
        // Sem dependências — usa a NotificationsPage partilhada
    }
}
