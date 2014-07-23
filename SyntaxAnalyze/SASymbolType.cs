using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxAnalyze
{
    public enum SAGrammmarAnalyzeMode
    {
        Waiting,
        ReadingSymbol,
        ReadingOperator,
        ReadingChar,
        ReadingCharAfterEscape,
        ReadingString,
        ReadingStringAfterEscape,
        ReadingCommand
    }

    public enum SAGrammarItemType
    {
        String,
        Characters,
        Identifier,
        Operator
    }

    public enum SAGrammarParseMode
    {
        Waiting,
        WaitingWithCachedItem,
        ReadingLine
    }
}
