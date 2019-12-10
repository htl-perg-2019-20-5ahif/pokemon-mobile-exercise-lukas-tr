using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace Pokemon
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {

        private static HttpClient HttpClient = new HttpClient() { BaseAddress = new Uri("https://pokeapi.co/api/v2/") };

        private ObservableCollection<Pokemon> PokemonValue;
        public ObservableCollection<Pokemon> Pokemon
        {
            get => PokemonValue;
            set
            {
                PokemonValue = value;
                OnPropertyChanged(nameof(Pokemon));
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            LoadPokemonAsync();
        }

        private async Task LoadPokemonAsync()
        {
            var pokemonResponse = await HttpClient.GetAsync($"pokemon/?offset=0&limit=2000");
            pokemonResponse.EnsureSuccessStatusCode();
            var responseBody = await pokemonResponse.Content.ReadAsStringAsync();
            var pokemons = JsonSerializer.Deserialize<Pokemons>(responseBody);
            Pokemon = pokemons.Results;
        }

        private async Task LoadPokemonDetailsAsync(string name)
        {
            var resp = await HttpClient.GetAsync($"pokemon/{name}");
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync();
            var details = JsonSerializer.Deserialize<Pokemon>(body);
            details.Loaded = true;
            var idx = Pokemon.IndexOf(Pokemon.First(p => p.Name == name));
            Pokemon[idx] = details;
        }

        private void ListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            var pokemon = e.Item as Pokemon;
            if (!pokemon.Loaded)
            {
                LoadPokemonDetailsAsync(pokemon.Name);
            }
        }

        async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushModalAsync(new Details(e.Item as Pokemon));
        }
    }
    public class Pokemon
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("weight")]
        public float Weight { get; set; }
        [JsonPropertyName("abilities")]
        public List<AbilitySlot> AbilitySlots { get; set; }
        [JsonPropertyName("sprites")]
        public Sprites Sprites { get; set; }
        public bool Loaded { get; set; }
    }

    public class Pokemons
    {
        [JsonPropertyName("results")]
        public ObservableCollection<Pokemon> Results { get; set; }
    }

    public class AbilitySlot
    {
        [JsonPropertyName("ability")]
        public Ability Ability { get; set; }
    }

    public class Ability
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Sprites
    {
        [JsonPropertyName("front_default")]
        public string Front { get; set; }
        [JsonPropertyName("back_default")]
        public string Back { get; set; }
    }

}
