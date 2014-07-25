using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxAnalyze
{
    public class SAGrammar
    {
        public Dictionary<string, SASymbolDefinition> Symbols = new Dictionary<string, SASymbolDefinition>();
        public String MainNode { get; set; }
        public List<String> PropagateNodes = new List<string>();
        public Dictionary<string, HashSet<string>> RemoveNodes = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, HashSet<string>> JointextNodes = new Dictionary<string, HashSet<string>>();
        public List<String> undefinedIdentifiers = new List<string>();


        public SAGrammarParseMode parseMode = SAGrammarParseMode.Waiting;
        public SASymbolDefinition parsedItem = null;
        public SAGrammarItemType cachedItemType;
        public string cachedItem;

        public SASymbolDefinition getSymbolDefinition(string symbolName)
        {
            if (Symbols.ContainsKey(symbolName))
            {
                return Symbols[symbolName];
            }
            else
            {
                throw new Exception("Cannot find definition for symbol " + symbolName + " in grammar.");
            }
        }

        public void addRemoveNodes(string parent, string child)
        {
            if (RemoveNodes.ContainsKey(parent))
            {
                RemoveNodes[parent].Add(child);
            }
            else
            {
                HashSet<string> s = new HashSet<string>();
                s.Add(child);
                RemoveNodes.Add(parent, s);
            }
        }
        public void addJointextNodes(string parent, string child)
        {
            if (JointextNodes.ContainsKey(parent))
            {
                JointextNodes[parent].Add(child);
            }
            else
            {
                HashSet<string> s = new HashSet<string>();
                s.Add(child);
                JointextNodes.Add(parent, s);
            }
        }

        public void checkConsistency()
        {

            foreach (string key in Symbols.Keys)
            {
                SASymbolDefinition sd = Symbols[key];

                foreach (List<SAGrammarSymbol> s in sd.Lines)
                {
                    foreach (SAGrammarSymbol ss in s)
                    {
                        if (ss.Type == SAGrammarItemType.Identifier
                            && !Symbols.ContainsKey(ss.Value))
                        {
                            undefinedIdentifiers.Add(ss.Value);
                        }
                    }
                }
            }
        }
    }
}
