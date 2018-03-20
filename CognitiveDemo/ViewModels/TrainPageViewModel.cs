using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveDemo.ViewModels
{
    using System.Collections.ObjectModel;

    using CognitiveDemo.AzureContracts;
    using CognitiveDemo.Services;

    using Xamarin.Forms;

    public class TrainPageViewModel : BaseViewModel
    {
        private IFaceService faceService;

        public ObservableCollection<Person> Persons { get; set; }

        public Command LoadPersonsCommand { get; set; }

        public TrainPageViewModel()
        {
            this.Title = "Face Training";

            this.Persons = new ObservableCollection<Person>();

            //this.faceService = DependencyService.Get<IFaceService>();
            this.faceService = new MockFaceService();

            LoadPersonsCommand = new Command(async () => await LoadPersonsAsync());
        }

        public async Task LoadPersonsAsync()
        {
            try
            {
                this.IsBusy = true;

                this.Persons.Clear();

                var persons = await this.faceService.GetKnownPersons(false);

                foreach (var person in persons)
                {
                    this.Persons.Add(person);
                }
            }
            finally
            {
                this.IsBusy = false;
            }
        }
    }
}
