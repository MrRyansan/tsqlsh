using System;
using Xunit;
using tsqlsh;

namespace tsqlsh.unittests
{
    public class CmdLineParserTests
    {
        [Fact]
        public void Ctor_empty_args_should_set_ShowHelp_to_true()
        {
            // arrange
            var args = new string[] { };

            // act
            var cla = new CmdLineArgs(args);

            // assert
            Assert.True(cla.ShowHelp);
        }
    }
}
