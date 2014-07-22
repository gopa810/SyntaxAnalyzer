using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxAnalyze
{
    /// <summary>
    /// ENgine for parsing
    /// 
    /// symbol  : def | def1 def2 ;
    /// symbol2 : 'c' | '\'';
    /// </summary>
    public class SAEngine
    {
        private SARequest inputData = null;

        public SARequest Request
        {
            get
            {
                return inputData;
            }
            set
            {
                inputData = value;
            }
        }

        public bool Process(SARequest request)
        {
            Request = request;

            request.Grammar = ConvertTextToGrammar(request.GrammarText);

            return true;
        }

        public SAGrammar ConvertTextToGrammar(string str)
        {
            SAGrammmarAnalyzeMode mode = SAGrammmarAnalyzeMode.Waiting;
            StringBuilder symbol = new StringBuilder();
            SAGrammar grammar = new SAGrammar();

            foreach (char c in str)
            {
                if (mode == SAGrammmarAnalyzeMode.Waiting)
                {
                    if (IsIdentifierChar(c))
                    {
                        symbol.Clear();
                        symbol.Append(c);
                        mode = SAGrammmarAnalyzeMode.ReadingSymbol;
                    }
                    else if (IsOperatorChar(c))
                    {
                        symbol.Clear();
                        symbol.Append(c);
                        mode = SAGrammmarAnalyzeMode.ReadingOperator;
                    }
                    else if (c == '\'')
                    {
                        symbol.Clear();
                        mode = SAGrammmarAnalyzeMode.ReadingChar;
                    }
                    else if (c == '\"')
                    {
                        symbol.Clear();
                        mode = SAGrammmarAnalyzeMode.ReadingString;
                    }
                }
                else if (mode == SAGrammmarAnalyzeMode.ReadingOperator)
                {
                    if (IsIdentifierChar(c))
                    {
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Operator);
                        symbol.Clear();
                        symbol.Append(c);
                        mode = SAGrammmarAnalyzeMode.ReadingSymbol;
                    }
                    else if (IsOperatorChar(c))
                    {
                        symbol.Append(c);
                    }
                    else if (c == '\'')
                    {
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Operator);
                        symbol.Clear();
                        mode = SAGrammmarAnalyzeMode.ReadingChar;
                    }
                    else if (c == '\"')
                    {
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Operator);
                        symbol.Clear();
                        mode = SAGrammmarAnalyzeMode.ReadingString;
                    }
                    else
                    {
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Operator);
                        symbol.Clear();
                        mode = SAGrammmarAnalyzeMode.Waiting;
                    }
                }
                else if (mode == SAGrammmarAnalyzeMode.ReadingSymbol)
                {
                    if (IsIdentifierChar(c))
                    {
                        symbol.Append(c);
                    }
                    else if (IsOperatorChar(c))
                    {
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Identifier);
                        symbol.Clear();
                        symbol.Append(c);
                        mode = SAGrammmarAnalyzeMode.ReadingOperator;
                    }
                    else if (c == '\'')
                    {
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Identifier);
                        symbol.Clear();
                        mode = SAGrammmarAnalyzeMode.ReadingChar;
                    }
                    else if (c == '\"')
                    {
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Identifier);
                        symbol.Clear();
                        mode = SAGrammmarAnalyzeMode.ReadingString;
                    }
                    else
                    {
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Identifier);
                        symbol.Clear();
                        mode = SAGrammmarAnalyzeMode.Waiting;
                    }
                }
                else if (mode == SAGrammmarAnalyzeMode.ReadingChar)
                {
                    if (c == '\\')
                    {
                        mode = SAGrammmarAnalyzeMode.ReadingCharAfterEscape;
                    }
                    else if (c == '\'')
                    {
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Characters);
                        mode = SAGrammmarAnalyzeMode.Waiting;
                    }
                    else
                    {
                        symbol.Append(c);
                    }
                }
                else if (mode == SAGrammmarAnalyzeMode.ReadingCharAfterEscape)
                {
                    symbol.Append(c);
                    mode = SAGrammmarAnalyzeMode.ReadingChar;
                }
                else if (mode == SAGrammmarAnalyzeMode.ReadingString)
                {
                    if (c == '\\')
                    {
                        mode = SAGrammmarAnalyzeMode.ReadingStringAfterEscape;
                    }
                    else if (c == '\"')
                    {
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.String);
                        mode = SAGrammmarAnalyzeMode.Waiting;
                    }
                    else
                    {
                        symbol.Append(c);
                    }
                }
                else if (mode == SAGrammmarAnalyzeMode.ReadingStringAfterEscape)
                {
                    symbol.Append(c);
                    mode = SAGrammmarAnalyzeMode.ReadingChar;
                }
            }

            return grammar;
        }

        public void AcceptItem(SAGrammar grammar, string item, SAGrammarItemType type)
        {
            if (grammar.parseMode == SAGrammarParseMode.Waiting)
            {
                grammar.cachedItem = item;
                grammar.cachedItemType = type;
                grammar.parseMode = SAGrammarParseMode.WaitingWithCachedItem;
            }
            else if (grammar.parseMode == SAGrammarParseMode.WaitingWithCachedItem)
            {
                if (type == SAGrammarItemType.Operator && item.Equals(":"))
                {
                    if (grammar.cachedItemType == SAGrammarItemType.Identifier)
                    {
                        SASymbolDefinition symbol = new SASymbolDefinition();
                        grammar.Symbols.Add(grammar.cachedItem, symbol);
                        grammar.parsedItem = symbol;
                        grammar.parseMode = SAGrammarParseMode.ReadingLine;
                    }
                }
            }
            else if (grammar.parseMode == SAGrammarParseMode.ReadingLine)
            {
                if (type == SAGrammarItemType.Operator)
                {
                    if (item.Equals(";"))
                    {
                        grammar.parseMode = SAGrammarParseMode.Waiting;
                    }
                    else if (grammar.parsedItem != null)
                    {
                        grammar.parsedItem.AddItem(item, type);
                    }
                }
                else
                {
                    grammar.parsedItem.AddItem(item, type);
                }
            }
        }

        public bool IsIdentifierChar(char c)
        {
            return Char.IsLetterOrDigit(c) || c == '_' || c=='-' || c=='+';
        }

        public bool IsOperatorChar(char c)
        {
            return ":;|".IndexOf(c) >= 0;
        }

    }
}
