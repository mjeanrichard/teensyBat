﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

using TeensyBatMap.Domain;

using WinRtLib;

namespace TeensyBatMap.ViewModels
{
	public class SimpleIntBin : IBin
	{
		public SimpleIntBin(double value, string label, bool isPeak)
		{
			Value = value;
			Label = label;
			SecondaryValue = value;
			IsHighlighted = isPeak;
		}

		public double Value { get; }
		public double SecondaryValue { get; }
		public string Label { get; }
		public bool IsHighlighted { get; set; }
	}

	public class BatCallViewModel : INotifyPropertyChanged
	{
		private readonly BatNodeLog _log;
		private readonly BatCall _batCall;
		private readonly FftAnalyzer _fftAnalyzer;
		private bool _isInitialized;
		private ObservableCollection<SimpleIntBin> _frequencies;

		public BatCallViewModel(BatNodeLog log, BatCall batCall, int index)
		{
			Index = index;
			_log = log;
			_batCall = batCall;
			_fftAnalyzer = new FftAnalyzer(2, 5);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool Enabled
		{
			get { return _batCall.Enabled; }
			set
			{
				if (_batCall.Enabled != value)
				{
					_batCall.Enabled = value;
					OnPropertyChanged();
				}
			}
		}

		public string MainFrequencies
		{
			get { return string.Format(CultureInfo.CurrentCulture, "{0} kHz", _batCall.MaxFrequency); }
		}

		public string Duration
		{
			get { return string.Format(CultureInfo.CurrentCulture, "{0} ms", _batCall.Duration / 1000); }
		}

		public string StartTime
		{
			get { return _log.LogStart.AddMilliseconds(_batCall.StartTimeMs).ToString("HH:mm:ss.fff", CultureInfo.CurrentCulture); }
		}

		public string StartTimeFull
		{
			get { return _log.LogStart.AddMilliseconds(_batCall.StartTimeMs).ToString("dd.MM.yyyy HH:mm:ss.fff", CultureInfo.CurrentCulture); }
		}

		public BatCall BatCall
		{
			get { return _batCall; }
		}

		public int Index { get; private set; }

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public ObservableCollection<SimpleIntBin> Frequencies
		{
			get { return _frequencies; }
			set
			{
				_frequencies = value;
				OnPropertyChanged();
			}
		}

		public void Initialize()
		{
			if (_isInitialized)
			{
				return;
			}
			_isInitialized = true;

			SimpleIntBin[] simpleIntBins = new SimpleIntBin[255];

			FftResult fftResult = _fftAnalyzer.Analyze(_batCall);
			int iPeak = 0;
			for (int i = 1; i < fftResult.FftData.Length; i++)
			{
				simpleIntBins[i - 1] = new SimpleIntBin(fftResult.FftData[i], (i / 2).ToString(CultureInfo.CurrentCulture), false);
				if (iPeak < fftResult.Peaks.Length && fftResult.Peaks[iPeak] == i)
				{
					simpleIntBins[i - 1].IsHighlighted = true;
					iPeak++;
				}
			}

			Frequencies = new ObservableCollection<SimpleIntBin>(simpleIntBins);
		}
	}
}