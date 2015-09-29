# Named Parsers #

A parser can be named, so that if one of its sub-parsers fails, the top-level name will be reported as an expectation:

```
var p = Parse.LetterOrDigit.AtLeastOnce().Named("identifier");
```

# Exclusive-Or #

If two parsers are in an `Or` relationship, the default behaviour will be to try the first, then on failure, try the second.

In this situation, if the first parser partially succeeded, the error message will still be generated at the point where the `Or` parser could examine both results. This means that for parser:

```
var type = Class.Or(Interface);
```

With input:

```
class Some%Problem
```

The generated message will be "unexpected 'c'; expected class or interface".

To improve this, the parser can be told that "if the first option consumes anything, report its error and don't try the second parser". This is done with `XOr`.

```
var type = Class.XOr(Interface);
```

Using the XOr combinator, the failure will be reported as "unexpected '%'; expected letter or digit".

Note that in order for XOr to work, there must be no ambiguity after parsing a single character.

# Exclusive-Many #

Like `XOr`, `XMany` will report a failure on any partial match. This works for lists of items that are followed by an unambiguous terminator (e.g. `End`).

```
var bottles = Bottle.XMany().End();
```

This way, if parsing fails midway through a bottle, the error will be reported at that point rather than at the start of the failing bottle as it would with `Many`.