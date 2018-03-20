using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveDemo.ViewModels
{
    using CognitiveDemo.AzureContracts;

    using Plugin.Media;
    using Plugin.Media.Abstractions;
    using Plugin.Permissions;
    using Plugin.Permissions.Abstractions;

    using Xamarin.Forms;

    public class TrainDetailPageViewModel : BaseViewModel
    {
        private string name;

        private ImageSource currentImageSource;

        private bool isCreation;

        private bool isRunning;

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        public ImageSource CurrentImageSource
        {
            get => this.currentImageSource;
            set => this.SetProperty(ref this.currentImageSource, value);
        }

        public bool IsCreation
        {
            get => this.isCreation;
            set => this.SetProperty(ref this.isCreation, value);
        }

        public bool IsRunning
        {
            get => this.isRunning;
            set => this.SetProperty(ref this.isRunning, value);
        }

        public TrainDetailPageViewModel(Person person)
        {
            this.Name = person.Name;

            if (person.PersonId == Guid.Empty)
            {
                this.IsCreation = true;
            }

            this.CurrentImageSource = ImageSource.FromFile("empty.png");
        }

        public async Task TakePhoto()
        {
            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

            if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] {Permission.Camera, Permission.Storage});
                cameraStatus = results[Permission.Camera];
                storageStatus = results[Permission.Storage];
            }

            if (cameraStatus == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted)
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("No Camera", ":( No camera available.", "OK");
                    return;
                }

                try
                {
                    this.IsRunning = true;

                    var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                                                                           {
                                                                               Directory = "Images",
                                                                               Name = "FacePerson.jpg"
                                                                           });

                    if (file == null)
                    {
                        await DisplayAlert("Error", ":Unable to save the photo", "OK");
                        return;
                    }

                    this.CurrentImageSource = ImageSource.FromStream(
                        () =>
                            {
                                var stream = file.GetStream();
                                file.Dispose();
                                return stream;
                            });

                }
                finally
                {
                    this.IsRunning = false;
                }
            }
            else
            {
                await DisplayAlert("Permissions Denied", "Unable to take photos.", "OK");
                //On iOS you may want to send your user to the settings screen.
                //CrossPermissions.Current.OpenAppSettings();
            }
        }
    }
}
