using System.Linq;
using Futbol_9.Modelos;
using Futbol_9.Servicios;

namespace Futbol_9.Vistas;

public partial class PaginaJugadores : ContentPage
{
    private readonly ServicioBaseDatos _db = new();
    private List<Equipo> _equiposHabilitados = new();
    private List<Jugador> _jugadores = new();
    private Equipo? _equipoSel;

    public PaginaJugadores()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _db.InicializarAsync();
        await CargarEquiposHabilitadosAsync();
    }

    private async Task CargarEquiposHabilitadosAsync()
    {
        _equiposHabilitados = await _db.ListarEquiposHabilitadosAsync();
        pkEquipo.ItemsSource = _equiposHabilitados.Select(e => e.Nombre).ToList();
        pkEquipo.SelectedIndex = _equiposHabilitados.Count > 0 ? 0 : -1;

        if (_equiposHabilitados.Count == 0)
        {
            await DisplayAlert("Info", "No hay equipos habilitados. Complete el pago del plan para habilitar jugadores.", "OK");
            cvJugadores.ItemsSource = null;
            lblContador.Text = "0 / 18 jugadores";
        }
        else
        {
            await CargarJugadoresDelEquipoActualAsync();
        }
    }

    private Equipo? EquipoActual()
    {
        if (pkEquipo.SelectedIndex < 0 || pkEquipo.SelectedIndex >= _equiposHabilitados.Count) return null;
        return _equiposHabilitados[pkEquipo.SelectedIndex];
    }

    private async Task CargarJugadoresDelEquipoActualAsync()
    {
        _equipoSel = EquipoActual();
        if (_equipoSel is null)
        {
            cvJugadores.ItemsSource = null;
            lblContador.Text = "0 / 18 jugadores";
            return;
        }

        _jugadores = await _db.ListarJugadoresPorEquipoAsync(_equipoSel.EquipoId);
        cvJugadores.ItemsSource = _jugadores;
        lblContador.Text = $"{_jugadores.Count} / 18 jugadores";
    }

    private async void pkEquipo_SelectedIndexChanged(object sender, EventArgs e)
    {
        await CargarJugadoresDelEquipoActualAsync();
    }

    private async void AgregarJugador_Clicked(object sender, EventArgs e)
    {
        if (_equipoSel is null)
        {
            await DisplayAlert("Validación", "Seleccione un equipo habilitado.", "OK");
            return;
        }

        var nombre = txtJugadorNombre.Text?.Trim() ?? "";
        var movil = txtJugadorMovil.Text?.Trim() ?? "";
        var dni = txtJugadorDocumento.Text?.Trim() ?? "";
        var nroTxt = txtJugadorCamiseta.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(nombre))
        {
            await DisplayAlert("Validación", "Ingrese el nombre del jugador.", "OK");
            return;
        }

        // DMI 8 dígitos)
        if (string.IsNullOrWhiteSpace(dni) || dni.Length < 6)
        {
            await DisplayAlert("Validación", "Ingrese un DNI válido.", "OK");
            return;
        }

        
        if (string.IsNullOrWhiteSpace(movil) || movil.Length < 6)
        {
            await DisplayAlert("Validación", "Ingrese un número móvil válido.", "OK");
            return;
        }

        // N camiseta obligatorio, 1–99
        if (!int.TryParse(nroTxt, out var nro) || nro < 1 || nro > 99)
        {
            await DisplayAlert("Validación", "Ingrese un N° de camiseta válido (1–99).", "OK");
            return;
        }

        // Máximo 18
        var cantidad = (await _db.ListarJugadoresPorEquipoAsync(_equipoSel.EquipoId)).Count;
        if (cantidad >= 18)
        {
            await DisplayAlert("Límite", "Ya alcanzó el máximo de 18 jugadores.", "OK");
            return;
        }

        // Duplicados: nombre, DNI y camiseta dentro del equipo
        if (_jugadores.Any(j => string.Equals(j.NombreCompleto, nombre, StringComparison.OrdinalIgnoreCase)))
        {
            await DisplayAlert("Duplicado", "Ya existe un jugador con ese nombre en este equipo.", "OK");
            return;
        }
        if (_jugadores.Any(j => string.Equals(j.Documento, dni, StringComparison.OrdinalIgnoreCase)))
        {
            await DisplayAlert("Duplicado", "Ese DNI ya está registrado en este equipo.", "OK");
            return;
        }
        if (_jugadores.Any(j => j.NumeroCamiseta == nro))
        {
            await DisplayAlert("Duplicado", $"El N° de camiseta {nro} ya está registrado en este equipo.", "OK");
            return;
        }

        // Regla: el delegado NO puede ser jugador (evita mismo nombre)
        if (string.Equals(_equipoSel.DelegadoNombre, nombre, StringComparison.OrdinalIgnoreCase))
        {
            await DisplayAlert("Regla", "El delegado NO puede ser jugador.", "OK");
            return;
        }

        await _db.InsertarJugadorAsync(new Jugador
        {
            EquipoId = _equipoSel.EquipoId,
            NombreCompleto = nombre,
            Documento = dni,
            TelefonoMovil = movil,
            NumeroCamiseta = nro
        });

        // Limpiar inputs
        txtJugadorNombre.Text = "";
        txtJugadorDocumento.Text = "";
        txtJugadorMovil.Text = "";
        txtJugadorCamiseta.Text = "";

        await CargarJugadoresDelEquipoActualAsync();
    }

    private async void EliminarJugador_Clicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        var jugador = btn.BindingContext as Jugador;
        if (jugador is null) return;

        await _db.EliminarJugadorAsync(jugador.JugadorId);
        await CargarJugadoresDelEquipoActualAsync();
    }
}