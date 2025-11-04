namespace Futbol_9.Vistas;

public partial class PaginaInicio : ContentPage
{
    public PaginaInicio() => InitializeComponent();

    async void GoEquipos_Clicked(object s, EventArgs e) => await Shell.Current.GoToAsync("//equipos");
    async void GoPlanes_Clicked(object s, EventArgs e) => await Shell.Current.GoToAsync("//planes");
    async void GoPagos_Clicked(object s, EventArgs e) => await Shell.Current.GoToAsync("//pagos");
}
