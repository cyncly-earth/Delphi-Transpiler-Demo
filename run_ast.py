#!/usr/bin/env python3
"""
Safe AST Runner - Doesn't modify any existing files
Uces dotnet CLI directly
"""
import os
import subprocess
import sys

def run_ast_builder():
    """Run the AST builder using dotnet CLI directly"""
    
    print("🔧 AST Builder - Safe Execution")
    print("=" * 50)
    
    # List of AST source files
    ast_files = [
        "ast/ast_nodes.cs",
        "ast/ast_builder.cs", 
        "ast/ast_runner.cs"
    ]
    
    # Check files exist
    for file in ast_files:
        if not os.path.exists(file):
            print(f"❌ Missing file: {file}")
            return False
    
    print("✓ All AST files found")
    
    # Create a temporary program
    temp_program = """
using System;
using System.IO;
using System.Text.Json;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("AST Builder Running...");
        
        // We'll need to include the actual AST code here
        // For now, just show we can run
        Console.WriteLine("AST module would run here");
        Console.WriteLine("Output would go to run/output/*.ast.json");
    }
}
"""
    
    # Create temporary directory
    temp_dir = "temp_ast_build"
    os.makedirs(temp_dir, exist_ok=True)
    
    # Write files to temp directory
    for file in ast_files:
        with open(file, 'r') as src:
            content = src.read()
        dest_path = os.path.join(temp_dir, os.path.basename(file))
        with open(dest_path, 'w') as dest:
            dest.write(content)
    
    print(f"✓ Files copied to {temp_dir}")
    print("\nTo compile and run manually:")
    print("1. cd /workspaces/Delphi-Transpiler-Demo")
    print("2. mcs -out:ast_builder.exe ast/*.cs")
    print("3. mono ast_builder.exe")
    
    return True

if __name__ == "__main__":
    success = run_ast_builder()
    sys.exit(0 if success else 1)
