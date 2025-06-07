using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.WebTests
{
    [Collection(nameof(CollectionTest))]
    public class ConfirmEmailTests : IAsyncDisposable
    {
        private readonly ConfigureWebApiTests _app;

        public ConfirmEmailTests(ConfigureWebApiTests app)
        {
            _app = app;
        }

        public async ValueTask DisposeAsync()
        {
            await _app.DeleteTables();   
        }

        [Fact]
        public async Task EmailAlreadyExists()
        {

        }
    }
}
