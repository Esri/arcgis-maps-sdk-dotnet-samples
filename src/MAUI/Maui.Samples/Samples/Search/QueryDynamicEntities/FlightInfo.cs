// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.RealTime;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArcGIS.Samples.QueryDynamicEntities
{
    public class FlightInfo : INotifyPropertyChanged
    {
        // The dynamic entity representing the flight.
        public DynamicEntity Entity { get; }

        // Backing fields for flight attributes.
        private string _flightNumber;
        private string _aircraft;
        private string _altitude;
        private string _speed;
        private string _heading;
        private string _status;
        private string _arrivalAirport;
        private bool _isExpanded;

        // Initializes flight info with entity and subscribes to change events.
        public FlightInfo(DynamicEntity entity)
        {
            Entity = entity;
            UpdateAttributes();
            Entity.DynamicEntityChanged += OnEntityChanged;
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ToggleButtonText));
                }
            }
        }

        // Text for the toggle button based on expansion state.
        public string ToggleButtonText => IsExpanded ? "Hide Details" : "Show Details";

        // Updates field value and raises property changed event if value differs.
        private bool SetField<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }

        public string FlightNumber { get => _flightNumber; private set => SetField(ref _flightNumber, value); }
        public string Aircraft { get => _aircraft; private set => SetField(ref _aircraft, value); }
        public string Altitude { get => _altitude; private set => SetField(ref _altitude, value); }
        public string Speed { get => _speed; private set => SetField(ref _speed, value); }
        public string Heading { get => _heading; private set => SetField(ref _heading, value); }
        public string Status { get => _status; private set => SetField(ref _status, value); }
        public string ArrivalAirport { get => _arrivalAirport; private set => SetField(ref _arrivalAirport, value); }

        // Retrieves attribute value from entity or returns default if not found.
        private string GetAttribute(string key, string defaultValue)
        {
            if (Entity?.Attributes != null && Entity.Attributes.TryGetValue(key, out var value))
                return value?.ToString() ?? defaultValue;
            return defaultValue;
        }

        // Formats numeric string to zero decimal places or returns original if not numeric.
        private string FormatNumber(string value)
        {
            if (double.TryParse(value, out double number))
                return number.ToString("F0");
            return value;
        }

        // Refreshes all flight attribute properties from the entity.
        private void UpdateAttributes()
        {
            FlightNumber = GetAttribute("flight_number", "N/A");
            Aircraft = GetAttribute("aircraft", "Unknown");
            Altitude = FormatNumber(GetAttribute("altitude_feet", "0"));
            Speed = FormatNumber(GetAttribute("speed", "0"));
            Heading = FormatNumber(GetAttribute("heading", "0"));
            Status = GetAttribute("status", "Unknown");
            ArrivalAirport = GetAttribute("arrival_airport", "N/A");
        }

        // Updates attributes when entity receives new observation data.
        private void OnEntityChanged(object sender, DynamicEntityChangedEventArgs e)
        {
            if (e.ReceivedObservation != null)
            {
                UpdateAttributes();
            }
        }

        // INotifyPropertyChanged implementation.
        public event PropertyChangedEventHandler PropertyChanged;

        // Raises property changed event for data binding updates.
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}