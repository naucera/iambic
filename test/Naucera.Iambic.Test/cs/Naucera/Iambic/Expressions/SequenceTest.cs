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
	public class SequenceTest
	{
		[Fact]
		public void ShouldConvertToGrammarString()
		{
			var expr = new Sequence(
				new LiteralTerminal("a"),
				new LiteralTerminal("b"));

			// Create a parser to ensure the expression is compiled
			new Parser<Token>((token, ctx, args) => token, new ParseRule("A", expr));

			Assert.Equal("('a' 'b')", expr.ToString());
		}


		[Fact]
		public void ShouldMatchSubExpressionsInSequence()
		{
			const string text = "ab";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
					  new LiteralTerminal("a"),
					  new LiteralTerminal("b"))));

			var t = p.Parse(text);

			Assert.Equal(2, t.ChildCount);
			Assert.Equal("a", t[0].MatchedText(text));
			Assert.Equal("b", t[1].MatchedText(text));
		}


		[Fact]
		public void ShouldNotAllowInvalidSubExpression()
		{
			try {
				new Parser<Token>(
					(token, ctx, args) => token,
					new ParseRule("A",
						new Sequence(
							new LiteralTerminal("a"),
							new RuleRef("nonExistantRule"))));

				Assert.True(false, "Invalid subexpression was allowed but should have been rejected");
			}
			catch (UndefinedConstructException e) {
				Assert.Equal("nonExistantRule", e.ConstructName);
			}
		}


		[Fact]
		public void ShouldNotAllowZeroSubExpressions()
		{
			var expr = new Sequence();

			try {
				new Parser<Token>((token, ctx, args) => token, new ParseRule("A", expr));
				Assert.True(false, "Expression without subexpressions was allowed but should have been rejected");
			}
			catch (EmptyCompositeException e) {
				Assert.Equal(expr, e.Expression);
			}
		}


		[Fact]
		public void ShouldNotMatchSubExpressionsOutOfSequence()
		{
			const string text = "ba";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
					  new LiteralTerminal("a"),
					  new LiteralTerminal("b"))));

			try {
				p.Parse(text);
				Assert.True(false, "Expression matched but should not have");
			}
			catch (SyntaxException) {
				// Expected exception
			}
		}


		[Fact]
		public void ShouldProductOneTokenForEachMatchingTerminal()
		{
			const string text = "abc";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new LiteralTerminal("a"),
						new LiteralTerminal("b"),
						new LiteralTerminal("c"))));

			var t = p.Parse(text);

			Assert.Equal(3, t.ChildCount);
			Assert.Equal("a", t[0].MatchedText(text));
			Assert.Equal("b", t[1].MatchedText(text));
			Assert.Equal("c", t[2].MatchedText(text));
		}


		[Fact]
		public void ShouldRecoverFromParseErrorsAtLastSubExpression()
		{
			const string text = "xbc";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new LiteralTerminal("a"),
						new LiteralTerminal("b"),
						new LiteralTerminal("c"))
				)) { MaxErrors = 3 };

			try {
				p.Parse(text);
				Assert.True(false, "Expression matched but should not have");
			}
			catch (SyntaxException e) {
				Assert.Equal(1, e.Context.ErrorCount);
				Assert.Equal(2, e.Result.ChildCount);
				Assert.Equal("b", e.Result[0].MatchedText(text));
				Assert.Equal("c", e.Result[1].MatchedText(text));
			}
		}
	}
}
