﻿// 
// Teensy Bat Explorer - Copyright(C) 2020 Meinrad Jean-Richard
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Threading.Tasks;

using TeensyBatExplorer.WPF.Views.AddLogs;
using TeensyBatExplorer.WPF.Views.Device;
using TeensyBatExplorer.WPF.Views.Project;
using TeensyBatExplorer.WPF.Views.Start;

using Unity;

namespace TeensyBatExplorer.WPF.Infrastructure
{
    public class NavigationService
    {
        private readonly IUnityContainer _container;

        public NavigationService(IUnityContainer container)
        {
            _container = container;
        }

        public BaseViewModel CurrentViewModel { get; private set; }

        public async Task NavigateToProjectPage()
        {
            await Navigate<ProjectPageViewModel>();
        }

        private async Task<T> Navigate<T>() where T : BaseViewModel
        {
            T currentViewModel = _container.Resolve<T>();
            await currentViewModel.Initialize();
            CurrentViewModel = currentViewModel;
            OnViewModelChanged?.Invoke(this, new EventArgs());
            await Task.Run(() => currentViewModel.Load());
            return currentViewModel;
        }

        public event EventHandler<EventArgs> OnViewModelChanged;

        public async Task NavigateToStartPage()
        {
            await Navigate<StartPageViewModel>();
        }

        public async Task NavigateToAddLogsPage()
        {
            await Navigate<AddLogsViewModel>();
        }

        public async Task NavigateToDevicePage()
        {
            await Navigate<DeviceViewModel>();
        }
    }
}