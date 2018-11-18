#pragma warning disable IDE1006

using System.Collections.Generic;
using System.Data.SqlClient;
using Xunit;

namespace tsqlsh.unittests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class CmdLineParserTests
    {
        private string[] createArgs(string svr, string db, string user, string pwd, bool isRW)
        {
            var args = new List<string>();

            if (!string.IsNullOrWhiteSpace(svr)) args.Add($"server={svr}");
            if (!string.IsNullOrWhiteSpace(db)) args.Add($"database={db}");
            if (!string.IsNullOrWhiteSpace(user)) args.Add($"user={user}");
            if (!string.IsNullOrWhiteSpace(pwd)) args.Add($"pwd={pwd}");
            if (isRW) args.Add("-rw");

            return args.ToArray();
        }

        [Fact]
        public void Empty_args_should_set_ShowHelp_to_true()
        {
            // arrange
            var args = new string[] { };

            // act
            var cla = new CmdLineArgs(args);

            // assert
            Assert.True(cla.ShowHelp);
        }

        [Fact]
        public void Short_help_flag_should_set_ShowHelp_to_true()
        {
            // arrange
            var args = new string[] { "-h" };

            // act
            var cla = new CmdLineArgs(args);

            // assert
            Assert.True(cla.ShowHelp);
        }

        [Fact]
        public void Full_help_flag_should_set_ShowHelp_to_true()
        {
            // arrange
            var args = new string[] { "--help" };

            // act
            var cla = new CmdLineArgs(args);

            // assert
            Assert.True(cla.ShowHelp);
        }

        [Fact]
        public void Missing_rw_flag_set_appIntent_to_ReadOnly()
        {
            // arrange
            var args = this.createArgs("test_svr", "test_db", "test_usr", "test_pwd", false);

            // act
            var cla = new CmdLineArgs(args);

            // assert
            Assert.False(cla.HasErrors);
            Assert.Equal<ApplicationIntent>(ApplicationIntent.ReadOnly, cla.Intent);
        }

        [Fact]
        public void Contains_rw_flag_set_appIntent_to_ReadWrite()
        {
            // arrange
            var args = this.createArgs("test_svr", "test_db", "test_usr", "test_pwd", true);

            // act
            var cla = new CmdLineArgs(args);

            // assert
            Assert.False(cla.HasErrors);
            Assert.Equal<ApplicationIntent>(ApplicationIntent.ReadWrite, cla.Intent);
        }

        [Fact]
        public void No_user_or_pwd_sets_integrated_security_to_true()
        {
            // arrange
            var args = this.createArgs("test_svr", "test_db", null, null, false);

            // act
            var cla = new CmdLineArgs(args);

            // act
            Assert.False(cla.HasErrors);
            Assert.True(cla.UseIntegratedSecurity);
        }

        [Fact]
        public void Missing_server_flag_creates_error()
        {
            // arrange
            var args = this.createArgs("", "test_db", null, null, false);

            // act
            var cla = new CmdLineArgs(args);

            // act
            Assert.True(cla.HasErrors);

            Assert.Equal("missing server name", cla.Errors[0]);
        }

        [Fact]
        public void Missing_db_flag_defaults_db_to_master()
        {
            // arrange
            var args = this.createArgs("test_server", "", null, null, false);

            // act
            var cla = new CmdLineArgs(args);

            // act
            Assert.False(cla.HasErrors);

            Assert.Equal("master", cla.Database);
        }

        [Fact]
        public void Supplying_user_flag_no_pwd_flag_create_error()
        {
            // arrange
            var args = this.createArgs("test_server", "test_db", "test_user", null, false);

            // act
            var cla = new CmdLineArgs(args);

            // act
            Assert.True(cla.HasErrors);

            Assert.Equal("a password is required if supplying a user name", cla.Errors[0]);
        }

        [Fact]
        public void Supplying_pwd_flag_no_user_flag_create_error()
        {
            // arrange
            var args = this.createArgs("test_server", "test_db", "", "test_pwd", false);

            // act
            var cla = new CmdLineArgs(args);

            // act
            Assert.True(cla.HasErrors);

            Assert.Equal("a user name is required if supplying a password", cla.Errors[0]);
        }

        [Fact]
        public void ConnectionBuilder_is_valid()
        {
            // arrange
            var args = this.createArgs("test_svr", "test_db", "test_usr", "test_pwd", true);

            // act
            var cla = new CmdLineArgs(args);

            // assert
            Assert.False(cla.HasErrors);
            Assert.False(cla.UseIntegratedSecurity);
            Assert.Equal("test_svr", cla.Server);
            Assert.Equal("test_db", cla.Database);
            Assert.Equal("test_usr", cla.User);
            Assert.Equal("test_pwd", cla.Password);
            Assert.Equal<ApplicationIntent>(ApplicationIntent.ReadWrite, cla.Intent);

            Assert.NotNull(cla.ConnectionBuilder);
            Assert.Equal(cla.Server, cla.ConnectionBuilder.DataSource);
            Assert.Equal(cla.Database, cla.ConnectionBuilder.InitialCatalog);
            Assert.Equal(cla.User, cla.ConnectionBuilder.UserID);
            Assert.Equal(cla.Password, cla.ConnectionBuilder.Password);
            Assert.Equal(cla.Intent, cla.ConnectionBuilder.ApplicationIntent);
        }
    }
}
