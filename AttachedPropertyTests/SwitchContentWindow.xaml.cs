// -----------------------------------------------------------------------
// <copyright file="SwitchContentWindow.xaml.cs" company="Anori Soft">
// Copyright (c) Anori Soft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AttachedPropertyTests
{
    /// <summary>
    ///     Interaction logic for SwitchContentWindow.xaml
    /// </summary>
    public partial class SwitchContentWindow : Window
    {
        private readonly FrameworkElement control;

        public SwitchContentWindow()
        {
            this.InitializeComponent();
            this.control = new AttachedUserControl();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            this.Control2.Content = null;
            this.Control1.Content = this.control;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            this.Control1.Content = null;
            this.Control2.Content = this.control;
        }
    }
}