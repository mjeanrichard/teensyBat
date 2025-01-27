﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Navigation;

using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Sqlite;

using TeensyBatMap.Common;
using TeensyBatMap.Database;
using TeensyBatMap.Domain;
using TeensyBatMap.ViewModels;

namespace TeensyBatMap.Views.Main
{
	public class MainPageModel : BaseViewModel
	{
		private readonly BatContext _db;
		private readonly ObservableCollection<BatNodeLog> _logFiles = new ObservableCollection<BatNodeLog>();
		private readonly BatNodeLogReader _logReader;
		private readonly NavigationService _navigationService;
		private BatNodeLog _selectedItem;

		public MainPageModel()
		{
			_logFiles.Add(DesignData.CreateBatLog());
			_logFiles.Add(DesignData.CreateBatLog());
			OnPropertyChanged(nameof(HasFiles));
		}

		public MainPageModel(NavigationEventArgs navigation, BatContext db, NavigationService navigationService, BatNodeLogReader logReader)
		{
			_db = db;
			_navigationService = navigationService;
			_logReader = logReader;

			ImportFileCommand = new RelayCommand(async () => await ImportLogFile());
			EditCommand = new RelayCommand(() =>
			{
				if (SelectedItem != null)
				{
					_navigationService.EditLog(SelectedItem);
				}
			}, () => SelectedItem != null);
			DetailsCommand = new RelayCommand(() =>
			{
				if (SelectedItem != null)
				{
					_navigationService.NavigateToLogDetails(SelectedItem);
				}
			}, () => SelectedItem != null);

			ManageDevicesCommand = new RelayCommand(() => _navigationService.NavigateToMangeDevices());
		}

		public RelayCommand ImportFileCommand { get; private set; }
		public RelayCommand DetailsCommand { get; }
		public RelayCommand EditCommand { get; }
		public RelayCommand ManageDevicesCommand { get; }

		public ObservableCollection<BatNodeLog> LogFiles
		{
			get { return _logFiles; }
		}

		public BatNodeLog SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				_selectedItem = value;
				DetailsCommand.RaiseCanExecuteChanged();
				EditCommand.RaiseCanExecuteChanged();
			}
		}

		public bool HasFiles
		{
			get { return _logFiles.Count > 0; }
		}

		public override string Titel
		{
			get { return "Log Files"; }
		}

		private async Task RefreshLogFiles()
		{
			List<BatNodeLog> batNodeLogs = await _db.Logs.ToListAsync();
			_logFiles.Clear();
			foreach (BatNodeLog log in batNodeLogs)
			{
				_logFiles.Add(log);
			}
			OnPropertyChanged(nameof(HasFiles));
		}

		public async Task ImportLogFile()
		{
			using (MarkBusy())
			{
				FileOpenPicker openPicker = new FileOpenPicker();
				openPicker.ViewMode = PickerViewMode.List;
				openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
				openPicker.FileTypeFilter.Add(".dat");
				IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
				if (files.Count > 0)
				{
					foreach (StorageFile file in files)
					{
						await AddFile(file);
					}
				}
				OnPropertyChanged(nameof(HasFiles));
			}
		}

		private async Task AddFile(StorageFile file)
		{
			BatNodeLog batNodeLog = await _logReader.Load(file);
			batNodeLog.Name = file.DisplayName;

			await _db.InsertLog(batNodeLog);

			_logFiles.Add(batNodeLog);
			OnPropertyChanged(nameof(HasFiles));
		}

		protected override async Task InitializeInternal()
		{
			using (MarkBusy())
			{
				await RefreshLogFiles();
			}
		}
	}
}