using Futbol_9.Servicios;

namespace Futbol_9.Vistas;

public partial class PaginaEstadoEquipos : ContentPage
{
    private readonly ServicioBaseDatos _db = new();

    public PaginaEstadoEquipos()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _db.InicializarAsync();
        await CargarResumenAsync();
    }

    private async Task CargarResumenAsync()
    {
        var resumenes = await _db.ListarResumenesAsync();
        cvResumen.ItemsSource = resumenes;
    }

    private async void Actualizar_Clicked(object sender, EventArgs e)
    {
        await CargarResumenAsync();
    }
}