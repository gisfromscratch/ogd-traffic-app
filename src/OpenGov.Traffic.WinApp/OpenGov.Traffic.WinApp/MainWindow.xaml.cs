using System;
using System.ComponentModel;
/*
 * Copyright 2019 Jan Tschada
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace OpenGov.Traffic.WinApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // Nicht schön aber wenigstens verwaltet das ViewModel die Ausdehnung
            var mapViewModel = (MapViewModel) Resources[@"MapViewModel"];
            mapViewModel.AreaOfInterestChanged += AreaOfInterestChanged;
        }

        private void AreaOfInterestChanged(object sender, EventArgs e)
        {
            var mapViewModel = sender as MapViewModel;
            if (null == mapViewModel)
            {
                return;
            }

            if (mapViewModel.AreaOfInterest.IsEmpty)
            {
                return;
            }

            MapView.SetViewpointGeometryAsync(mapViewModel.AreaOfInterest);
        }
    }
}
