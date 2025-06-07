using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.WebTests
{
    [CollectionDefinition(nameof(CollectionTest))]
    public class CollectionTest : ICollectionFixture<ConfigureWebApiTests>
    {
    }
}
