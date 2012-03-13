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

using Naucera.Iambic.Expressions;
using NUnit.Framework;

namespace Naucera.Iambic
{
	[TestFixture]
	public class ParseRuleTest
	{
		[Test]
		public void ShouldInvokeAnnotationDelegateWithParsedToken()
		{
			const string text = "abc";

			var processorInvoked = false;

			var p = new Parser<Token>(
				(token, ctx, args) => token,
				new ParseRule("A", new LiteralTerminal(text)))

				.Tagging("A", with: (token, context, args) => {
						Assert.AreEqual(1, token.ChildCount);
						Assert.AreEqual(text, token[0].MatchedText(text));

						processorInvoked = true;
						return null;
					});

			p.Parse(text);

			Assert.IsTrue(processorInvoked);
		}


		[Test]
		public void ShouldReturnOutputFromConversion()
		{
			var p = new Parser<string>(
				(token, ctx, args) => token.Tag.ToString(),
				new ParseRule("A", new LiteralTerminal("a")))
				.Tagging("A", with: (token, context, args) => "Some value");

			Assert.AreEqual("Some value", p.Parse("a"));
		}
	}
}
