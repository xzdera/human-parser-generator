// unit tests for Generator Model
// author: Christophe VG <contact@christophe.vg>

using System;
using System.IO;
using System.Collections.Generic;

using NUnit.Framework;

[TestFixture]
public class GeneratorModelTests {

  Parser parser;

  [SetUp]
  public void SetUp() {
    this.parser = new Parser();
  }

  [TearDown]
  public void TearDown() {
    this.parser = null;
  }

  private Copybook parse(string src) {
    return this.parser.Parse(src).AST;
  }

  private Copybook testSingleSentence(string src, string expected) {
    Copybook book = this.parse(src);
    Assert.AreEqual(1, book.Records.Count);
    Assert.AreEqual(
      expected.Replace("\n", "").Replace(" ", ""),
      book.Records[0].ToString()
    );
    return book;
  }

  [Test]
  public void BasicRecordWithoutOptions() {
    Copybook book = this.testSingleSentence(
      "01 TOP.",
      "BasicRecord(Level=Int(Value=01),LevelName=Identifier(Name=TOP),Options=[])"
    );
    Assert.AreEqual(((BasicRecord)book.Records[0]).Level.Value, "01");
  }

  [Test]
  public void BasicRecordWithPicWithCompOption() {
    this.testSingleSentence(
      "10 FIELD   PIC S9(05) COMP-5.",
      @"BasicRecord(
         Level=Int(
              Value=10
              ),
         LevelName=Identifier(
                  Name=FIELD
                  ),
         Options=[
                PictureFormat(
                HasTure=False,
                HasIs=False,
                MainType=S9,
                MainIndex=Int(
                          Value=05
                          ),
                HasAny=False,
                SubType=,
                SubIndex=,
                HasPictureTypeInt=False,
                HasInt=True
                ),
                UsageOption(
                HasIs=False,
                HasAll=False,
                Usage=CompUsage(
                      CompLevel=5,
                      HasDigit=True
                      )
                )]
         )"
    );
  }

}