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

using NUnit.Framework;

namespace Naucera.Iambic.Expressions
{
	[TestFixture]
	public class ZeroOrMoreTest
	{
		[Test]
		public void ShouldConvertToGrammarString()
		{
			var expr = new ZeroOrMore(new LiteralTerminal("a"));

			Assert.AreEqual("'a'*", expr.ToString());
		}


		[Test]
		public void ShouldMatchSubExpressionOnce()
		{
			const string text = "ab";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new ZeroOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
				));

			p.Parse(text);
		}


		[Test]
		public void ShouldMatchSubExpressionRepeatedly()
		{
			const string text = "aaab";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new ZeroOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
					));

			p.Parse(text);
		}


		[Test]
		public void ShouldMatchSubExpressionZeroTimes()
		{
			const string text = "b";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new ZeroOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
				));

			p.Parse(text);
		}


		[Test]
		public void ShouldNotProduceTokenIfMatchedZeroTimes()
		{
			const string text = "b";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new ZeroOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
				));

			var t = p.Parse(text);

			Assert.AreEqual(1, t.ChildCount);
			Assert.AreEqual("b", t[0].MatchedText(text));
		}


		[Test]
		public void ShouldProduceOneTokenPerTimeMatched()
		{
			const string text = "aaab";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new ZeroOrMore(new LiteralTerminal("a")),
						new LiteralTerminal("b"))
				));

			var t = p.Parse(text);

			Assert.AreEqual(4, t.ChildCount);
			Assert.AreEqual("a", t[0].MatchedText(text));
			Assert.AreEqual("a", t[1].MatchedText(text));
			Assert.AreEqual("a", t[2].MatchedText(text));
			Assert.AreEqual("b", t[3].MatchedText(text));
		}


		[Test]
		public void ShouldNotLoopForeverIfSubExpressionSuccessfullyMatchesBlank()
		{
			const string text = "b";

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A",
					new Sequence(
						new ZeroOrMore(new ZeroOrMore(new LiteralTerminal("a"))),
						new LiteralTerminal("b"))
				));

			p.Parse(text);
		}
	}
}