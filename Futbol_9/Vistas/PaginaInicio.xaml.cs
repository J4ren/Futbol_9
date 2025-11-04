using Futbol_9.Modelos;
using Futbol_9.Servicios;

namespace Futbol_9.Vistas;

public partial class PaginaInicio : ContentPage
{
    private readonly ServicioBaseDatos _db = new();

    public PaginaInicio()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _db.InicializarAsync();
        await CargarDashboardAsync();
        await CargarAvisosAsync();
    }

    private async Task CargarDashboardAsync()
    {
        var (camp, equipos, jugadores, equiposPagados, grupos) = await _db.ObtenerResumenAsync();

        if (camp is null)
        {
            lblTituloCamp.Text = "Sin campeonato activo";
            lblFechasCamp.Text = "Cree o active un campeonato en la pestaña correspondiente.";
            lblEquipos.Text = "0";
            lblJugadores.Text = "0";
            lblEquiposPagados.Text = "0";
            lblGrupos.Text = "0";
            return;
        }

        lblTituloCamp.Text = camp.Nombre;
        lblFechasCamp.Text = $"Inscripciones: {camp.InscripcionInicio:dd/MM/yyyy} - {camp.InscripcionFin:dd/MM/yyyy}";
        lblJugadores.Text = jugadores.ToString();
        lblEquiposPagados.Text = equiposPagados.ToString();
        lblGrupos.Text = grupos.ToString();
    }

    private async Task CargarAvisosAsync()
    {
        var lista = await _db.ListarAvisosAsync();
        cvAvisos.ItemsSource = lista;
    }

    private async void AgregarAviso_Clicked(object sender, EventArgs e)
    {
        string titulo = await DisplayPromptAsync("Nuevo aviso", "Título del aviso:");
        if (string.IsNullOrWhiteSpace(titulo)) return;

        string mensaje = await DisplayPromptAsync("Nuevo aviso", "Mensaje del aviso:");
        if (string.IsNullOrWhiteSpace(mensaje)) return;

        await _db.InsertarAvisoAsync(new Aviso { Titulo = titulo.Trim(), Mensaje = mensaje.Trim(), Fecha = DateTime.Now });
        await CargarAvisosAsync();
    }

    private async void EliminarAviso_Clicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        var aviso = btn.BindingContext as Aviso;
        if (aviso is null) return;

        bool ok = await DisplayAlert("Confirmar", $"¿Eliminar el aviso '{aviso.Titulo}'?", "Sí", "No");
        if (!ok) return;

        await _db.EliminarAvisoAsync(aviso.AvisoId);
        await CargarAvisosAsync();
    }

    // Accesos rápidos
    async void GoEquipos_Clicked(object s, EventArgs e) => await Shell.Current.GoToAsync("//equipos");
    async void GoPagos_Clicked(object s, EventArgs e) => await Shell.Current.GoToAsync("//pagos");
    async void GoGrupos_Clicked(object s, EventArgs e) => await Shell.Current.GoToAsync("//grupos");
    async void GoSorteo_Clicked(object s, EventArgs e) => await Shell.Current.GoToAsync("//asignacion");
    async void GoJugadores_Clicked(object s, EventArgs e) => await Shell.Current.GoToAsync("//jugadores");
    async void GoResumen_Clicked(object s, EventArgs e) => await Shell.Current.GoToAsync("//equipos-resumen");
}