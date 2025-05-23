﻿using System.IO;
using System.Windows;
using System.Windows.Input;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.MdStyles;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit
{
    public class TipsModel : BaseNotify
    {
        private string _markdown;
        private Style _style;
        private ICommand _escape;

        public string Markdown
        {
            get => _markdown;
            set => SetField(ref _markdown, value);
        }

        public Style Style
        {
            get => _style;
            set => SetField(ref _style, value);
        }

        public ICommand Escape
        {
            get => _escape;
            set => SetField(ref _escape, value);
        }
    }


    /// <summary>
    /// Interaction logic for Tips.xaml
    /// </summary>
    public partial class TipsWindow : Window
    {

        public TipsWindow()
        {
            InitializeComponent();
            var tips = new TipsModel
            {
                Markdown = File.ReadAllText("Tips.md"),
                Style = CustomStyles.BetterGithub,
                Escape = new RelayCommand<object>(o => Close())
            };

            //Markdown engine = new Markdown();
            //engine.DocumentStyle = CustomStyles.BetterGithub;
            //FlowDocument document = engine.Transform(markdown);
            //RichTextBox.Document = document;
            DataContext = tips;
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "https://github.com/RupertAvery/DiffusionToolkit/blob/master/Diffusion.Toolkit/Tips.md");
        }
    }
}
