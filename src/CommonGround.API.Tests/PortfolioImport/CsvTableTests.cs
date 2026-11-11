using CommonGround.API.PortfolioImport;
using FluentAssertions;

namespace CommonGround.API.Tests.PortfolioImport;

public class CsvTableTests
{
    public class TheParseMethod
    {
        [Fact]
        public void GivenQuotedFieldsWithCommasQuotesAndLineBreaks_WhenParsed_ThenReadsEachFieldWhole()
        {
            var text = "Name,Note\r\n\"Lee, Ana\",\"said \"\"hi\"\"\nthen left\"\r\n";

            var table = CsvTable.Parse(text);

            var row = table.Rows.Should().ContainSingle().Subject;
            table.Value(row, "Name").Should().Be("Lee, Ana");
            table.Value(row, "Note").Should().Be("said \"hi\"\nthen left");
        }

        [Fact]
        public void GivenBlankLinesAndAMultiLineField_WhenParsed_ThenRowNumbersMatchTheSpreadsheet()
        {
            var text = "A,B\n1,x\n\n2,\"two\nlines\"\n3,y\n";

            var table = CsvTable.Parse(text);

            table.Rows.Select(r => r.Line).Should().Equal(2, 4, 6);
        }

        [Fact]
        public void GivenAByteOrderMarkAndOtherCase_WhenParsed_ThenFindsTheColumn()
        {
            var table = CsvTable.Parse("﻿Parcel Number\nAPN-1\n");

            table.HasColumn("parcel number").Should().BeTrue();
            table.Value(table.Rows[0], "PARCEL NUMBER").Should().Be("APN-1");
        }

        [Fact]
        public void GivenARowShorterThanTheHeader_WhenAMissingCellIsRead_ThenItIsEmpty()
        {
            var table = CsvTable.Parse("A,B,C\n1\n");

            table.Value(table.Rows[0], "C").Should().BeEmpty();
        }
    }
}
