# Human Parser Generator

A simple Parser Generator with a focus on "human" code generation    
Christophe VG (<contact@christophe.vg>)  
[https://github.com/christophevg/cs-parser-generator](https://github.com/christophevg/cs-parser-generator)

# Introduction

Although many parser generators exist, I feel like there is room for one more, which generates a parser in a more "human" way.

The objectives are:

* start from an EBNF-like notation, e.g. allow copy pasting existing grammars and (maybe almost) be done with it.
* generate code, as if it were written by a human developer:
	* generate functional classes to construct the AST
	* generate parser logic that is readable and understandable
* be self hosting: the project should be able to generate itself.

The project will initially target C#. No other target language is planned at this point.

**Disclaimer** I'm not aiming for feature completeness and only add support for what I need at a given time ;-)

## Current Status

As of February 18, I tagged the repository `v1.0`. At that point I felt that the current code base was capable of fulfilling the objectives I had set forward at the beginning:

* A trivial example of a small subset of the Pascal language can be parsed.
* The generator is capable of generating a parser for its own EBNF-like definition language, which means its self-hosting (see also below for more information on this feature). 
* A parser for a more complex grammar for Cobol record definitions (aka Copybooks) is capable of parsing a set of example Copybooks and outputs a nice corresponding parser.

Since then I've started working on improving things that didn't turn out the way I wanted or expected:

* The emitted generator became less _human_ with more complex grammars, such as the one for Cobol Copybooks. Especially the nested `try { ... } catch { ... }` blocks were no longer nice on the eye and hard to read. So I started implementing an inner-DSL. See below for more info.
* Error reporting was not so "useful". Towards `v1.1` I'm aiming for much improved error reporting. See below for more info.

## Example

The following example is taken from [the Wikipedia page on EBNF](https://en.wikipedia.org/wiki/Extended_Backus–Naur_form):

```ebnf
(* a simple program syntax in EBNF − Wikipedia *)
 program = 'PROGRAM', white space, identifier, white space, 
            'BEGIN', white space, 
            { assignment, ";", white space }, 
            'END.' ;
 identifier = alphabetic character, { alphabetic character | digit } ;
 number = [ "-" ], digit, { digit } ;
 string = '"' , { all characters - '"' }, '"' ;
 assignment = identifier , ":=" , ( number | identifier | string ) ;
 alphabetic character = "A" | "B" | "C" | "D" | "E" | "F" | "G"
                      | "H" | "I" | "J" | "K" | "L" | "M" | "N"
                      | "O" | "P" | "Q" | "R" | "S" | "T" | "U"
                      | "V" | "W" | "X" | "Y" | "Z" ;
 digit = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" ;
 white space = ? white space characters ? ;
 all characters = ? all visible characters ? ;
```

This grammar would allow to parse

```pascal
 PROGRAM DEMO1
 BEGIN
   A:=3;
   B:=45;
   H:=-100023;
   C:=A;
   D123:=B34A;
   BABOON:=GIRAFFE;
   TEXT:="Hello world!";
 END.
```

### Changes, Simplifications, Extensions,...

To get a head-start I've added few changes/extensions/limitations to the standard EBNF notation. The following grammar is a rewritten version of the earlier Pascal example, using these extensions:

```ebnf
program               ::= "PROGRAM" identifier
                          "BEGIN"
                          { assignment }
                          "END."
                        ;

assignment            ::= identifier ":=" expression ";" ;

expression            ::= identifier
                        | string
                        | number
                        ;

identifier            ::= name  @ /([A-Z][A-Z0-9]*)/ ;
string                ::= text  @ /"([^"]*)"|'([^']*)'/ ;
number                ::= value @ /(-?[1-9][0-9]*)/ ;
```

The extensions that are applied are:

* abandoned `,` (colon) in sequences of consecutive expressions
* spaces in rule names (left hand side) are not allowed (e.g. use dashes)
* ignoring whitespace, removing the need for explicit whitespace description
* definition of "extracting terminals" using regular expressions
* introduction of _implicit_ "virtual" entities, which don't show up in the AST (`expression` is an example)

## Demos

A few demos show the capabilities and results of the generated parsers.

> I'm running on macOS with [Mono](http://mono-project.com). Mono provides an implementation of `msbuild` in the form of `xbuild`. I've recently moved from `Makefiles` to project build files. This is still to be tested on Windows with the original `msbuild`, so for now YMMV in any environment other than macOS ;-)

### Pascal

In the [`example/pascal`](example/pascal) folder I've started by writing a manual implementation for the embryonal Pascal example, [`pascal.cs`](example/pascal/pascal.cs), taking into account how I think this could be generated. The output of the example program, parses the example Pascal file and outputs an AST-like structure.

```bash
$ cd example/pascal

$ xbuild /target:Manual
XBuild Engine Version 14.0
Mono, Version 4.6.2.0
Copyright (C) 2005-2013 Various Mono authors

Build started 2/23/2017 12:37:29 AM.
__________________________________________________
Project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/example/pascal/pascal.csproj" (Manual target(s)):
	Target Manual:
		Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/pascal-manual.exe ../../generator/parsable.cs ../../generator/dump-ast.cs pascal-manual.cs /target:exe
		Executing: mono bin/Debug/pascal-manual.exe example.pascal | LC_ALL="C" astyle -s2 -xt0 -xe -Y -xC80
		new Program() {
		  Identifier = new Identifier() { Name = "DEMO1"},
		  Assignments = new List<Assignment>() {
		    new Assignment() {
		      Identifier = new Identifier() { Name = "A"},
		      Expression = new Number() {
		        Value = "3"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "B"},
		      Expression = new Number() {
		        Value = "45"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "H"},
		      Expression = new Number() {
		        Value = "-100023"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "C"},
		      Expression = new Identifier() {
		        Name = "A"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "D123"},
		      Expression = new Identifier() {
		        Name = "B34A"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "BABOON"},
		      Expression = new Identifier() {
		        Name = "GIRAFFE"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "TEXT"},
		      Expression = new String() {
		        Text = "Hello world!"
		      }
		    }
		  }
		}
Done building project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/example/pascal/pascal.csproj".

Build succeeded.
	 0 Warning(s)
	 0 Error(s)

Time Elapsed 00:00:00.7789880
```

> The manual implementation uses two supporting files, [parsable.cs](generator/parsable.cs) and [dump-ast.cs](generator/dump-ast.cs), that are now part of the generator framework. They were "promoted" from the manual example to the actual framework.

> I've chosen to implement the `ToString()` functionality of the AST as C# code that builds the same data structure. This is easy to reuse it, e.g. in unit tests and it formats nicely (using e.g. AStyle).

The build file also implements an example on how to generate a Pascal generator from the EBNF-like language definition. In fact, just issuing `xbuild` runs the manual implementation, generates a fresh Pascal parser, compares the manual and generated parser and finally also runs the generated version:

```bash
$ xbuild
XBuild Engine Version 14.0
Mono, Version 4.6.2.0
Copyright (C) 2005-2013 Various Mono authors

Build started 2/23/2017 12:36:53 AM.
__________________________________________________
Project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/example/pascal/pascal.csproj" (default target(s)):
	Target Manual:
		Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/pascal-manual.exe ../../generator/parsable.cs ../../generator/dump-ast.cs pascal-manual.cs /target:exe
		Executing: mono bin/Debug/pascal-manual.exe example.pascal | LC_ALL="C" astyle -s2 -xt0 -xe -Y -xC80
		new Program() {
		  Identifier = new Identifier() { Name = "DEMO1"},
		  Assignments = new List<Assignment>() {
		    new Assignment() {
		      Identifier = new Identifier() { Name = "A"},
		      Expression = new Number() {
		        Value = "3"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "B"},
		      Expression = new Number() {
		        Value = "45"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "H"},
		      Expression = new Number() {
		        Value = "-100023"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "C"},
		      Expression = new Identifier() {
		        Name = "A"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "D123"},
		      Expression = new Identifier() {
		        Name = "B34A"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "BABOON"},
		      Expression = new Identifier() {
		        Name = "GIRAFFE"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "TEXT"},
		      Expression = new String() {
		        Text = "Hello world!"
		      }
		    }
		  }
		}
	Target Generated:
		Project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/hpg.csproj" (default target(s)):
			Target Gen0Parser:
				Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/hpg.gen0.exe generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/grammar.cs generator/bootstrap.cs /target:exe
			Target Gen1Source:
				Executing: mono bin/Debug/hpg.gen0.exe generator/hpg.bnf > generator/parser.gen1.cs
			Target Gen1Parser:
				Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/hpg.gen1.exe generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/parser.gen1.cs generator/hpg.cs /target:exe
			Target HPGSource:
				Executing: mono bin/Debug/hpg.gen1.exe generator/hpg.bnf > generator/parser.cs
			Target Build:
				Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/hpg.exe generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/parser.cs generator/hpg.cs /target:exe
		Done building project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/hpg.csproj".
		Executing: mono ../../bin/Debug//hpg.exe -i pascal.bnf | LC_ALL="C" astyle -s2 -xt0 -xe -Y -xC80 > pascal.cs
	Target Compare:
		Executing: diff -u -w  pascal-manual.cs pascal.cs
	Target BuildGenerated:
		Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/parse-pascal.exe ../../generator/parsable.cs ../../generator/dump-ast.cs pascal.cs /target:exe
	Target RunGenerated:
		Executing: mono bin/Debug/parse-pascal.exe example.pascal | LC_ALL="C" astyle -s2 -xt0 -xe -Y -xC80
		new Program() {
		  Identifier = new Identifier() { Name = "DEMO1"},
		  Assignments = new List<Assignment>() {
		    new Assignment() {
		      Identifier = new Identifier() { Name = "A"},
		      Expression = new Number() {
		        Value = "3"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "B"},
		      Expression = new Number() {
		        Value = "45"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "H"},
		      Expression = new Number() {
		        Value = "-100023"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "C"},
		      Expression = new Identifier() {
		        Name = "A"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "D123"},
		      Expression = new Identifier() {
		        Name = "B34A"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "BABOON"},
		      Expression = new Identifier() {
		        Name = "GIRAFFE"
		      }
		    },new Assignment() {
		      Identifier = new Identifier() { Name = "TEXT"},
		      Expression = new String() {
		        Text = "Hello world!"
		      }
		    }
		  }
		}
Done building project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/example/pascal/pascal.csproj".

Build succeeded.
	 0 Warning(s)
	 0 Error(s)

Time Elapsed 00:00:03.0725190
```

### Cobol Example

In [`example/cobol`](example/cobol) a parser is generated for Cobol record definitions, also known as Copybooks - Warning: this is work in progress. The generated parser is currently capable of parsing several example Copybooks.

```bash
$ cd examples/cobol

$ xbuild
XBuild Engine Version 14.0
Mono, Version 4.6.2.0
Copyright (C) 2005-2013 Various Mono authors

Build started 2/23/2017 12:40:14 AM.
__________________________________________________
Project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/example/cobol/cobol.csproj" (default target(s)):
	Target Generated:
		Project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/hpg.csproj" (default target(s)):
			Target Gen0Parser:
				Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/hpg.gen0.exe generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/grammar.cs generator/bootstrap.cs /target:exe
			Target Gen1Source:
				Executing: mono bin/Debug/hpg.gen0.exe generator/hpg.bnf > generator/parser.gen1.cs
			Target Gen1Parser:
				Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/hpg.gen1.exe generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/parser.gen1.cs generator/hpg.cs /target:exe
			Target HPGSource:
				Executing: mono bin/Debug/hpg.gen1.exe generator/hpg.bnf > generator/parser.cs
			Target Build:
				Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/hpg.exe generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/parser.cs generator/hpg.cs /target:exe
		Done building project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/hpg.csproj".
		Executing: mono ../../bin/Debug//hpg.exe  cobol.bnf | LC_ALL="C" astyle -s2 -xt0 -xe -Y -xC80 > cobol.cs
		hpg-emitter: warning: rewriting property name: float
		hpg-emitter: warning: rewriting property name: float
		hpg-emitter: warning: rewriting property name: string
		hpg-emitter: warning: rewriting property name: string
		hpg-emitter: warning: rewriting property name: float
		hpg-emitter: warning: rewriting property name: string
	Target RunTests:
		Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/test.dll ../../generator/parsable.cs ../../generator/dump-ast.cs cobol.cs test_records.cs /target:library /reference:nunit.framework.dll
		Executing: nunit-console -nologo bin/Debug/test.dll
		....
		Tests run: 4, Failures: 0, Not run: 0, Time: 0.109 seconds
	Target BuildGenerated:
		Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/parse-cobol.exe ../../generator/parsable.cs ../../generator/dump-ast.cs cobol.cs /target:exe
	Target RunGenerated:
		Executing: mono bin/Debug/parse-cobol.exe example.cobol | LC_ALL="C" astyle -s2 -xt0 -xe -Y -xC80
		new Copybook() {
		  Records = new List<Record>() {
		    new BasicRecord() {
		      Level = new Int() { Value = "01"},
		      LevelName = new LevelName() {
		        HasFiller = this.HasFiller,
		        Identifier = new Identifier() {
		          Name = "TOP"
		        }
		      },
		      Options = new List<Option>() {}
		    },new BasicRecord() {
		      Level = new Int() { Value = "05"},
		      LevelName = new LevelName() {
		        HasFiller = this.HasFiller,
		        Identifier = new Identifier() {
		          Name = "SUB"
		        }
		      },
		      Options = new List<Option>() {}
		    },new BasicRecord() {
		      Level = new Int() { Value = "10"},
		      LevelName = new LevelName() {
		        HasFiller = this.HasFiller,
		        Identifier = new Identifier() {
		          Name = "FIELD"
		        }
		      },
		      Options = new List<Option>() {
		        new PictureFormatOption() {
		          Type = "S9",
		          Digits = new Int() { Value = "05"},
		          DecimalType = null,
		          DecimalDigits = null
		        },new CompUsage() {
		          Level = "5"
		        }
		      }
		    }
		  }
		}
Done building project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/example/cobol/cobol.csproj".

Build succeeded.
	 0 Warning(s)
	 0 Error(s)

Time Elapsed 00:00:04.7227060
```

The build file also provides a more visible demo that parses a small fragment of a simple Cobol copybook:

```cobol
01 TOP.
  05 SUB.
    10 FIELD   PIC S9(05) COMP-5.
```

The AST is shown in the output above.

> As shown by the output from the examples, the generator first generates two copies of itself, before the examples use it to generate a Pascal or Cobol parser. This is the implementation of the *self-hosting objective*.

## Being Self Hosting

An important aspect of this project is being self hosting and parsing the EBNF-like grammars with a parser that is generated by the parser generator itself. The following diagram shows what I mean by this; it shows the 9 steps to get from *no parser* to a fully generated second generation EBNF-like parser, that can be used to generate a parser for a different language, e.g. Pascal:

![Bootstrapping HPG](assets/hpg-bootstrap.png)

1. Human encoding of EBNF to loadable C# code
2. Importing of the Grammar and transformation to a Parser model
3. Emission of Generation 1 EBNF Parser
4. Using Generation 1 Parser, parsing of EBNF definition into Grammar parse tree
5. Importing of the Grammar and transformation to a Parser model
6. Emission of Generation 2 EBNF Parser
7. Using Generation 2 Parser, parsing of Pascal definition into Grammar parse tree
8. Importing of the Grammar and transformation to a Parser model
9. Emission of Pascal Parser

## Grammar

The grammar for the Human Parser Generator BNF-like notation (currently) looks like this:

```ebnf
grammar                   ::= { rule } ;

rule                      ::= identifier "::=" expression ";" ;

expression                ::= sequential-expression
                            | non-sequential-expression
                            ;

sequential-expression     ::= non-sequential-expression expression ;

non-sequential-expression ::= alternatives-expression
                            | atomic-expression
                            ;

alternatives-expression   ::= atomic-expression "|" non-sequential-expression ;


atomic-expression         ::= nested-expression
                            | terminal-expression
                            ;

nested-expression         ::= optional-expression
                            | repetition-expression
                            | group-expression
                            ;

optional-expression       ::= "[" expression "]" ;
repetition-expression     ::= "{" expression "}" ;
group-expression          ::= "(" expression ")" ;

terminal-expression       ::= identifier-expression
                            | string-expression
                            | extractor-expression
                            ;

identifier-expression     ::= [ name ] identifier ;
string-expression         ::= [ name ] string;

extractor-expression      ::= [ name ] "/" pattern "/" ;

name                      ::= identifier "@" ;

identifier                ::= /([A-Za-z_][A-Za-z0-9-_]*)/ ;
string                    ::= /"([^"]*)"|^'([^']*)'/ ;
pattern                   ::= /(.*?)(?<keep>/\s*;)/ ;
```

To bootstrap the generator, to allow it to generate a parser for the EBNF-like definition language, a grammar modelled by hand is used. It is located in `generator/grammar.cs` in the `AsModel` class, retrievable via the `BNF` property.

The model is a direct implementation of this EBNF-like definition in object-oriented structures and looks like this:

![Grammar Model](model/grammar.png)

Not shown in the model are `identifier`, `string` and `pattern`. These lowest-level, extracting entities (aka extractors), are not generated as classes, but as (static) prepared regular expressions, and used when needed.

## Generator

The generator accepts a Grammar Model, which is basically an Abstract Syntax Tree (AST), and first transforms this, to a Parser Model. This model is a rewritten version of the Grammar Model and is designed to facilitate the emission of the actual Parser code.

The structure of the Generator (currently) looks like this:

![Parser Model](model/generator.png)

### Building `hpg.exe`

Building the generator simply requires an `xbuild` command in the root of the repository - which is what is also done by the Pascal and Cobol examples above:

```bash
$ xbuild
XBuild Engine Version 14.0
Mono, Version 4.6.2.0
Copyright (C) 2005-2013 Various Mono authors

Build started 2/22/2017 9:11:12 PM.
__________________________________________________
Project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/hpg.csproj" (default target(s)):
	Target Gen0Parser:
		Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/hpg.gen0.exe generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/grammar.cs generator/bootstrap.cs /target:exe
	Target Gen1Source:
		Executing: mono bin/Debug/hpg.gen0.exe generator/hpg.bnf > generator/parser.gen1.cs
	Target Gen1Parser:
		Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/hpg.gen1.exe generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/parser.gen1.cs generator/hpg.cs /target:exe
	Target HPGSource:
		Executing: mono bin/Debug/hpg.gen1.exe generator/hpg.bnf > generator/parser.cs
	Target Build:
		Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/hpg.exe generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/parser.cs generator/hpg.cs /target:exe
Done building project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/hpg.csproj".

Build succeeded.
	 0 Warning(s)
	 0 Error(s)

Time Elapsed 00:00:01.7712950
```

This compiles a second generation parser generator, called `hpg.exe`:

```bash
$ mono bin/Debug/hpg.exe --help
Human Parser Generator version 1.1.6262.38847
Usage: hpg.exe [options] [file ...]

    --help, -h              Show usage information

Output options.
Select one of the following:
    --parser, -p            Generate parser (DEFAULT)
    --ast, -a               Show AST
    --model, -m             Show parser model
    --grammar, -g           Show grammar
Formatting options.
    --text, -t              Generate textual output (DEFAULT).
    --dot, -d               Generate Graphviz/Dot format output. (model)
Emission options.
    --info, -i              Suppress generation of info header
    --rule, -r              Suppress generation of rule comment
    --namespace, -n NAME    Embed parser in namespace
```

Providing it with an EBNF-like language definition, will simple generate a corresponding parser to standard output:

```bash
$ mono bin/Debug/hpg.exe example/pascal/pascal.bnf
// DO NOT EDIT THIS FILE
// This file was generated using the Human Parser Generator
// (https://github.com/christophevg/human-parser-generator)
// on Thursday, February 23, 2017 at 12:35:26 AM
// Source : example/pascal/pascal.bnf

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

// program ::= "PROGRAM" identifier "BEGIN" { assignment } "END." ;
public class Program {
public Identifier Identifier { get; set; }
public List<Assignment> Assignments { get; set; }
  public Program() {
...
```

> If no `file` is provided, input is read from standard input.

#### Graphviz/Dot support

The parser model can be dumped and formatted in [Graphviz/Dot](http://graphviz.org) format, which is often an great tool while restructuring/rewriting the EBNF to obtain a nicer class model.

The output is generated, so it isn't as attractive as manually laid out diagrams, but still very useful. A few examples:

#### ENBF
![HPG Grammar Model](assets/hpg.bnf.png)

#### Pascal
![Pascal Model](assets/pascal.bnf.png)

#### Cobol
![Cobol Model](assets/cobol.bnf.png)

### Test Driven Development

To be able to focus on sub-problems and isolate combinations of constructs, I've set up a unit testing infrastructure that generates a EBNF-like parser and then uses that to run the tests:

```bash
$ xbuild /target:RunTests
XBuild Engine Version 14.0
Mono, Version 4.6.2.0
Copyright (C) 2005-2013 Various Mono authors

Build started 2/22/2017 9:12:48 PM.
__________________________________________________
Project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/hpg.csproj" (RunTests target(s)):
	Target Gen0Parser:
		Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/hpg.gen0.exe generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/grammar.cs generator/bootstrap.cs /target:exe
	Target Gen1Source:
		Executing: mono bin/Debug/hpg.gen0.exe generator/hpg.bnf > generator/parser.gen1.cs
	Target Gen1Parser:
		Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/hpg.gen1.exe generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/parser.gen1.cs generator/hpg.cs /target:exe
	Target HPGSource:
		Executing: mono bin/Debug/hpg.gen1.exe generator/hpg.bnf > generator/parser.cs
	Target RunTests:
		Tool /Library/Frameworks/Mono.framework/Versions/4.6.2/lib/mono/4.5/mcs.exe execution started with arguments:  /debug+ /out:bin/Debug/test.dll generator/parsable.cs generator/generator.cs generator/emitter.csharp.cs generator/emitter.bnf.cs generator/format.csharp.cs generator/AssemblyInfo.cs generator/parser.cs test/test_bnf_parser.cs test/test_factory.cs test/test_model.cs test/test_parsable.cs /target:library /reference:nunit.framework.dll
		Executing: nunit-console -nologo bin/Debug/test.dll
		..............................
		Tests run: 30, Failures: 0, Not run: 0, Time: 0.329 seconds
Done building project "/Users/xtof/Workspace/2know/bellekens/cobol-copybook-parser/human-parser-generator/hpg.csproj".

Build succeeded.
	 0 Warning(s)
	 0 Error(s)

Time Elapsed 00:00:02.8446630
```

> The unit tests hardly cover the basics of the source tree, but the goal is to have a comprehensive set, covering all possibilities. The unit tests will be the driving force for the continued development :-)

### An inner-(parsing)-DSL

After `v1.0` I decided to improve the generated parsers, especially for more complex grammars. A few wrapper functions later this _old_ code

```csharp
public Program ParseProgram() {
  Identifier identifier = null;
  List<Assignment> assignments = new List<Assignment>();
  this.Log("ParseProgram");
  int pos = this.source.position;
  try {
    this.source.Consume("PROGRAM");
    identifier = this.ParseIdentifier();
    this.source.Consume("BEGIN");
    {
      Assignment temp;
      while(true) {
        try {
          temp = this.ParseAssignment();
        } catch(ParseException) {
          break;
        }
        assignments.Add(temp);
      }
    }
    this.source.Consume("END.");
  } catch(ParseException e) {
    this.source.position = pos;
    throw this.source.GenerateParseException(
      "Failed to parse Program.", e
    );
  }
  return new Program() {
    Identifier  = identifier,
    Assignments = assignments
  };
}
```

... was turned into ...

```csharp
public Program ParseProgram() {
  Identifier identifier = null;
  List<Assignment> assignments = new List<Assignment>();
  this.Log( "ParseProgram" );
  Parse( () => {
    Consume("PROGRAM");
    identifier = ParseIdentifier();
    Consume("BEGIN");
    assignments = Many<Assignment>(ParseAssignment);
    Consume("END.");
  }).OrThrow("Failed to parse Program.");
  return new Program() {
    Identifier = identifier,
    Assignments = assignments
  };
}
```

Although it violates a few of my personal code style rules, in this case the inner-DSL is dominant and requires as little as possible _normal_ C# code ;-) The first results of this change are nice. I'm now continuing to investigate possibilities down this road.

### Error Reporting

Another aspect I'm working on is improving the Error Reporting. Currently the new code is already a lot more expressive. A few examples:

```
Parsing failed, best effort parser error:
Failed to parse Assignment. at line 6/7
7 : D123:=B34A .
────╯
```

```
Parsing failed, best effort parser error:
Failed to parse Option. at line 3/70
3 : 10 LL-VELD                             PIC S9(04) COMP-5    ;           
────────────────────────────────────────────────────────────╯
```
