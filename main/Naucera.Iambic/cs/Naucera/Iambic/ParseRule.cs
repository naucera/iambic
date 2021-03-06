#region license

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
using System.Text;
using Naucera.Iambic.Expressions;

namespace Naucera.Iambic
{
	/// <summary>
	/// Single named parsing grammar rule.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>
	/// Each ParseRule should only be added to one and only one Parser. It is an
	/// error to add a given ParseRule to more than one Parser.</para>
	/// 
	/// <para>
	/// ParseRules will most often be generated from a grammar specification
	/// using the ParserCompiler rather than being created manually.</para>
	/// </remarks>

	public sealed class ParseRule : GrammarConstruct
	{
		ParseExpression mExpression;


		/// <summary>
		/// Creates a ParseRule with the specified name and expression.
		/// </summary>
		/// 
		/// <param name="name">
		/// Grammar construct name for this rule.</param>
		/// 
		/// <param name="expr">
		/// Grammar expression matched by this rule.</param>

		public ParseRule(string name, ParseExpression expr) : base(name)
		{
			mExpression = expr;
		}


		/// <summary>
		/// Grammar expression which is matched by this rule.
		/// </summary>
		
		public ParseExpression Expression
		{
			get { return mExpression; }
		}


		/// <summary>
		/// Checks for well-formedness by ensuring that no left-recursion loops
		/// exist in the expression, throwing an exception if so.
		/// </summary>
		/// 
		/// <exception cref="InvalidGrammarException">
		/// Thrown if the grammar is not well-formed.</exception>
		
		internal void CheckWellFormed()
		{
			var ruleNames = new HashSet<string> { Name };
			mExpression.CheckWellFormed(Name, ruleNames);
		}


		/// <summary>
		/// Compiles this rule for its parser.
		/// </summary>
		/// 
		/// <param name="parser">
		/// Parser to compile for.</param>

		internal void Compile<T>(Parser<T> parser)
		{
			mExpression = mExpression.Compile(parser);
		}


		internal bool Parse(ParseContext context, out Token result)
		{
			var useCache = !context.Recovering;

			// Attempt to fetch a previously cached result if we are not
			// recovering from a parse error.
			if (useCache) {
				var cached = context.UseCachedResult(this);
				if (cached != null) {
					result = cached.Result;
					return cached.Accepted;
				}
			}

			var startOffset = context.Offset;
			Token token;
			var accepted = mExpression.Parse(context, this, out token);

			// Decorate the result if parsing succeeded
			if (!context.HasErrors && accepted) {
				var endOffset = context.Offset;
				context.Offset = startOffset;

				Token res;
				context.Accept(this, out res);
				res.EndOffset = token.EndOffset;
				res.Add(token);
				token = res;

				context.Offset = endOffset;
			}

			// Cache the parsed result
			if (useCache && context.MarkedError == null)
				context.CacheResult(this, startOffset, accepted, token);

			result = token;
			return accepted;
		}


		/// <summary>
		/// Returns the grammar specification representing this parse rule.
		/// </summary>
		/// 
		/// <returns>
		/// Grammar specification string.</returns>
		
		public override string ToString()
		{
			var text = new StringBuilder();
			ToString(text);

			return text.ToString();
		}


		internal void ToString(StringBuilder text)
		{
			text.Append(Name);
			text.Append(" := ");
			mExpression.ToString(text);
		}
	}
}
