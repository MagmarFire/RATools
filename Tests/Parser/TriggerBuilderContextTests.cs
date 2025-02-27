﻿using Jamiras.Components;
using NUnit.Framework;
using RATools.Data;
using RATools.Parser;
using RATools.Parser.Functions;
using RATools.Parser.Internal;

namespace RATools.Test.Parser
{
    [TestFixture]
    class TriggerBuilderTests
    {
        private static ExpressionBase Parse(string input)
        {
            return ExpressionBase.Parse(new PositionalTokenizer(Tokenizer.CreateTokenizer(input)));
        }

        [Test]
        [TestCase("1", "v1")]
        [TestCase("1 + 7", "v8")]
        [TestCase("1 + 3 * 2", "v7")]
        [TestCase("byte(0x1234)", "0xH001234")]
        [TestCase("byte(0x1234) * 10", "0xH001234*10")]
        [TestCase("byte(0x1234) / 10", "0xH001234*0.1")]
        [TestCase("byte(0x1234) * 10 / 3", "0xH001234*3.33333333333333")]
        [TestCase("byte(0x1234) + 10", "0xH001234_v10")]
        [TestCase("byte(0x1234) - 10", "0xH001234_v-10")]
        [TestCase("(byte(0) + byte(1)) * 10", "0xH000000*10_0xH000001*10")]
        [TestCase("(byte(0) + 2) * 10", "0xH000000*10_v20")]
        [TestCase("(byte(0) + byte(1)) / 10", "0xH000000*0.1_0xH000001*0.1")]
        [TestCase("byte(0x1234) * 2", "0xH001234*2")]
        [TestCase("byte(0x1234) / 2", "0xH001234*0.5")]
        [TestCase("byte(0x1234) * 100 / 2", "0xH001234*50")]
        [TestCase("byte(0x1234) * 2 / 100", "0xH001234*0.02")]
        [TestCase("byte(0x1234) + 100 - 2", "0xH001234_v98")]
        [TestCase("byte(0x1234) + 1 - 1", "0xH001234")]
        [TestCase("byte(0x1234) * 2 + 1", "0xH001234*2_v1")]
        [TestCase("byte(0x1234) * 2 - 1", "0xH001234*2_v-1")]
        [TestCase("byte(0x1234) * 256 + byte(0x2345) + 1", "0xH001234*256_0xH002345_v1")]
        [TestCase("(byte(0x1234) / (2 * 20)) * 100", "0xH001234*2.5")]
        [TestCase("byte(0x1234) * byte(0x2345)", "M:0xH001234*0xH002345")]
        [TestCase("byte(0x1234) / byte(0x2345)", "M:0xH001234/0xH002345")]
        [TestCase("byte(0x1234 + byte(0x2345))", "I:0xH002345_M:0xH001234")]
        [TestCase("byte(0x1234 + byte(0x2345)) + 1", "I:0xH002345_A:0xH001234_M:1")]
        [TestCase("byte(0x1234 + byte(0x2345)) + byte(0x1235 + byte(0x2345))", "I:0xH002345_A:0xH001234_I:0xH002345_M:0xH001235")]
        [TestCase("byte(0x1234 + byte(0x2345)) + byte(0x1235 + byte(0x2345)) + 1", "I:0xH002345_A:0xH001234_I:0xH002345_A:0xH001235_M:1")]
        [TestCase("byte(0x1234 + byte(0x2345)) + 1 + byte(0x1235 + byte(0x2345))", "I:0xH002345_A:0xH001234_A:1_I:0xH002345_M:0xH001235")]
        [TestCase("1 + byte(0x1234 + byte(0x2345))", "I:0xH002345_A:0xH001234_M:1")]
        [TestCase("byte(0x1234 + byte(0x2345)) - 1", "B:1_I:0xH002345_M:0xH001234")]
        [TestCase("byte(0x1234 + byte(0x2345)) - byte(0x1235 + byte(0x2345))", "I:0xH002345_B:0xH001235_I:0xH002345_M:0xH001234")]
        [TestCase("byte(0x1234 + byte(0x2345)) * 2", "I:0xH002345_M:0xH001234*2")]
        [TestCase("measured(byte(0x1234) != prev(byte(0x1234))", "M:0xH001234!=d0xH001234")]
        public void TestGetValueString(string input, string expected)
        {
            ExpressionBase error;
            InterpreterScope scope = new InterpreterScope(AchievementScriptInterpreter.GetGlobalScope());
            scope.Context = new TriggerBuilderContext();

            var expression = Parse(input);
            var result = TriggerBuilderContext.GetValueString(expression, scope, out error);
            Assert.That(error, Is.Null);
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
