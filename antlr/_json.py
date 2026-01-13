
import json
from antlr4 import InputStream, CommonTokenStream, FileStream
from antlr4.tree.Tree import TerminalNodeImpl

# If you generated into antlr/gen, adjust the import path
from gen.MyGrammarLexer import MyGrammarLexer
from gen.MyGrammarParser import MyGrammarParser

def node_to_json(tree, parser, include_tokens=False, compact=False):
    """
    Convert an ANTLR parse tree to JSON.
    - compact=True → terminals become plain strings
    - include_tokens=True → include token type/line/column for terminals
    """
    # Terminal node
    if isinstance(tree, TerminalNodeImpl):
        tok = tree.getSymbol()
        if compact:
            return tok.text
        obj = {"type": "terminal", "text": tok.text}
        if include_tokens and tok:
            # symbolicNames includes token names by index
            tname = parser.symbolicNames[tok.type] if tok.type < len(parser.symbolicNames) else str(tok.type)
            obj["token"] = {"type": tname, "line": tok.line, "column": tok.column}
        return obj

    # Rule node
    rule_index = tree.getRuleIndex()
    rule_name = parser.ruleNames[rule_index] if rule_index is not None else "unknown"
    children = [node_to_json(tree.getChild(i), parser, include_tokens, compact)
                for i in range(tree.getChildCount())]

    return {"type": "rule", "rule": rule_name, "text": tree.getText(), "children": children}

def parse_string_to_json(text, entry_rule="start", include_tokens=False, compact=False):
    """
    Parse an input string using MyGrammar and return JSON.
    Set entry_rule to your grammar's top-level rule name.
    """
    chars = InputStream(text)
    lexer = MyGrammarLexer(chars)
    tokens = CommonTokenStream(lexer)
    parser = MyGrammarParser(tokens)
    # Dynamically call the entry rule by name
    parse_fn = getattr(parser, entry_rule, None)
    if parse_fn is None:
        raise AttributeError(f"Entry rule '{entry_rule}' not found in MyGrammarParser. "
                             f"Choose one of: {parser.ruleNames}")
    tree = parse_fn()
    return node_to_json(tree, parser, include_tokens, compact)

def parse_file_to_json(path, entry_rule="start", include_tokens=False, compact=False):
    fs = FileStream(path, encoding='utf-8')
    lexer = MyGrammarLexer(fs)
    tokens = CommonTokenStream(lexer)
    parser = MyGrammarParser(tokens)
    parse_fn = getattr(parser, entry_rule, None)
    if parse_fn is None:
        raise AttributeError(f"Entry rule '{entry_rule}' not found in MyGrammarParser. "
                             f"Choose one of: {parser.ruleNames}")
    tree = parse_fn()
    return node_to_json(tree, parser, include_tokens, compact)

if __name__ == "__main__":
    # Replace 'start' with your actual entry rule (e.g., 'program', 'file', 'compilationUnit')
    entry = "start"
    sample = "your minimal valid input here"
    print(json.dumps(parse_string_to_json(sample, entry_rule=entry, include_tokens=True, compact=False), indent=2))
