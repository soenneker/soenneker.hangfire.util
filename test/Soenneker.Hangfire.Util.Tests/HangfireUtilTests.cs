using AwesomeAssertions;
using Soenneker.Hangfire.Util.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Hangfire.Util.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class HangfireUtilTests : HostedUnitTest
{
    private readonly IHangfireUtil _util;

    public HangfireUtilTests(Host host) : base(host)
    {
        _util = Resolve<IHangfireUtil>(true);
    }

    [Test]
    public void Default()
    {
        _util.Should().NotBeNull();
    }
}
