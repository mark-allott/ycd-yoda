using FluentAssertions;
using YodaAssembler.Processor;

namespace YodaAssembler.Tests.Processor;

[TestClass]
public partial class YodaTests
{
	[TestMethod]
	[DynamicData(nameof(ValidateConstructorTestData), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(DynamicDataTestDisplayNameProvider))]
	public void ValidateConstructor(ConstructorTest test)
	{
		if (test.ShouldFail)
		{
			//	ArgumentException.ThrowIfNullOrWhiteSpace throws:
			//		ArgumentNullException if Path is null
			//		otherwise ArgumentException
			if (test.Path is not null && string.IsNullOrWhiteSpace(test.Path))
				Assert.ThrowsException<ArgumentException>(() =>
					new Yoda(test.Path!, test.Tokeniser, test.Generator));
			else
				Assert.ThrowsException<ArgumentNullException>(() =>
					new Yoda(test.Path!, test.Tokeniser, test.Generator));
			return;
		}

		Yoda sut = new Yoda(test.Path!, test.Tokeniser, test.Generator);
		sut.Should().NotBeNull();
		sut.Lines.Should().NotBeNull();
		sut.Lines.Should().BeEmpty();
		sut.BootFileData.Should().NotBeNull();
		sut.BootFileData.Count.Should().Be(256);
	}

	[TestMethod]
	[DynamicData(nameof(ParseTestDataProvider), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(DynamicDataTestDisplayNameProvider))]
	public void ValidateParseOfEnumerableStrings(ParseTestData test)
	{
		if (test.ShouldFail)
		{
			if (test.ExpectedExceptionType is null)
				Assert.Fail("Cannot check exception type");
			try
			{
				new Yoda(".").Parse(test.Lines);
				Assert.Fail($"Should have thrown exception of type {test.ExpectedExceptionType.Name}");
			}
			catch (Exception e)
			{
				if (e.GetType() != test.ExpectedExceptionType)
					Assert.Fail($"Expected {test.ExpectedExceptionType.Name}, but got {e.GetType().Name}");
			}
			return;
		}

		Yoda sut = new Yoda(".");
		sut.Should().NotBeNull();
		
		//	Parse the test data
		var tokens = sut.Parse(test.Lines).ToList();
		sut.Lines.Should().NotBeNullOrEmpty();
		sut.Lines.Count.Should().Be(test.Lines.Count);
		
		//	Check the tokenising worked as expected
		tokens.Should().NotBeNullOrEmpty();
		tokens.Count.Should().Be(test.ExpectedTokenCount);
		var tokenTypes = tokens.Select(s => s.TokenType).ToList();
		tokenTypes.Should().NotBeNullOrEmpty();
		tokenTypes.Should().ContainInConsecutiveOrder(test.ExpectedTokenTypes);
	}

	[TestMethod]
	[DynamicData(nameof(CompileTestDataProvider), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(DynamicDataTestDisplayNameProvider))]
	public void ValidateCompileOfEnumerableStrings(CompileTestData test)
	{
		if (test.ShouldFail)
		{
			if (test.ExpectedExceptionType is null)
				Assert.Fail("Cannot check exception type");
			try
			{
				new Yoda(".").Compile(test.Lines);
				Assert.Fail($"Should have thrown exception of type {test.ExpectedExceptionType.Name}");
			}
			catch (Exception e)
			{
				if (e.GetType() != test.ExpectedExceptionType)
					Assert.Fail($"Expected {test.ExpectedExceptionType.Name}, but got {e.GetType().Name}");
			}
			return;
		}

		Yoda sut = new Yoda(".");
		sut.Should().NotBeNull();
		
		//	Compile the source in test data
		sut.Compile(test.Lines);
		sut.Lines.Should().NotBeNullOrEmpty();
		sut.Lines.Count.Should().Be(test.Lines.Count);
		sut.BootFileData.Should().NotBeNull();
		sut.BootFileData.Count.Should().Be(256);
		sut.BootFileData.Should().ContainInConsecutiveOrder(test.ExpectedBytes);
	}

	[TestMethod]
	[DynamicData(nameof(CompileToByteCodeTestProvider), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(DynamicDataTestDisplayNameProvider))]
	public void ValidateCompileToByteCode(CompileToByteCodeTestData test)
	{
		Yoda sut = test.Processor;
		sut.Should().NotBeNull();

		if (test.ShouldFail)
		{
			if (test.ExpectedExceptionType is null)
				Assert.Fail("Cannot check exception type");
			try
			{
				sut.CompileToByteCode(test.Tokens);
				Assert.Fail($"Should have thrown exception of type {test.ExpectedExceptionType.Name}");
			}
			catch (Exception e)
			{
				if (e.GetType() != test.ExpectedExceptionType)
					Assert.Fail($"Expected {test.ExpectedExceptionType.Name}, but got {e.GetType().Name}");
			}
			return;
		}
		
		//	Compile the source in test data
		var result = sut.CompileToByteCode(test.Tokens)
			.ToList();
		result.Should().NotBeNull();
		if (test.ByteCodeTokens is null)
		{
			result.Should().BeEmpty();
			return;
		}
		result.Count.Should().Be(test.ByteCodeTokens.Count);

		for (int i = 0; i < test.ByteCodeTokens.Count; i++)
		{
			var expectedByteCode = test.ByteCodeTokens[i];
			var actualByteCode = result[i];

			actualByteCode.LineNumber.Should().Be(expectedByteCode.LineNumber);
			actualByteCode.DirectiveType.Should().Be(expectedByteCode.DirectiveType);
			actualByteCode.MemoryLocation.Should().Be(expectedByteCode.MemoryLocation);
			actualByteCode.NextLocation.Should().Be(expectedByteCode.NextLocation);
			actualByteCode.Bytes.Should().NotBeNullOrEmpty();
			actualByteCode.Bytes.Should().ContainInConsecutiveOrder(expectedByteCode.Bytes);
		}
	}

	[TestMethod]
	[DynamicData(nameof(CompilePass2TestProvider), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(DynamicDataTestDisplayNameProvider))]
	public void ValidateCompilePass2(SourceCodeDataTest test)
	{
		Yoda sut = new  Yoda(".");
		sut.Should().NotBeNull();
		
		if (test.ShouldFail)
		{
			if (test.ExpectedExceptionType is null)
				Assert.Fail("Cannot check exception type");
			try
			{
				sut.Compile(test.Lines);
				Assert.Fail($"Should have thrown exception of type {test.ExpectedExceptionType.Name}");
			}
			catch (Exception e)
			{
				if (e.GetType() != test.ExpectedExceptionType)
					Assert.Fail($"Expected {test.ExpectedExceptionType.Name}, but got {e.GetType().Name}");
			}
			return;
		}

		//	Compile the source in test data
		sut.Compile(test.Lines);
	}

	[TestMethod]
	[DynamicData(nameof(CompilePass3TestProvider), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(DynamicDataTestDisplayNameProvider))]
	public void ValidateCompilePass3(CompileTestData test)
	{
		Yoda sut = new  Yoda(".");
		sut.Should().NotBeNull();
		
		if (test.ShouldFail)
		{
			if (test.ExpectedExceptionType is null)
				Assert.Fail("Cannot check exception type");
			try
			{
				sut.Compile(test.Lines);
				Assert.Fail($"Should have thrown exception of type {test.ExpectedExceptionType.Name}");
			}
			catch (Exception e)
			{
				if (e.GetType() != test.ExpectedExceptionType)
					Assert.Fail($"Expected {test.ExpectedExceptionType.Name}, but got {e.GetType().Name}");
			}
			return;
		}

		//	Compile the source in test data
		sut.Compile(test.Lines);
		sut.BootFileData.Should().NotBeNull();
		sut.BootFileData.Count.Should().Be(256);
		sut.BootFileData.Should().ContainInConsecutiveOrder(test.ExpectedBytes);
	}

	[TestMethod]
	[DynamicData(nameof(CompilePass4TestProvider), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(DynamicDataTestDisplayNameProvider))]
	public void ValidateCompilePass4(CompileTestData test)
	{
		Yoda sut = new  Yoda(".");
		sut.Should().NotBeNull();
		
		if (test.ShouldFail)
		{
			if (test.ExpectedExceptionType is null)
				Assert.Fail("Cannot check exception type");
			try
			{
				sut.Compile(test.Lines);
				Assert.Fail($"Should have thrown exception of type {test.ExpectedExceptionType.Name}");
			}
			catch (Exception e)
			{
				if (e.GetType() != test.ExpectedExceptionType)
					Assert.Fail($"Expected {test.ExpectedExceptionType.Name}, but got {e.GetType().Name}");
			}
			return;
		}

		//	Compile the source in test data
		sut.Compile(test.Lines);
		sut.BootFileData.Should().NotBeNull();
		sut.BootFileData.Count.Should().Be(256);
		sut.BootFileData.Should().ContainInConsecutiveOrder(test.ExpectedBytes);
	}
}