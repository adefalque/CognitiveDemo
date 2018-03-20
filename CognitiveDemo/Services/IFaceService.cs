namespace CognitiveDemo
{
    using System.Threading.Tasks;

    using CognitiveDemo.AzureContracts;

    public interface IFaceService
    {
        Task<Person[]> GetKnownPersons(bool useCache = true);
    }
}