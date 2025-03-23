using Soenneker.Hangfire.Util.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Hangfire.Util.Tests;

[Collection("Collection")]
public class HangfireUtilTests : FixturedUnitTest
{
    private readonly IHangfireUtil _util;

    public HangfireUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IHangfireUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
