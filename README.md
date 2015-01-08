# StackLang

(working title)

StackLang is a stack-based language. It's relatively easy to use after a bit of getting used to. Nobody expects anyone to work in StackLang, though. It was made as an exercise, and it was a really fun one.

The project is written entirely in C#.

## Project layout

The solution consists of three projects:

1. StackLang.Core: contains the StackLang runtime. Its public API is really easy to use.
2. StackLang.Interpreter: a command-line interpreter for StackLang. Being based on StackLang.Core, it's really small (< 50 LOC)
3. StackLang.Debugger: a command-line debugger for StackLang. Again, being based on StackLang.Core, it's quite small, staying below 200 LOC.
4. StackLang.Ide: a complete IDE, made with MVVM in WPF. It contains a tab-based editor with all the editing functionality one may want, an interpreter with in-app or file-based input/output and a complete debugger (with stepping, stack viewing, watches, breakpoints).

## How to run StackLang

Either build the source (NuGet will update the packages automatically) or download one of the binary releases. For the best experience, run StackLang.Ide; if you would rather run the console apps, use the StackLang.Interpreter or StackLang.Debugger executables.

## How to use the StackLang.Core API:

1. Create an instance of `StackLang.Core.Parser`, supplying a `System.Io.Stream` as a code source.
2. Create an instance of `StackLang.Core.ExecutionContext`, supplying the result of `Parser.Parse()` as the first parameter. The second and the third parameters (defining the program input and output), will default to calls to `Console.WriteLine` and `Console.ReadLine`. If you want to read or write from a stream, use the `StackLang.Core.StreamInputManager` or the `StackLang.Core.StreamOutputManager` classes. A `StackLang.Core.Exceptions.ParseException` may be thrown.
3. If parsing is successful, call `ExecutionContext.Tick` until either `ExecutionContext.ExecutionEnded` is true or a `StackLang.Core.Exceptions.CodeException` is thrown. If you want to debug, get a `StackLang.Core.ExecutionSnapshot` with `ExecutionContext.Parameters.GetSnapshot` and inspect it.

## Coding help

Instructions are separated by whitespace (spaces, tabs, newlines). Comments start with `:` and affect the whole line after them.

### Arithmetic and IO:

* Numbers are pushed on the stack as-is.
* Operators (arithmetic, boolean or bitwise) are executed immediately.

That means that the code `2 2 +` will leave the value `4` on the stack.

* `<<` will read a number from the input manager. `>>` will pop a value on the stack and print it to the output manager.

That means that the code `2 2 + >>` will print `4`.

### Variables

* `r` is the register variable: a special built-in variable. Use only for short runs, there is only one and it WILL get reused.
* `=` will pop 2 values from the stack, and it will assign the value of the **second** to the **first**. The first must be a variable. Take note, the **first** value is the value **on top** of the stack and the second is the one **below** it.

That means that the code `2 r =` will assign the value `2` to the register. Take note, the code `r 2 =` will fail to execute. The code `r >>` will print the value currently on the register.

* The language has 1024 memory slots. Access them with `m0` through `m1023`.

That means that the code `<< m0 =` will read a value from the input manager and assign it to the first memory slot. The code `m10 1 + >>` will add `1` to the value stored in `m10` and print the result. The code `m1 1 + m1 =` will get the value in `m1`, add `1` to it and save the result in `m1`, effectively **incrementing** `m1`.

### Execution manipulation

* `p` will pop a value from the stack, discarding it.
* Instructions starting with `k` and a number, like `k10`, will jump to that line of code.
* Instructions starting with `k+` or `k-` and a number, like `k+2` or `k-3` will jump down (or up) that many lines of code.

### Execution sources

* The execution can run either from the code or the stack. Normally, there are no instructions on the stack, but they can be pushed to the stack instead of being executed if the execution is **escaped**.
* Escaping is done with the `\` instruction for only one instruction or `\\` for the rest of the line.

That means that the code `1 2 \ k2` will end with the stack looking like `1, 2, k2`, the jump instruction will **not** be executed. The code `1 2 \\ k2 k3 r` will end with the stack looking like `1, 2, k2, k3, r`, none of the jump instructions will be executed (the registry instruction is not executed but placed on the stack anyway).

* If you want to set the execution source **to the stack**, use the `;` instruction. If you want to set the execution source **to the code**, use the `k` instruction (**without any numbers** to resume execution from where it ended or with some sort of **line reference** to also jump).

That means that the code `\\ k+2 3 >> ;` will **first** print `3` and then jump **2 lines of code down** (as an explanation, the stack will first be `k+2, 3, >>` and then the execution source will be **set to stack** by the `;` instruction, then the `>>` instruction will execute first, printing the **top** stack value, which is `3`, and then `k+2` will be executed, jumping 2 lines down and **setting the execution source back to code**.

### Control flow

* `.` is a no-op, it does nothing. (it may come in handy later)
* `if` will **pop** and **evaluate** the top value on the stack. If it's **evaluated to 0**, the instruction will *pop two values off the stack**. Otherwise, it does nothing. While it doesn't seem too useful, it is critical to allowing alternatives in code, or, as we know it, `if`-like behavior.

#### If structure

The way to write an if in StackLang is:

````
[condition]
r = \\ k+[block size + 2] p k r if ;
	[instructions in block]
;
[instructions after if]
````

This is an if instruction **with no else block**. For example:

````
<<
r = \\ k+3 p k r if ;
	2 >>
;
3 >>
	
````

To write an if instruction **with an else block**, use

````
[condition]
r = \\ k+[total block size + 3] k+[true block size + 2] p k r if ;
	[instructions in true block]
;
	[instructions in false block]
;
[instructions after block]
````

The first jump is the "end jump" (it will jump to after the if and the else), the second jump is the "false jump" (it will jump to the else block) and the third jump (or only the 'k' instruction) is the "true jump" (it will jump, or not, to the if block). For example:

````
<<
r = \\ k+5 k+3 p k r if ;
	2 >>
;
	3 >>
;
0 >>
````

The way these work is (you can also fire up the debugger in the IDE to see for yourself):

1. Save the condition (which should be on the stack) to the register.
2. Put all the jump instructions on the stack
3. Put the register on the stack, then the `if` instruction, then set the execution source to stack.
4. **If the value is true**, simply continue executing from the next line (where the instruction pointer is already)
5. After the block has been executed, run from the stack again **to clean up the stack** (this is very important, otherwise the rest of the code may not work.
6. In the version **without else**, the next instruction on the stack is the one to jump after the block, so it's executed. In the version **with else**, the stack looks like (for example) `k+5, k+3, p`. The false jump instruction is popped off the stack so the end jump instruction is executed.
7. **If the value is false**, the if instruction will pop off the true jump instruction and the pop instruction off the stack, so the stack will look like (for example) `k+5, k+3`. The false jump instruction will be executed.
8. After the else block has been executed, we again clean up the stack by executing it. The last instruction is the end jump, so we end up at the end of the block.

#### While structure

The while structure is similar. The way to write a while in StackLang is:

````


````

## License:

This is licensed under the MIT license.