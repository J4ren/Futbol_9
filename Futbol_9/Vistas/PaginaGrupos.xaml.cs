using System.Linq;
using Futbol_9.Modelos;
using Futbol_9.Servicios;

namespace Futbol_9.Vistas;

public partial class PaginaGrupos : ContentPage
{
    private readonly ServicioBaseDatos _db = new();
    private List<Campeonato> _campeonatos = new();
    private List<Equipo> _equiposFuente = new();
    private Dictionary<string, List<Equipo>> _preview = new();

    public PaginaGrupos()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _db.InicializarAsync();
        await CargarCampeonatosAsync();
        await RecargarEquiposFuenteAsync();
    }

    private async Task CargarCampeonatosAsync()
    {
        _campeonatos = await _db.ListarCampeonatosAsync();
        pkCampeonato.ItemsSource = _campeonatos.Select(c => $"{c.Nombre} {(c.Activo ? "(Activo)" : "")}").ToList();

        var activo = await _db.ObtenerCampeonatoActivoAsync();
        if (activo != null)
        {
            var idx = _campeonatos.FindIndex(c => c.CampeonatoId == activo.CampeonatoId);
            pkCampeonato.SelectedIndex = idx;
        }
        else
        {
            pkCampeonato.SelectedIndex = _campeonatos.Count > 0 ? 0 : -1;
        }
    }

    private async Task RecargarEquiposFuenteAsync()
    {
        if (pkCampeonato.SelectedIndex < 0 || pkCampeonato.SelectedIndex >= _campeonatos.Count)
        {
            _equiposFuente = new();
            return;
        }

        var campId = _campeonatos[pkCampeonato.SelectedIndex].CampeonatoId;
        var todos = await _db.ListarEquiposAsync();
        var delCampeonato = todos.Where(e => e.CampeonatoId == campId).ToList();

        if (swSoloHabilitados.IsToggled)
        {
            var habil = new List<Equipo>();
            foreach (var e in delCampeonato)
                if (await _db.EquipoHabilitadoParaJugadoresAsync(e.EquipoId))
                    habil.Add(e);
            _equiposFuente = habil;
        }
        else
        {
            _equiposFuente = delCampeonato;
        }
    }

    private async void Generar_Clicked(object sender, EventArgs e)
    {
        await RecargarEquiposFuenteAsync();

        if (_equiposFuente.Count == 0)
        {
            await DisplayAlert("Info", "No hay equipos para agrupar.", "OK");
            return;
        }

        if (!int.TryParse(txtCantidadGrupos.Text?.Trim(), out int n) || n < 1)
        {
            await DisplayAlert("Validación", "Ingrese una cantidad de grupos válida.", "OK");
            return;
        }

        if (n > _equiposFuente.Count)
        {
            await DisplayAlert("Validación", "La cantidad de grupos no puede exceder el número de equipos.", "OK");
            return;
        }

        var nombres = ServicioGrupos.GenerarNombres(n);
        _preview = ServicioGrupos.RepartirEquipos(_equiposFuente, nombres);

        // Bind a la vista previa
        var data = _preview.Select(kv => new GrupoConNombre
        {
            NombreGrupo = $"Grupo {kv.Key}",
            Equipos = kv.Value
        }).ToList();

        cvResultado.ItemsSource = data;
    }

    private async void Guardar_Clicked(object sender, EventArgs e)
    {
        if (pkCampeonato.SelectedIndex < 0 || pkCampeonato.SelectedIndex >= _campeonatos.Count)
        {
            await DisplayAlert("Validación", "Seleccione un campeonato.", "OK");
            return;
        }
        if (_preview.Count == 0)
        {
            await DisplayAlert("Validación", "Genere los grupos antes de guardar.", "OK");
            return;
        }

        var campId = _campeonatos[pkCampeonato.SelectedIndex].CampeonatoId;

        // Limpia y guarda
        await _db.LimpiarGruposAsync(campId);
        await _db.CrearGruposAsync(campId, _preview.Keys);

        // Obtén ids reales de grupos creados
        var gruposGuardados = await _db.ListarGruposPorCampeonatoAsync(campId);
        var mapa = gruposGuardados.ToDictionary(g => g.Nombre, g => g.GrupoId);

        foreach (var (nombre, equipos) in _preview)
        {
            var gid = mapa[nombre];
            foreach (var eq in equipos)
                await _db.AsignarEquipoAGrupoAsync(gid, eq.EquipoId);
        }

        await DisplayAlert("OK", "Grupos guardados correctamente.", "Continuar");
    }
}