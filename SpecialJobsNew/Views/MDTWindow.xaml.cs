﻿using System;
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

namespace SpecialJobs.Views
{
    /// <summary>
    /// Interaction logic for MDTWindow.xaml
    /// </summary>
    public partial class MDTWindow : Window
    {
        public MDTWindow()
        {
            InitializeComponent();
        }
        public void Focus()
        {
            teName.Focus();
        }
    }
}
