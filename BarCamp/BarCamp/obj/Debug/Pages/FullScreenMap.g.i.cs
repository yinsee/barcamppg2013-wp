﻿#pragma checksum "E:\Temporary storage for 8\Project Code\BarCamp\BarCamp_vs8_ fix web\BarCamp\BarCamp\Pages\FullScreenMap.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "2E6040D12617668D3AD0ADA8ACC47D62"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18051
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace BarCamp {
    
    
    public partial class FullScreenMap : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Maps.Controls.Map map_Fullscreen;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton appbarbtn_indoorMap;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/BarCamp;component/Pages/FullScreenMap.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.map_Fullscreen = ((Microsoft.Phone.Maps.Controls.Map)(this.FindName("map_Fullscreen")));
            this.appbarbtn_indoorMap = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbarbtn_indoorMap")));
        }
    }
}

