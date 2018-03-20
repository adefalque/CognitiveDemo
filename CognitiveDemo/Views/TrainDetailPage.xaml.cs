using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CognitiveDemo.Views
{
    using CognitiveDemo.AzureContracts;
    using CognitiveDemo.ViewModels;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TrainDetailPage : ContentPage
    {
        private TrainDetailPageViewModel viewModel;

        public TrainDetailPage() : this(new TrainDetailPageViewModel(new Person() { Name = "John Does" }))
        {
        }

        public TrainDetailPage(TrainDetailPageViewModel viewModel)
        {
            InitializeComponent();

            this.BindingContext = this.viewModel = viewModel;
        }

        private void Save_Clicked(object sender, EventArgs e)
        {
            
        }

        private async void OnTapGestureRecognizerCameraTapped(object sender, EventArgs e)
        {
            await this.viewModel.TakePhoto();
        }
    }
}