﻿#region license

// Copyright 2012 Amanda Koh. All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
//    1. Redistributions of source code must retain the above copyright notice,
//       this list of conditions and the following disclaimer.
//
//    2. Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY AMANDA KOH ``AS IS'' AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
// EVENT SHALL AMANDA KOH OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// The views and conclusions contained in the software and documentation are
// those of the authors and should not be interpreted as representing official
// policies, either expressed or implied, of Amanda Koh.

#endregion

using System;
using Xunit;

namespace Naucera.Iambic
{
	public class ParserCompilerTest
	{
		[Fact]
		public void ShouldBeAbleToParseOwnPegGrammar()
		{
			var pegGrammar = ParserCompiler.BuildPegGrammarParser().ToString();
			var p = ParserCompiler.Compile(pegGrammar);

			Assert.Equal(pegGrammar, p.ToString());
		}


		[Fact]
		public void ShouldBuildPegGrammarParserWithPegGrammar()
		{
			var newLine = Environment.NewLine;

			var grammar =
				@"Grammar := (Ignorable? Definition+ EndOfInput)" + newLine +
				@"Definition := (Identifier ASSIGN Expression)" + newLine +
				@"Expression := (OrderedChoice || Sequence)" + newLine +
				@"OrderedChoice := (Sequence (OR Sequence)+)" + newLine +
				@"Sequence := Prefix+" + newLine +
				@"Prefix := ((AND || NOT)? Suffix)" + newLine +
				@"Suffix := (Primary (QUESTION || STAR || PLUS)?)" + newLine +
				@"Primary := ((Identifier !ASSIGN) || (OPEN Expression CLOSE) || Literal)" + newLine +
				@"Identifier := (/\w+/ Ignorable?)" + newLine +
				@"Literal := (BasicLiteral || RegexLiteral || CustomMatcher)" + newLine +
				@"BasicLiteral := (/'(\\\\|\\'|[^'])*'/ Ignorable?)" + newLine +
				@"RegexLiteral := (/\/(\\\\|\\\/|[^\/])*\// Ignorable?)" + newLine +
				@"CustomMatcher := (/\{\w+\}/ Ignorable?)" + newLine +
				@"EndOfInput := /$/" + newLine +
				@"ASSIGN := (':=' Ignorable?)" + newLine +
				@"OR := ('||' Ignorable?)" + newLine +
				@"AND := ('&' Ignorable?)" + newLine +
				@"NOT := ('!' Ignorable?)" + newLine +
				@"QUESTION := ('?' Ignorable?)" + newLine +
				@"STAR := ('*' Ignorable?)" + newLine +
				@"PLUS := ('+' Ignorable?)" + newLine +
				@"OPEN := ('(' Ignorable?)" + newLine +
				@"CLOSE := (')' Ignorable?)" + newLine +
				@"Ignorable := (Spacing || LineComment || BlockComment)+" + newLine +
				@"Spacing := /\s+/" + newLine +
				@"LineComment := ('//' (!EndOfLine /./)* EndOfLine)" + newLine +
				@"BlockComment := ('/*' (!'*/' /./)* '*/')" + newLine +
				@"EndOfLine := (/$/ || /\r?\n/)" + newLine;

			Assert.Equal(grammar, ParserCompiler.BuildPegGrammarParser().ToString());
		}


		[Fact]
		public void ShouldIgnoreBlockCommentsInGrammar()
		{
			var p = ParserCompiler.Compile(
				"A := B						/* This is a block comment *\n" +
				"NotARule := NotADefinition  * which spans a few lines *\n" +
				"							 * and includes junk.	   */\n" +
				"B := 'abc'");

			p.Parse("abc");

			Assert.Equal("A := B" + Environment.NewLine + "B := 'abc'" + Environment.NewLine, p.ToString());
		}


		[Fact]
		public void ShouldIgnoreLineCommentsInGrammar()
		{
			var p = ParserCompiler.Compile(
				"A := B // This is a grammar rule\n" +
				"B := 'abc' // This is another grammar rule");

			p.Parse("abc");

			Assert.Equal("A := B" + Environment.NewLine + "B := 'abc'" + Environment.NewLine, p.ToString());
		}


		[Fact]
		public void ShouldRejectSpuriousInputAfterGrammar()
		{
			try {
				ParserCompiler.Compile("A := 'abc' :");
				Assert.True(false, "Invalid grammar was accepted but should have been rejected");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Fact]
		public void ShouldUnescapeBasicLiteralExpressions()
		{
			const string text = "They're \\";

			var p = ParserCompiler.Compile("A := 'They\\'re \\\\'");
			var t = p.Parse(text);

			Assert.Equal(1, t.ChildCount);
			Assert.Equal("They're \\", t[0].MatchedText(text));
		}


		[Fact]
		public void ShouldUnescapeRegexLiteralExpressions()
		{
			const string text = "abc/def\\ghi\\";

			var p = ParserCompiler.Compile("A := /abc\\/def\\\\ghi\\\\/");
			var t = p.Parse(text);

			Assert.Equal(1, t.ChildCount);
			Assert.Equal("abc/def\\ghi\\", t[0].MatchedText(text));
		}
	}
}
