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

using Xunit;

namespace Naucera.Iambic.Expressions
{
	public class NotMatchTest
	{
		[Fact]
		public void ShouldConvertToGrammarString()
		{
			var expr = new NotMatch(new LiteralTerminal("a"));

			Assert.Equal("!'a'", expr.ToString());
		}


		[Fact]
		public void ShouldMatchWithoutConsumingInput()
		{
			const string text = "ab";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new NotMatch(new LiteralTerminal("b")),
						new LiteralTerminal("ab"))
				));

			p.Parse(text);
		}


		[Fact]
		public void ShouldNotAllowSubexpressionToRecoverFromParseError()
		{
			const string text = "ab";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A", new NotMatch(new LiteralTerminal("a"))))
				{ MaxErrors = 2 };

			try {
				p.Parse(text);
				Assert.True(false, "Expression matched but should not have");
			}
			catch (SyntaxException e) {
				Assert.Equal(1, e.Context.ErrorCount);
				Assert.Equal(0, e.Result.ChildCount);
			}
		}


		[Fact]
		public void ShouldNotMatchIfSubexpressionMatches()
		{
			const string text = "b";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A", new NotMatch(new LiteralTerminal("b"))));

			try {
				p.Parse(text);
				Assert.True(false, "Expression matched when it should not have");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Fact]
		public void ShouldNotProduceTokenForMatch()
		{
			const string text = "ab";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new NotMatch(new LiteralTerminal("b")),
						new LiteralTerminal("ab"))
				));

			var t = p.Parse(text);

			Assert.Equal(1, t.ChildCount);
			Assert.Equal("ab", t[0].MatchedText(text));
		}
	}
}
