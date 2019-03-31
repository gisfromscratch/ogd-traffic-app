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

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Mapping;

namespace OpenGov.Traffic.WinApp
{
    /// <summary>
    /// Verwaltet die Kartendaten für diese Anwendung.
    /// </summary>
    public class MapViewModel : INotifyPropertyChanged
    {
        private Map _map;

        /// <summary>
        /// Erzeugt eine neue Instanz und setzt die Hintergrundkarte.
        /// </summary>
        public MapViewModel()
        {
            _map = new Map(Basemap.CreateStreets());
            _map.Loaded += MapLoaded;
        }

        /// <summary>
        /// Die Karte mit allen Ebenen dieser Anwendung.
        /// </summary>
        public Map Map
        {
            get { return _map; }
            set { _map = value; OnPropertyChanged(); }
        }

        private void MapLoaded(object sender, EventArgs evt)
        {
            
        }

        /// <summary>
        /// Erzeugt das <see cref="MapViewModel.PropertyChanged" /> Ereignis.
        /// </summary>
        /// <param name="propertyName">Name der Eigenschaft, welche verändert wurde</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var propertyChangedHandler = PropertyChanged;
            if (propertyChangedHandler != null)
                propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
