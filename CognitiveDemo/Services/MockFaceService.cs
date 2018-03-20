using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveDemo.Services
{
    using CognitiveDemo.AzureContracts;

    public class MockFaceService : IFaceService
    {
        public Task<Person[]> GetKnownPersons(bool useCache = true)
        {
            return Task.FromResult(
                new []
                {
                    new Person() { Name = "Alexandre Defalque" }, 
                    new Person() { Name = "Emma Defalque" }, 
                    new Person() { Name = "André Paulos" }, 
                    new Person() { Name = "Adrien Rouchy" }, 
                    new Person() { Name = "Benjamin Whyman" }, 
                });
        }
    }
}
