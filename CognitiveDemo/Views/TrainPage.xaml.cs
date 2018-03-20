using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CognitiveDemo.Views
{
    using CognitiveDemo.AzureContracts;
    using CognitiveDemo.ViewModels;

    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TrainPage : ContentPage
    {
        private TrainPageViewModel viewModel;

        public TrainPage()
        {
            InitializeComponent();

            this.viewModel = new TrainPageViewModel();
            this.BindingContext = this.viewModel;

            this.Appearing += async (sender, args) =>
                {
                    if (this.viewModel.Persons.Count == 0)
                    {
                        await this.viewModel.LoadPersonsAsync();
                    }
                };
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as Person;
            if (item == null)
                return;

            await Navigation.PushAsync(new TrainDetailPage(new TrainDetailPageViewModel(item)));

            // Manually deselect item.
            this.PersonsListView.SelectedItem = null;
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TrainDetailPage(new TrainDetailPageViewModel(new Person())));
        }
    }
}