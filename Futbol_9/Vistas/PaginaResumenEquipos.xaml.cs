using Futbol_9.Servicios;


namespace Futbol_9.Vistas;

public partial class PaginaResumenEquipos : ContentPage
{
    private readonly ServicioBaseDatos _db = new();

    public PaginaResumenEquipos()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _db.InicializarAsync();
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        var data = await _db.ListarResumenEquiposAsync();
        cvResumen.ItemsSource = data;
    }

    private async void Actualizar_Clicked(object sender, EventArgs e)
    {
        await CargarAsync();
    }
}