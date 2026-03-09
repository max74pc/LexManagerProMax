using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LexManagerProMax.Application.Interfaces;
using System;
using System.Collections.Generic;

namespace LexManagerProMax.AvaloniaApp.ViewModels.Utilita;

// TODO - Da implementare ICalcolatoreScadenzeService

//public partial class UtilitaViewModel(c scadenze) : ObservableObject
//{
//    private readonly ICalcolatoreScadenzeService _scadenze = scadenze;

//    // --- Calcolo scadenze ---
//    [ObservableProperty] private DateTime _dataDecorrenza = DateTime.Today;
//    [ObservableProperty] private int _giorniTermine = 30;
//    [ObservableProperty] private bool _escludioSabato = true;
//    [ObservableProperty] private bool _escludiDomenica = true;
//    [ObservableProperty] private string _dataScadenzaCalcolata = string.Empty;
//    [ObservableProperty] private string _noteScadenza = string.Empty;

//    // --- Convertitore CF ---
//    [ObservableProperty] private string _inputCf = string.Empty;
//    [ObservableProperty] private string _risultatoCf = string.Empty;

//    // --- Calcolo parcella orientativa ---
//    [ObservableProperty] private decimal _valoreControversia;
//    [ObservableProperty] private string _tipoAttivita = "Fase Istruttoria";
//    [ObservableProperty] private decimal _parcellaMinimaCalcolata;
//    [ObservableProperty] private decimal _parcellaMediaCalcolata;
//    [ObservableProperty] private decimal _parcellaMassimaCalcolata;

//    // --- Contatore parole ---
//    [ObservableProperty] private string _testoContatore = string.Empty;
//    [ObservableProperty] private int _numeroParole;
//    [ObservableProperty] private int _numeroCaratteri;
//    [ObservableProperty] private int _numeroPagine;

//    [RelayCommand]
//    private void CalcolaScadenza()
//    {
//        var dataFine = _scadenze.CalcolaScadenza(
//            DataDecorrenza, GiorniTermine, EscludioSabato, EscludiDomenica);

//        DataScadenzaCalcolata = dataFine.ToString("dddd dd MMMM yyyy");
//        NoteScadenza = dataFine < DateTime.Today
//            ? "⚠️ ATTENZIONE: scadenza già decorsa!"
//            : $"Mancano {(dataFine - DateTime.Today).Days} giorni";
//    }

//    [RelayCommand]
//    private void DecodificaCf()
//    {
//        if (string.IsNullOrWhiteSpace(InputCf) || InputCf.Length != 16)
//        {
//            RisultatoCf = "Codice fiscale non valido (richiesti 16 caratteri)";
//            return;
//        }

//        try
//        {
//            var cf = InputCf.ToUpperInvariant();
//            var mesiMap = new Dictionary<char, int>
//            {
//                ['A'] = 1,
//                ['B'] = 2,
//                ['C'] = 3,
//                ['D'] = 4,
//                ['E'] = 5,
//                ['H'] = 6,
//                ['L'] = 7,
//                ['M'] = 8,
//                ['P'] = 9,
//                ['R'] = 10,
//                ['S'] = 11,
//                ['T'] = 12
//            };

//            var anno = cf[6..8];
//            var mese = mesiMap.TryGetValue(cf[8], out var m) ? m : 0;
//            var giornoRaw = int.Parse(cf[9..11]);
//            var giorno = giornoRaw > 40 ? giornoRaw - 40 : giornoRaw;
//            var sesso = giornoRaw > 40 ? "F" : "M";
//            var comuneCode = cf[11..15];

//            RisultatoCf = $"Sesso: {sesso}\nData nascita: {giorno:D2}/{mese:D2}/19{anno} (o 20{anno})\nCodice comune: {comuneCode}";
//        }
//        catch { RisultatoCf = "Errore nella decodifica"; }
//    }

//    [RelayCommand]
//    private void CalcolaParcella()
//    {
//        // Parametri orientativi basati sul D.M. 55/2014
//        var (min, med, max) = ValoreControversia switch
//        {
//            <= 1_100 => (270m, 500m, 900m),
//            <= 5_200 => (500m, 1_100m, 2_200m),
//            <= 26_000 => (1_100m, 2_800m, 5_600m),
//            <= 52_000 => (2_200m, 4_500m, 9_000m),
//            <= 260_000 => (4_000m, 8_000m, 16_000m),
//            <= 520_000 => (6_500m, 12_000m, 24_000m),
//            _ => (10_000m, 20_000m, 40_000m)
//        };

//        ParcellaMinimaCalcolata = min;
//        ParcellaMediaCalcolata = med;
//        ParcellaMassimaCalcolata = max;
//    }

//    partial void OnTestoContatoreChanged(string value)
//    {
//        var words = value.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);
//        NumeroParole = words.Length;
//        NumeroCaratteri = value.Length;
//        NumeroPagine = (int)Math.Ceiling(NumeroParole / 250.0);
//    }
//}