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

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Expression which evaluates a named rule as a nested expression.
	/// </summary>
	
	public class RuleRef : ParseExpression
	{
		readonly string mTargetRuleName;
		ParseRule mTargetRule;


		/// <summary>
		/// Creates a RuleRef which references a rule with the specified name.
		/// </summary>
		
		public RuleRef(string ruleName)
		{
			mTargetRuleName = ruleName;
		}


		/// <summary>
		/// Name of the referenced rule.
		/// </summary>
		
		public string TargetRuleName
		{
			get { return mTargetRuleName; }
		}


		internal override bool CheckWellFormed(string baseRuleName, HashSet<string> ruleNames)
		{
			if (!ruleNames.Add(mTargetRuleName))
				throw new CircularDefinitionException(baseRuleName, mTargetRuleName);

			return mTargetRule.Expression.CheckWellFormed(mTargetRuleName, ruleNames);
		}


		internal override ParseExpression Compile<T>(Parser<T> parser)
		{
			mTargetRule = parser.GetRule(mTargetRuleName);
			if (mTargetRule == null)
				throw new UndefinedConstructException(mTargetRuleName);

			return this;
		}


		internal override bool Parse(ParseContext context, ParseRule rule, out Token result)
		{
			return mTargetRule.Parse(context, out result);
		}


		public override void ToString(StringBuilder text)
		{
			text.Append(mTargetRuleName);
		}
	}
}
