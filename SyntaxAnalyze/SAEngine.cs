using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
        private int logLevel = 0;
        public int parsedLength = 0;

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

            ConvertInputToTree(request); 

            return true;
        }

        public void printLogLevel()
        {
            Debugger.Log(0, "", "".PadLeft(logLevel));
        }

        public SAGrammar ConvertTextToGrammar(string str)
        {
            SAGrammmarAnalyzeMode mode = SAGrammmarAnalyzeMode.Waiting;
            StringBuilder symbol = new StringBuilder();
            SAGrammar grammar = new SAGrammar();
            int unicodeCharsCounter = 0;
            StringBuilder unicodeValue = new StringBuilder();

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
                    else if (c == '#')
                    {
                        symbol.Clear();
                        mode = SAGrammmarAnalyzeMode.ReadingCommand;
                    }
                }
                else if (mode == SAGrammmarAnalyzeMode.ReadingCommand)
                {
                    if (c == ';')
                    {
                        AcceptCommand(symbol.ToString(), grammar);
                        mode = SAGrammmarAnalyzeMode.Waiting;
                    }
                    else
                    {
                        symbol.Append(c);
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
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Operator);
                        symbol.Clear();
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
                    if (c == 'n')
                    {
                        symbol.Append('\n');
                        mode = SAGrammmarAnalyzeMode.ReadingChar;
                    }
                    else if (c == 't')
                    {
                        symbol.Append('\t');
                        mode = SAGrammmarAnalyzeMode.ReadingChar;
                    }
                    else if (c == 'u')
                    {
                        unicodeCharsCounter = 4;
                        unicodeValue.Clear();
                        mode = SAGrammmarAnalyzeMode.ReadingUnicodeChar;
                    }
                    else if (c == 'U')
                    {
                        unicodeCharsCounter = 8;
                        unicodeValue.Clear();
                        mode = SAGrammmarAnalyzeMode.ReadingUnicodeChar;
                    }
                    else
                    {
                        symbol.Append(c);
                        mode = SAGrammmarAnalyzeMode.ReadingChar;
                    }
                }
                else if (mode == SAGrammmarAnalyzeMode.ReadingUnicodeChar)
                {
                    if (c == '\\')
                    {
                        AppendUnicodeToSymbol(symbol, unicodeValue);
                        mode = SAGrammmarAnalyzeMode.ReadingCharAfterEscape;
                    }
                    else if (c == '\'')
                    {
                        AppendUnicodeToSymbol(symbol, unicodeValue);
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Characters);
                        mode = SAGrammmarAnalyzeMode.Waiting;
                    }
                    else
                    {
                        if (unicodeCharsCounter <= 1)
                        {
                            unicodeValue.Append(c);
                            mode = SAGrammmarAnalyzeMode.ReadingChar;
                            AppendUnicodeToSymbol(symbol, unicodeValue);
                        }
                        else
                        {
                            unicodeValue.Append(c);
                            unicodeCharsCounter--;
                        }
                    }
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
                    if (c == 'n')
                    {
                        symbol.Append('\n');
                        mode = SAGrammmarAnalyzeMode.ReadingString;
                    }
                    else if (c == 't')
                    {
                        symbol.Append('\t');
                        mode = SAGrammmarAnalyzeMode.ReadingString;
                    }
                    else if (c == 'u')
                    {
                        unicodeCharsCounter = 4;
                        unicodeValue.Clear();
                        mode = SAGrammmarAnalyzeMode.ReadingUnicodeString;
                    }
                    else if (c == 'U')
                    {
                        unicodeCharsCounter = 8;
                        unicodeValue.Clear();
                        mode = SAGrammmarAnalyzeMode.ReadingUnicodeString;
                    }
                    else
                    {
                        symbol.Append(c);
                        mode = SAGrammmarAnalyzeMode.ReadingString;
                    }
                }
                else if (mode == SAGrammmarAnalyzeMode.ReadingUnicodeString)
                {
                    if (c == '\\')
                    {
                        AppendUnicodeToSymbol(symbol, unicodeValue);
                        mode = SAGrammmarAnalyzeMode.ReadingStringAfterEscape;
                    }
                    else if (c == '\'')
                    {
                        AppendUnicodeToSymbol(symbol, unicodeValue);
                        AcceptItem(grammar, symbol.ToString(), SAGrammarItemType.Characters);
                        mode = SAGrammmarAnalyzeMode.Waiting;
                    }
                    else
                    {
                        if (unicodeCharsCounter <= 1)
                        {
                            unicodeValue.Append(c);
                            mode = SAGrammmarAnalyzeMode.ReadingString;
                            AppendUnicodeToSymbol(symbol, unicodeValue);
                        }
                        else
                        {
                            unicodeValue.Append(c);
                            unicodeCharsCounter--;
                        }
                    }
                }
            }

            return grammar;
        }

        private static void AppendUnicodeToSymbol(StringBuilder symbol, StringBuilder unicodeValue)
        {
            int uc;
            if (int.TryParse(unicodeValue.ToString(), out uc))
            {
                symbol.Append(Convert.ToChar(uc));
            }
        }

        public void AcceptCommand(string cmd, SAGrammar grammar)
        {
            string[] cmdParts = cmd.Split(' ');
            if (cmdParts.Length == 2)
            {
                if (cmdParts[0].Equals("setMain"))
                {
                    grammar.MainNode = cmdParts[1];
                }
                else if (cmdParts[0].Equals("propagate"))
                {
                    grammar.PropagateNodes.Add(cmdParts[1]);
                }
            }
            else if (cmdParts.Length == 4)
            {
                if (cmdParts[0].Equals("remove") && cmdParts[2].Equals("for"))
                {
                    grammar.addRemoveNodes(cmdParts[3], cmdParts[1]);
                }
                else if (cmdParts[0].Equals("jointext") && cmdParts[2].Equals("in"))
                {
                    grammar.addJointextNodes(cmdParts[3], cmdParts[1]);
                }
            }
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
                        if (grammar.Symbols.ContainsKey(grammar.cachedItem))
                        {
                            Debugger.Log(0, "", "Duplicated terms: " + grammar.cachedItem + "\n");
                        }
                        else
                        {
                            grammar.Symbols.Add(grammar.cachedItem, symbol);
                        }
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
                    else if (item.Equals("?"))
                    {
                        grammar.parsedItem.SetMinMaxForLastItem(0,1);
                    }
                    else if (item.Equals("*"))
                    {
                        grammar.parsedItem.SetMinMaxForLastItem(0, int.MaxValue);
                    }
                    else if (item.Equals("+"))
                    {
                        grammar.parsedItem.SetMinMaxForLastItem(1, int.MaxValue);
                    }
                    else if (item.Equals("!"))
                    {
                        grammar.parsedItem.SetNegator(true);
                    }
                    else if (item.Equals("&"))
                    {
                        grammar.parsedItem.setLineUnion();
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
            return Char.IsLetterOrDigit(c) || c == '_' || c=='-';
        }

        public bool IsOperatorChar(char c)
        {
            return ":;|?*+!&".IndexOf(c) >= 0;
        }

        public void ConvertInputToTree(SARequest req)
        {
            SAParseTreeNode parser = new SAParseTreeNode();
            parser.Symbol = new SAGrammarSymbol(req.Grammar.MainNode, SAGrammarItemType.Identifier);
            int pos = 0;
            Debugger.Log(0, "", "-----------------\n");
            logLevel = 0;
            ConvertInputToTreeWithSymbol(req.Grammar, req.InputFileText, parser, ref pos);
            req.OutputTree = parser;

            parsedLength = pos;

            Debugger.Log(0, "", "Position: " + pos + ", InputLength: " + req.InputFileText.Length + "\n");

            parser.jointextNodes(req.Grammar);

            parser.removeNodes(req.Grammar);

            parser.propagateNodes(req.Grammar.PropagateNodes);

            parser.removeSymbolsWithEmptyLines();
        }

        /// <summary>
        /// Based on symbol linked in symbolNode parameter, this functions tries to find 
        /// all lines in symbol definition, that matches input text from given position
        /// </summary>
        /// <param name="g">grammar definition</param>
        /// <param name="inputText">input text</param>
        /// <param name="symbolNode">node in parser tree dedicated for one particular symbol (identifier, chars, string)</param>
        /// <param name="pos">position in input text to start parsing from</param>
        /// <returns>true if at least one line succeeded in matching the input text</returns>
        public bool ConvertInputToTreeWithSymbol(SAGrammar g, string inputText, SAParseTreeNode symbolNode, ref int pos)
        {
            logLevel++;
            SASymbolDefinition def = g.getSymbolDefinition(symbolNode.Symbol.Value);

            int prevPos = pos;
            int linesAdded = 0;
            foreach (SAGrammarLine line in def.Lines)
            {
                pos = prevPos;
                printLogLevel();
                Debugger.Log(0, "", "Reset pos to " + pos + "\n");
                SAParseNodeLine parsedDefinitionLine = new SAParseNodeLine();

                if (CheckLine(g, inputText, ref pos, line, parsedDefinitionLine))
                {
                    parsedDefinitionLine.LastPosition = pos;
                    symbolNode.Lines.Add(parsedDefinitionLine);
                    linesAdded++;
                }
                else
                {
                    parsedDefinitionLine = null;
                }
            }

            symbolNode.updateNextPos();

            logLevel--;
            return linesAdded > 0;
        }

        private bool CheckLine(SAGrammar g, string input, ref int pos, SAGrammarLine line, SAParseNodeLine nli)
        {
            logLevel++;
            bool succ = true;
            int origPos = pos;
            foreach (SAGrammarSymbol gs in line.Symbols)
            {
                // for each symbol in line tries to find as much instances as allowed
                // by (MinOccurences,MaxOccurences) range
                int count = 0;

                // if we have union operator
                // we try to macth all symbols from the beginning
                // in other words, character succesion on input must 
                // match all symbols in this line
                if (line.UnionOperator)
                    pos = origPos;

                for (int tries = 0; tries < gs.MaxOccurences; tries++)
                {
                    SAParseTreeNode ptn = new SAParseTreeNode();
                    ptn.Symbol = gs;
                    ptn.Pos = pos;
                    succ = CheckSymbolFromLine(g, ptn, input, ref pos, nli, gs);
                    if (!succ) break;
                    count++;
                }
                if (!(count >= gs.MinOccurences && count <= gs.MaxOccurences))
                {
                    //ptn.Valid = false;
                    logLevel--;
                    return false;
                }
/*                else
                {
                    //ptn.Valid = true;
                }*/
            }
            logLevel--;
            return true;
        }

        private bool CheckSymbolFromLine(SAGrammar g, SAParseTreeNode ptn, string input, ref int pos, SAParseNodeLine nli, SAGrammarSymbol gs)
        {
            logLevel++;
            bool succ = !gs.Negative;

            if (gs.Type == SAGrammarItemType.Characters)
            {
                if (pos < input.Length)
                {
                    printLogLevel();
                    Debugger.Log(0, "", "Compare char input[" + pos + "]=" + input[pos] + " with gs.Value=" + gs.Value + "\n");
                }
                if (pos < input.Length && gs.Value.IndexOf(input[pos]) >= 0)
                {
                    ptn.Value += input[pos];
                    pos++;
                    nli.Symbols.Add(ptn);
                    printLogLevel();
                    Debugger.Log(0, "", "- success, new pos is:" + pos + "\n");
                }
                else
                {
                    // fail
                    printLogLevel();
                    Debugger.Log(0, "", "-fail\n");
                    succ = gs.Negative;
//                    ptn.Valid = false;
                }
            }
            else if (gs.Type == SAGrammarItemType.String)
            {
                if (pos < input.Length)
                {
                    printLogLevel();
                    Debugger.Log(0, "", "Compare string input[" + pos + "]=" + input[pos] + " with gs.Value=" + gs.Value + "\n");
                }
                if (((pos + gs.Value.Length) <= input.Length)
                    && input.IndexOf(gs.Value, pos, 2) == pos)
                {
                    pos += gs.Value.Length;
                    ptn.Value += gs.Value;
                    nli.Symbols.Add(ptn);
                    printLogLevel();
                    Debugger.Log(0, "", "- success, new pos is:" + pos + "\n");
                }
                else
                {
                    printLogLevel();
                    Debugger.Log(0, "", "-fail\n");
                    succ = gs.Negative;
//                    ptn.Valid = false;
                }
            }
            else if (gs.Type == SAGrammarItemType.Identifier)
            {
                printLogLevel();
                Debugger.Log(0, "", "Trying symbol " + gs.Value + " at position " + pos + "\n");
                ptn.Valid = ConvertInputToTreeWithSymbol(g, input, ptn, ref pos);
                succ = ptn.Valid;
                if (!ptn.Valid)
                {
                    printLogLevel();
                    Debugger.Log(0, "", "Trying symbol " + gs.Value + " -fail\n");
                    //break;
                }
                else
                {
                    nli.Symbols.Add(ptn);
                    printLogLevel();
                    Debugger.Log(0, "", "Trying symbol " + gs.Value + " -success\n");
                    printLogLevel();
                    Debugger.Log(0, "", " New pos = " + ptn.NextPos + "\n");
                    pos = ptn.NextPos;
                }
            }
            else
            {
                throw new Exception("Unknown symbol type in definition");
            }
            logLevel--;
            return succ;
        }

    }
}
