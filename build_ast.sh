#!/bin/bash
# AST Builder Script - Doesn't affect main project

echo "=== Building AST Module ==="

# Create temporary project file
cat > ast_temp.csproj << 'TEMPEOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="ast/ast_nodes.cs" />
    <Compile Include="ast/ast_builder.cs" />
    <Compile Include="ast/ast_runner.cs" />
  </ItemGroup>

</Project>
TEMPEOF

echo "1. Building..."
dotnet build ast_temp.csproj

if [ $? -eq 0 ]; then
    echo -e "\n2. Running AST Builder..."
    dotnet run --project ast_temp.csproj
else
    echo "Build failed!"
    exit 1
fi

# Clean up
rm -f ast_temp.csproj
