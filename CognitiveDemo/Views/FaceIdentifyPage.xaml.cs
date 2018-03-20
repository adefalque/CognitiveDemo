using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CognitiveDemo.Views
{
    using System.Diagnostics.CodeAnalysis;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FaceIdentifyPage : ContentPage
    {
        public FaceIdentifyPage()
        {
            InitializeComponent();
        }

        private void BtLaunch_OnClicked(object sender, EventArgs e)
        {
            DependencyService.Get<INativePages>().StartNativeFaceIdentifyPage();
        }
    }
}