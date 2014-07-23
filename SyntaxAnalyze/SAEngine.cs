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
                        symbol.Append('\n');
                    else if (c == 't')
                        symbol.Append('\t');
                    else
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
                    if (c == 'n')
                        symbol.Append('\n');
                    else if (c == 't')
                        symbol.Append('\t');
                    else
                        symbol.Append(c);
                    mode = SAGrammmarAnalyzeMode.ReadingChar;
                }
            }

            return grammar;
        }

        public void AcceptCommand(string cmd, SAGrammar grammar)
        {
            if (cmd.StartsWith("setMain="))
            {
                grammar.MainNode = cmd.Substring(8).Trim();
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
            return ":;|?*+".IndexOf(c) >= 0;
        }

        public void ConvertInputToTree(SARequest req)
        {
            SAParseTreeNode parser = new SAParseTreeNode();
            parser.Symbol = new SAGrammarSymbol(req.Grammar.MainNode, SAGrammarItemType.Identifier);
            int pos = 0;
            Debugger.Log(0, "", "-----------------\n");
            ConvertInputToTreeWithSymbol(req.Grammar, req.InputFileText, parser, ref pos);
            req.OutputTree = parser;
        }

        public bool ConvertInputToTreeWithSymbol(SAGrammar g, string input, SAParseTreeNode trySymbol, ref int pos)
        {
            SASymbolDefinition def;
            if (g.Symbols.ContainsKey(trySymbol.Symbol.Value))
            {
                def = g.Symbols[trySymbol.Symbol.Value];
            }
            else
            {
                throw new Exception("Cannot find definition for symbol " + trySymbol.Symbol.Value + " in grammar.");
            }

            int prevPos = pos;
            foreach (List<SAGrammarSymbol> line in def.Lines)
            {
                pos = prevPos;
                Debugger.Log(0, "", "Reset pos to " + pos + "\n");
                SAParseNodeLine nli = new SAParseNodeLine();

                if (CheckLine(g, input, ref pos, line, nli))
                {
                    nli.LastPosition = pos;
                    trySymbol.Lines.Add(nli);
                }
                else
                {
                    nli = null;
                }
            }

            int maxPos = -1;
            SAParseNodeLine maxNode = null;
            foreach (SAParseNodeLine ln2 in trySymbol.Lines)
            {
                if (maxPos < ln2.LastPosition)
                {
                    maxNode = ln2;
                    maxPos = ln2.LastPosition;
                }
            }

            if (maxNode != null)
            {
                trySymbol.Lines.Clear();
                trySymbol.Lines.Add(maxNode);
                trySymbol.NextPos = maxPos;
            }

            return trySymbol.Lines.Count > 0;
        }

        private bool CheckLine(SAGrammar g, string input, ref int pos, List<SAGrammarSymbol> line, SAParseNodeLine nli)
        {
            bool succ = true;
            foreach (SAGrammarSymbol gs in line)
            {
                // for each symbol in line tries to find as much instances as allowed
                // by (MinOccurences,MaxOccurences) range
                int count = 0;
                
                SAParseTreeNode ptn = new SAParseTreeNode();
                ptn.Symbol = gs;
                ptn.Pos = pos;
                nli.Symbols.Add(ptn);

                for (int tries = 0; tries < gs.MaxOccurences; tries++)
                {
                    succ = CheckSymbolFromLine(g, ptn, input, ref pos, nli, gs);
                    if (!succ) break;
                    count++;
                }
                if (!(count >= gs.MinOccurences && count <= gs.MaxOccurences))
                {
                    ptn.Valid = false;
                    return false;
                }
                else
                {
                    ptn.Valid = true;
                }
            }

            return true;
        }

        private bool CheckSymbolFromLine(SAGrammar g, SAParseTreeNode ptn, string input, ref int pos, SAParseNodeLine nli, SAGrammarSymbol gs)
        {
//            SAParseTreeNode ptn = new SAParseTreeNode();
            bool succ = true;

            if (gs.Type == SAGrammarItemType.Characters)
            {
                if (pos < input.Length)
                    Debugger.Log(0, "", "Compare char input[" + pos + "]=" + input[pos] + " with gs.Value=" + gs.Value + "\n");
                if (pos < input.Length && gs.Value.IndexOf(input[pos]) >= 0)
                {
                    ptn.Value += input[pos];
                    pos++;
                    Debugger.Log(0, "", "- success, new pos is:" + pos + "\n");
                }
                else
                {
                    // fail
                    Debugger.Log(0, "", "-fail\n");
                    succ = false;
                    ptn.Valid = false;
                }
            }
            else if (gs.Type == SAGrammarItemType.String)
            {
                if (pos < input.Length)
                    Debugger.Log(0, "", "Compare string input[" + pos + "]=" + input[pos] + " with gs.Value=" + gs.Value + "\n");
                if (((pos + gs.Value.Length) <= input.Length)
                    && input.IndexOf(gs.Value, pos, 2) == pos)
                {
                    pos += gs.Value.Length;
                    ptn.Value += gs.Value;
                    Debugger.Log(0, "", "- success, new pos is:" + pos + "\n");
                }
                else
                {
                    Debugger.Log(0, "", "-fail\n");
                    succ = false;
                    ptn.Valid = false;
                }
            }
            else if (gs.Type == SAGrammarItemType.Identifier)
            {
                Debugger.Log(0, "", "Trying symbol " + gs.Value + " at position " + pos + "\n");
                ptn.Valid = ConvertInputToTreeWithSymbol(g, input, ptn, ref pos);
                succ = ptn.Valid;
                if (!ptn.Valid)
                {
                    Debugger.Log(0, "", "-fail\n");
                    //break;
                }
                else
                {
                    Debugger.Log(0, "", "-success\n");
                    Debugger.Log(0, "", " New pos = " + ptn.NextPos + "\n");
                    pos = ptn.NextPos;
                }
            }
            else
            {
                throw new Exception("Unknown symbol type in definition");
            }

            return succ;
        }

    }
}
