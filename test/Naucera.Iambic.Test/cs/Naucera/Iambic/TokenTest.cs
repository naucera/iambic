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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Naucera.Iambic.Expressions;

namespace Naucera.Iambic
{
	public class TokenTest
	{
		[Fact]
		public void IteratingChildrenShouldReturnAllChildren()
		{
			const string text = "abc";

			var p = new Parser<Token>((token, ctx, args) => token, new ParseRule("A", new ZeroOrMore(new PatternTerminal("[a-z]"))));
			var t = p.Parse(text);

			var count = 0;

			foreach (var child in t.Children) {
				Assert.Equal(text[count].ToString(), child.MatchedText(text));
				++count;
			}

			Assert.Equal(text.Length, count);
		}


		[Fact]
		public void IteratingChildTagsShouldReturnAllTags()
		{
			const string text = "aaa";

			var p = new Parser<IEnumerable<string>>(
				(token, ctx, args) => (IEnumerable<string>)token.Tag,
				new ParseRule("A", new ZeroOrMore(new RuleRef("B"))),
				new ParseRule("B", new LiteralTerminal("a")))
				.Tagging("A", with: token => token.ChildTags.OfType<string>())
				.Tagging("B", with: (token, ctx) => ctx.MatchedText(token));

			var values = p.Parse(text);

			var count = 0;

			foreach (var v in values) {
				Assert.Equal("a", v);
				++count;
			}

			Assert.Equal(text.Length, count);
		}


		[Fact]
		public void ShouldEscapeTextForXml()
		{
			const string text = "<Apple & Pear>";

			var p = new Parser<Token>((token, ctx, args) => token, new ParseRule("A", new LiteralTerminal(text)));
			var t = p.Parse(text);

			Assert.Equal("&lt;Apple &amp; Pear&gt;", t[0].ToXml(text));
		}


		[Fact]
		public void ShouldExtractValueFromParsedText()
		{
			const string text = "<Apple & Pear>";

			var p = new Parser<Token>((token, ctx, args) => token, new ParseRule("A", new LiteralTerminal(text)));
			var t = p.Parse(text);

			Assert.Equal("<Apple & Pear>", t.MatchedText(text));
		}


		[Fact]
		public void ShouldFormatXmlRespectingSignificantWhitespace()
		{
			const string grammar =
				"Expression := Term Plus Term || Term " +
				"Term := Value '*' Value || Value " +
				"Value := /\\d+/ " +
				"Plus := '+'";
			
			const string text = "1+2*3";

			var expected = new StringBuilder()
				.AppendLine("<Expression>")
				.AppendLine("  <Term>")
				.AppendLine("    <Value>1</Value>")
				.AppendLine("  </Term>")
				.AppendLine("  <Plus>+</Plus>")
				.AppendLine("  <Term><Value>2</Value>*<Value>3</Value></Term>")
				.Append("</Expression>");

			var p = ParserCompiler.Compile(grammar);
			var t = p.Parse(text);

			Assert.Equal(expected.ToString(), t.ToXml(text));
		}
	}
}
