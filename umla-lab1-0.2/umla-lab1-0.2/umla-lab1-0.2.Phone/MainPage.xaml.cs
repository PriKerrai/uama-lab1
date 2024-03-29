﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace uama_lab1
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void logIn_Click(object sender, RoutedEventArgs e)
        {
            // Verify user and if true navigate to UserPage...
            NavigationService.Navigate(new Uri("/UserPage.xaml", UriKind.Relative));
        }
    }
}