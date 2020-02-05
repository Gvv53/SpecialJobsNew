﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for OrganizationControl.xaml
    /// </summary>
    public partial class EqTypeWindow : Window  
    {
        public EqTypeWindow()
        {
            InitializeComponent();
        }
        public void Cancel()
        {
            this.Close();            
        }
        public void Focus()
        {
            teName.Focus();
        }
    }
}
