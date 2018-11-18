// File: SqlbuilderTests.cs
// Author: TwistingMercury
// Email: twistingmercury@outlook.com
// Copyright: Copyrighte © Jeremy K. Johnosn
// Date Created: 11/18/2018
using System;
using Xunit;

namespace tsqlsh.unittests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class SqlbuilderTests
    {
        [Fact]
        public void Exit_command_returns_exit()
        {
            // arrange
            var sio = new MockStdIo();
            sio.SetInput("EXIT");
            var sqlb = new SqlBuilder() { stdio = sio };
            var expected = "exit";

            // act 
            var actual = sqlb.Build();

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Clear_command_returns_clear()
        {
            // arrange
            var sio = new MockStdIo();
            sio.SetInput("Clear");
            var sqlb = new SqlBuilder() { stdio = sio };
            var expected = "clear";

            // act 
            var actual = sqlb.Build();

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Single_Line_stmt_returns(){
            // arrange
            var sio = new MockStdIo();
            var expected = "SELECT foo FROM bar";
            sio.SetInput("SELECT foo FROM bar go");
            var sqlb = new SqlBuilder() { stdio = sio };

            // act 
            var actual = sqlb.Build();

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Go_on_separate_Line_stmt_returns()
        {
            // arrange
            var sio = new MockStdIo();
            var expected = "SELECT foo FROM bar";
            sio.SetInput("SELECT foo FROM bar", "go");
            var sqlb = new SqlBuilder() { stdio = sio };

            // act 
            var actual = sqlb.Build();

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Multiline_Line_stmt_returns()
        {
            // arrange
            var sio = new MockStdIo();
            var expected = "SELECT foo FROM bar";
            sio.SetInput("SELECT foo", "FROM bar", "go");
            var sqlb = new SqlBuilder() { stdio = sio };

            // act 
            var actual = sqlb.Build();

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Complex_Multiline_stmt_returns(){
            // arrange
            var sio = new MockStdIo();
            var expected = "USE myDatabase; " 
                + "INSERT INTO FOO(SOME_VALUE) VALUES('THIS VALUE'); "
                + "SELECT * FROM FOO WHERE SOME_VALUE='THIS_VALUE';";

            sio.SetInput(
                "USE myDatabase;",
                "INSERT INTO FOO(SOME_VALUE)",
                "VALUES('THIS VALUE');",
                "SELECT * FROM FOO",
                "WHERE SOME_VALUE='THIS_VALUE';"
                ,"go");
            var sqlb = new SqlBuilder() { stdio = sio };

            // act 
            var actual = sqlb.Build();

            // assert
            Assert.Equal(expected, actual);
        }
    }
}
